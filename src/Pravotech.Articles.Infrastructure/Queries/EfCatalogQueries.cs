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

    /// <summary>
    /// Плоское представление связки статья - тег
    /// </summary>
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

        // Группируем строки по статье и восстанавливаем упорядоченный список тегов статьи
        var articles = rows
            .GroupBy(r => new { r.ArticleId, r.Title, r.CreatedAtUtc, r.UpdatedAtUtc })
            .Select(g => new
            {
                g.Key.ArticleId,
                g.Key.Title,
                g.Key.CreatedAtUtc,
                g.Key.UpdatedAtUtc,
                TagsOrdered = g
                    .OrderBy(x => x.Position)
                    .Select(x => new
                    {
                        x.TagName,
                        x.TagNameNormalized
                    })
                    .ToList()
            })
            .ToList();

        // Для каждой статьи считаем SectionKey и SectionId
        var articleWithSectionInfo = articles
            .Select(a =>
            {
                IEnumerable<string> normalizedNames = a.TagsOrdered
                    .Select(t => t.TagNameNormalized);

                string sectionKey = _sectionKeyService.BuildSectionKey(normalizedNames);
                Guid sectionId = _sectionKeyService.BuildSectionId(sectionKey);

                return new
                {
                    SectionId = sectionId,
                    SectionKey = sectionKey,
                    a.ArticleId,
                    a.Title,
                    a.CreatedAtUtc,
                    a.UpdatedAtUtc,
                    TagNamesOriginal = a.TagsOrdered.Select(t => t.TagName).ToList(),
                    TagNamesNormalized = a.TagsOrdered.Select(t => t.TagNameNormalized).ToList()
                };
            })
            // Статьи без тегов не попадают ни в один раздел
            .Where(x => x.SectionId != Guid.Empty && x.TagNamesNormalized.Count > 0)
            .ToList();

        if (articleWithSectionInfo.Count == 0)
        {
            return Array.Empty<SectionDto>();
        }

        // Группируем статьи по SectionId/SectionKey и считаем разделы
        List<SectionDto> sections = articleWithSectionInfo
            .GroupBy(x => new { x.SectionId, x.SectionKey })
            .Select(g =>
            {
                Guid sectionId = g.Key.SectionId;

                var firstArticle = g.First();

                List<string> sectionTags = firstArticle.TagNamesOriginal
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                int articlesCount = g
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
            })
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

        // Группируем по статье и собираем упорядоченный список тегов
        var articles = rows
            .GroupBy(r => new { r.ArticleId, r.Title, r.CreatedAtUtc, r.UpdatedAtUtc })
            .Select(g => new
            {
                g.Key.ArticleId,
                g.Key.Title,
                g.Key.CreatedAtUtc,
                g.Key.UpdatedAtUtc,
                TagsOrdered = g
                    .OrderBy(x => x.Position)
                    .Select(x => new
                    {
                        x.TagName,
                        x.TagNameNormalized
                    })
                    .ToList()
            })
            .ToList();

        // Для каждой статьи заново считаем SectionId и фильтруем по нужному разделу
        var articlesInSection = articles
            .Select(a =>
            {
                IEnumerable<string> normalizedNames = a.TagsOrdered
                    .Select(t => t.TagNameNormalized);

                string sectionKey = _sectionKeyService.BuildSectionKey(normalizedNames);
                Guid computedSectionId = _sectionKeyService.BuildSectionId(sectionKey);

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
                DateTimeOffset sortTime = a.UpdatedAtUtc ?? a.CreatedAtUtc;

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
    /// Загружает список связок статья - тег из базы
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
}
