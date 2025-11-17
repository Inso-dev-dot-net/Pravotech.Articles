using Pravotech.Articles.Application.Contracts.Articles;

namespace Pravotech.Articles.Application.Abstractions;

/// <summary>
/// Команды и операции над статьями
/// </summary>
public interface IArticleCommands
{
    /// <summary>Возвращает статью по идентификатору или null если не найдена</summary>
    Task<ArticleDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    /// <summary>Создает новую статью</summary>
    Task<ArticleDto> CreateAsync(
        UpsertArticleRequest request,
        CancellationToken ct = default);

    /// <summary>Обновляет существующую статью</summary>
    /// <returns>true если статья найдена и обновлена, иначе false</returns>
    Task<bool> UpdateAsync(
        Guid id,
        UpsertArticleRequest request,
        CancellationToken ct = default);

    /// <summary>Удаляет статью</summary>
    /// <returns>true если статья найдена и удалена, иначе false</returns>
    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken ct = default);
}
