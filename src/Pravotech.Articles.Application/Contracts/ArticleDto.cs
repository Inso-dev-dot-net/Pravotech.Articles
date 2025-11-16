namespace Pravotech.Articles.Application.Contracts.Articles;

/// <summary>
/// DTO статьи 
/// </summary>
public sealed class ArticleDto
{
    public Guid Id { get; init; }

    /// <summary>Название</summary>
    public string Title { get; init; } = default!;

    /// <summary>Дата и время создания UTC</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Дата и время изменения UTC </summary>
    public DateTimeOffset? UpdatedAtUtc { get; init; }

    /// <summary>Список тегов в исходном порядке</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}