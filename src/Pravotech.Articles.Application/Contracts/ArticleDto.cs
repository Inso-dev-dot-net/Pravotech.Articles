namespace Pravotech.Articles.Application.Contracts.Articles;

/// <summary>Данные статьи для ответов HTTP API</summary>
public sealed class ArticleDto
{
    /// <summary>Идентификатор статьи</summary>
    public Guid Id { get; set; }

    /// <summary>Название статьи</summary>
    public string Title { get; set; } = default!;

    /// <summary>Дата и время создания в UTC</summary>
    /// <remarks>
    /// Устанавливается автоматически при создании статьи на сервере
    /// Всегда заполнено
    /// </remarks>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Дата и время последнего изменения в UTC</summary>
    /// <remarks>
    /// Устанавливается автоматически при успешном обновлении статьи
    /// Может быть null, если статья ни разу не изменялась после создания
    /// </remarks>
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    /// <summary>Список тегов статьи</summary>
    /// <remarks>
    /// Теги возвращаются в том же порядке, в котором они были заданы клиентом
    /// Порядок соответствует последнему успешному запросу создания или обновления статьи
    /// </remarks>
    public List<string> Tags { get; set; } = new();
}