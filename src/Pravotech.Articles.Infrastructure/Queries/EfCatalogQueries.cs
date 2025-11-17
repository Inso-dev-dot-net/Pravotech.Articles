using Microsoft.EntityFrameworkCore;
using Pravotech.Articles.Application.Abstractions;
using Pravotech.Articles.Application.Contracts.Articles;
using Pravotech.Articles.Application.Contracts.Sections;
using Pravotech.Articles.Domain.Services;
using Pravotech.Articles.Infrastructure.Persistence;

namespace Pravotech.Articles.Infrastructure.Queries;

/// <summary>
/// Реализация ICatalogQueries на EF Core
/// </summary>
internal sealed class EfCatalogQueries : ICatalogQueries
{
    private readonly ArticlesDbContext _db;
    private readonly ISectionKeyService _sectionKeyService;

    private sealed record ArticleTagRow(
        Guid ArticleId,
        string Title,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? UpdatedAtUtc,
        string TagName,
        string TagNameNormalized,
        int Position);

    /// <summary>
    /// Создаёт новый экземпляр EfCatalogQueries
    /// </summary>
    /// <param name="db">Контекст БД</param>
    /// <param name="sectionKeyService">Сервис идентификаторов разделов</param>
    public EfCatalogQueries(
        ArticlesDbContext db,
        ISectionKeyService sectionKeyService)
    {
        _db = db;
        _sectionKeyService = sectionKeyService;
    }

    /// <summary>
    /// Возвращает список разделов каталога
    /// </summary>
    public async Task<IReadOnlyList<SectionDto>> GetSectionsAsync(
        CancellationToken ct = default)
    {
        List<ArticleTagRow> rows = await LoadArticleTagRowsAsync(ct);

        if (rows.Count == 0)
        {
            return Array.Empty<SectionDto>();
        }

        var articles = GroupRowsByArticle(rows);

        var articleWithSectionInfo = articles
            .Select(a => BuildArticleSectionInfo(a.TagsOrdered, a.ArticleId, a.Title, a.CreatedAtUtc, a.UpdatedAtUtc))
            .Where(x => x.SectionId != Guid.Empty && x.TagNamesNormalized.Count > 0)
            .ToList();

        if (articleWithSectionInfo.Count == 0)
        {
            return Array.Empty<SectionDto>();
        }

        List<SectionDto> sections = articleWithSectionInfo
            .GroupBy(x => x.SectionId)
            .Select(g => BuildSectionDto(g.Key, g))
            .OrderByDescending(s => s.ArticlesCount)
            .ThenBy(s => s.Name)
            .ToList();

        return sections;
    }

    /// <summary>
    /// Возвращает список статей в выбранном разделе
    /// </summary>
    /// <param name="sectionId">Идентификатор раздела</param>
    public async Task<IReadOnlyList<ArticleDto>> GetSectionArticlesAsync(
        Guid sectionId,
        CancellationToken ct = default)
    {
        if (sectionId == Guid.Empty)
        {
            return Array.Empty<ArticleDto>();
        }

        List<ArticleTagRow> rows = await LoadArticleTagRowsAsync(ct);

        if (rows.Count == 0)
        {
            return Array.Empty<ArticleDto>();
        }

        var articles = GroupRowsByArticle(rows);

        var articlesInSection = articles
            .Select(a =>
            {
                Guid computedSectionId = ComputeSectionId(
                    a.TagsOrdered.Select(t => t.TagNameNormalized));

                return new
                {
                    a.ArticleId,
                    a.Title,
                    a.CreatedAtUtc,
                    a.UpdatedAtUtc,
                    a.TagsOrdered,
                    SectionId = computedSectionId
                };
            })
            .Where(x => x.SectionId == sectionId)
            .ToList();

        if (articlesInSection.Count == 0)
        {
            return Array.Empty<ArticleDto>();
        }

        List<ArticleDto> result = articlesInSection
            .Select(a =>
            {
                DateTimeOffset sortTime = GetSortTime(a.CreatedAtUtc, a.UpdatedAtUtc);

                List<string> tagNamesInOrder = a.TagsOrdered
                    .Select(t => t.TagName)
                    .ToList();

                ArticleDto dto = new ArticleDto
                {
                    Id = a.ArticleId,
                    Title = a.Title,
                    CreatedAtUtc = a.CreatedAtUtc,
                    UpdatedAtUtc = a.UpdatedAtUtc,
                    Tags = tagNamesInOrder
                };

                return new
                {
                    SortTime = sortTime,
                    Dto = dto
                };
            })
            .OrderByDescending(x => x.SortTime)
            .Select(x => x.Dto)
            .ToList();

        return result;
    }

    /// <summary>
    /// Загружает плоский список связок статья - тег из базы
    /// </summary>
    private async Task<List<ArticleTagRow>> LoadArticleTagRowsAsync(CancellationToken ct)
    {
        List<ArticleTagRow> rows = await _db.Articles
            .AsNoTracking()
            .SelectMany(
                article => article.Tags,
                (article, articleTag) => new { article, articleTag })
            .Join(
                _db.Tags.AsNoTracking(),
                x => x.articleTag.TagId,
                tag => tag.Id,
                (x, tag) => new ArticleTagRow(
                    x.article.Id,
                    x.article.Title,
                    x.article.CreatedAtUtc,
                    x.article.UpdatedAtUtc,
                    tag.Name,
                    tag.NameNormalized,
                    x.articleTag.Position))
            .ToListAsync(ct);

        return rows;
    }

    /// <summary>
    /// Группирует строки по статье и восстанавливает упорядоченный список тегов
    /// </summary>
    private static List<(Guid ArticleId, string Title, DateTimeOffset CreatedAtUtc, DateTimeOffset? UpdatedAtUtc, List<(string TagName, string TagNameNormalized)> TagsOrdered)>
        GroupRowsByArticle(List<ArticleTagRow> rows)
    {
        return rows
            .GroupBy(r => new { r.ArticleId, r.Title, r.CreatedAtUtc, r.UpdatedAtUtc })
            .Select(g => (
                g.Key.ArticleId,
                g.Key.Title,
                g.Key.CreatedAtUtc,
                g.Key.UpdatedAtUtc,
                TagsOrdered: g
                    .OrderBy(x => x.Position)
                    .Select(x => (x.TagName, x.TagNameNormalized))
                    .ToList()))
            .ToList();
    }

    /// <summary>
    /// Строит SectionId на основе нормализованных имён тегов
    /// </summary>
    private Guid ComputeSectionId(IEnumerable<string> normalizedTagNames)
    {
        string sectionKey = _sectionKeyService.BuildSectionKey(normalizedTagNames);
        Guid sectionId = _sectionKeyService.BuildSectionId(sectionKey);
        return sectionId;
    }

    /// <summary>
    /// Строит агрегированную информацию по статье и её разделе
    /// </summary>
    private (Guid SectionId,
        Guid ArticleId,
        string Title,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? UpdatedAtUtc,
        List<string> TagNamesOriginal,
        List<string> TagNamesNormalized)
        BuildArticleSectionInfo(
            List<(string TagName, string TagNameNormalized)> tagsOrdered,
            Guid articleId,
            string title,
            DateTimeOffset createdAtUtc,
            DateTimeOffset? updatedAtUtc)
    {
        List<string> normalized = tagsOrdered
            .Select(t => t.TagNameNormalized)
            .ToList();

        Guid sectionId = ComputeSectionId(normalized);

        List<string> original = tagsOrdered
            .Select(t => t.TagName)
            .ToList();

        return (sectionId,
            articleId,
            title,
            createdAtUtc,
            updatedAtUtc,
            original,
            normalized);
    }

    /// <summary>
    /// Строит SectionDto по группе статей раздела
    /// </summary>
    private SectionDto BuildSectionDto(
        Guid sectionId,
        IEnumerable<(Guid SectionId,
            Guid ArticleId,
            string Title,
            DateTimeOffset CreatedAtUtc,
            DateTimeOffset? UpdatedAtUtc,
            List<string> TagNamesOriginal,
            List<string> TagNamesNormalized)> group)
    {
        var firstArticle = group.First();

        List<string> sectionTags = firstArticle.TagNamesOriginal
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        int articlesCount = group
            .Select(x => x.ArticleId)
            .Distinct()
            .Count();

        string sectionName = _sectionKeyService.BuildSectionName(sectionTags);

        SectionDto dto = new SectionDto
        {
            Id = sectionId,
            Name = sectionName,
            Tags = sectionTags,
            ArticlesCount = articlesCount
        };

        return dto;
    }

    /// <summary>
    /// Вычисляет время сортировки статьи по UpdatedAtUtc или CreatedAtUtc
    /// </summary>
    private static DateTimeOffset GetSortTime(
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc)
    {
        return updatedAtUtc ?? createdAtUtc;
    }
}
