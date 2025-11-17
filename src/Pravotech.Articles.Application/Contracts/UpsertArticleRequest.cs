namespace Pravotech.Articles.Application.Contracts.Articles;

/// <summary>
/// Моделька запроса для создания или обновления статьи
/// </summary>
public sealed class UpsertArticleRequest
{
    /// <summary>Название статьи</summary>
    public string Title { get; init; } = default!;


    /// <summary>Список тегов статьи в порядке клиента</summary>
    /// <remarks>
    /// Порядок тегов сохраняется и используется при отображении статьи
    /// Максимальное количество тегов 256
    /// Дубликаты будут удалены с сохранением порядка первого вхождения
    /// Регистр имени тега не влияет на уникальность, но оригинальное написание сохраняется
    /// </remarks>
    public List<string> Tags { get; init; } = new();
}
