using Microsoft.EntityFrameworkCore;
using Pravotech.Articles.Application.Abstractions;
using Pravotech.Articles.Application.Contracts.Articles;
using Pravotech.Articles.Domain.Entities;
using Pravotech.Articles.Domain.ValueObjects;
using Pravotech.Articles.Infrastructure.Persistence;

namespace Pravotech.Articles.Infrastructure.Commands;

/// <summary>
/// Реализация CRUD статей
/// </summary>
internal sealed class EfArticleCommands : IArticleCommands
{
    private readonly ArticlesDbContext _dbContext;

    public EfArticleCommands(ArticlesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<ArticleDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        Article? article = await _dbContext.Articles
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (article is null)
        {
            return null;
        }

        List<string> tagNames = await LoadArticleTagNamesAsync(id, ct);

        ArticleDto dto = new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            CreatedAtUtc = article.CreatedAtUtc,
            UpdatedAtUtc = article.UpdatedAtUtc,
            Tags = tagNames
        };

        return dto;
    }

    /// <inheritdoc/>
    public async Task<ArticleDto> CreateAsync(
        UpsertArticleRequest request,
        CancellationToken ct = default)
    {
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

        List<Tag> tags = await GetOrCreateTagsAsync(request.Tags, ct);

        Guid[] tagIdsInOrder = tags
            .Select(t => t.Id)
            .ToArray();

        Guid articleId = Guid.NewGuid();

        Article article = Article.Create(
            articleId,
            request.Title,
            nowUtc,
            tagIdsInOrder);

        _dbContext.Articles.Add(article);

        await _dbContext.SaveChangesAsync(ct);

        ArticleDto dto = new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            CreatedAtUtc = article.CreatedAtUtc,
            UpdatedAtUtc = article.UpdatedAtUtc,
            Tags = request.Tags.ToList()
        };

        return dto;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(
        Guid id,
        UpsertArticleRequest request,
        CancellationToken ct = default)
    {
        Article? article = await _dbContext.Articles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (article is null)
        {
            return false;
        }

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

        List<Tag> tags = await GetOrCreateTagsAsync(request.Tags, ct);

        Guid[] tagIdsInOrder = tags
            .Select(t => t.Id)
            .ToArray();

        article.Update(
            request.Title,
            nowUtc,
            tagIdsInOrder);

        await _dbContext.SaveChangesAsync(ct);

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken ct = default)
    {
        Article? article = await _dbContext.Articles
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (article is null)
        {
            return false;
        }

        _dbContext.Articles.Remove(article);

        await _dbContext.SaveChangesAsync(ct);

        return true;
    }

    /// <summary>
    /// Загружает имена тегов статьи в исходном порядке
    /// TODO: мб вынести в интерейс и переюзать в EfCatalogQueries
    /// </summary>
    private async Task<List<string>> LoadArticleTagNamesAsync(
        Guid articleId,
        CancellationToken ct)
    {
        List<string> tagNames = await _dbContext.Articles
            .Where(a => a.Id == articleId)
            .SelectMany(a => a.Tags, (article, tagRef) => new { tagRef })
            .Join(
                _dbContext.Tags,
                x => x.tagRef.TagId,
                tag => tag.Id,
                (x, tag) => new
                {
                    x.tagRef.Position,
                    tag.Name
                })
            .OrderBy(x => x.Position)
            .Select(x => x.Name)
            .ToListAsync(ct);

        return tagNames;
    }

    /// <summary>
    /// Находит существующие теги по именам или создает новые
    /// TODO: мб вынести в интерейс и переюзать в EfCatalogQueries
    /// </summary>
    private async Task<List<Tag>> GetOrCreateTagsAsync(
        IEnumerable<string> tagNames,
        CancellationToken ct)
    {
        List<TagName> tagNameValues = tagNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => new TagName(name))
            .ToList();

        if (tagNameValues.Count == 0)
        {
            return new List<Tag>();
        }

        List<string> normalizedNames = tagNameValues
            .Select(t => t.Normalized)
            .ToList();

        List<Tag> existingTags = await _dbContext.Tags
            .Where(t => normalizedNames.Contains(t.NameNormalized))
            .ToListAsync(ct);

        List<Tag> result = new List<Tag>();

        foreach (TagName tagName in tagNameValues)
        {
            Tag? existing = existingTags
                .FirstOrDefault(t => t.NameNormalized == tagName.Normalized);

            if (existing is not null)
            {
                result.Add(existing);
                continue;
            }

            Guid id = Guid.NewGuid();
            Tag newTag = Tag.Create(id, tagName);

            _dbContext.Tags.Add(newTag);
            result.Add(newTag);
        }

        return result;
    }
}
