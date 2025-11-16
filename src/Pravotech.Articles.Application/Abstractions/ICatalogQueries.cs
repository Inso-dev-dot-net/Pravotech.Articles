using Pravotech.Articles.Application.Contracts.Articles;
using Pravotech.Articles.Application.Contracts.Sections;

namespace Pravotech.Articles.Application.Abstractions;

/// <summary>
/// Запросы к каталогу статей
/// </summary>
public interface ICatalogQueries
{
    /// <summary>
    /// Возвращает список разделов, сортировка по количеству статей по убыванию
    /// </summary>
    Task<IReadOnlyList<SectionDto>> GetSectionsAsync(CancellationToken ct);

    /// <summary>
    /// Возвращает список статей раздела, сортировка по UpdatedAtUtc(CreatedAtUtc) по убыванию
    /// </summary>
    Task<IReadOnlyList<ArticleDto>> GetSectionArticlesAsync(Guid sectionId, CancellationToken ct);
}