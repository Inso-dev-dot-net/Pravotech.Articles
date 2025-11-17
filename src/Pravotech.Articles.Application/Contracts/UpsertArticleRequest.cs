namespace Pravotech.Articles.Application.Contracts.Articles;

/// <summary>
/// Моделька запроса для создания или обновления статьи
/// </summary>
public sealed class UpsertArticleRequest
{
    /// <summary>Название статьи</summary>
    public string Title { get; init; } = default!;

    /// <summary>Список имён тегов</summary>
    public List<string> Tags { get; init; } = new();
}
