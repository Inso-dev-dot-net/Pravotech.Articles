namespace Pravotech.Articles.Domain.Entities;

/// <summary>
/// Статья каталога
/// </summary>
public sealed class Article
{
    private const int MaxLength = 256;

    /// <summary>Идентификатор статьи</summary>
    public Guid Id { get; private set; }

    /// <summary>Название статьи</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Дата и время создания статьи (UTC)</summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>Дата и время изменения статьи (UTC)</summary>
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    private readonly List<ArticleTag> _tags = new();

    /// <summary>Список тегов статьи с позициями в исходном порядке</summary>
    public IReadOnlyList<ArticleTag> Tags => _tags;

    private Article() { }

    /// <summary>Создает новую статью</summary>
    /// <param name="id">Идентификатор статьи</param>
    /// <param name="title">Название статьи</param>
    /// <param name="nowUtc"> Время UTC для CreatedAt</param>
    /// <param name="tagIdsInOrder">Список идентификаторов тегов в исходном порядке клиента</param>
    public static Article Create(
        Guid id,
        string title,
        DateTimeOffset nowUtc,
        IEnumerable<Guid> tagIdsInOrder)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Article id cannot be empty", nameof(id));
        }

        string normalizedTitle = NormalizeAndValidateTitle(title);
        List<Guid> distinctTagIds = NormalizeAndValidateTags(tagIdsInOrder);

        Article article = new Article
        {
            Id = id,
            Title = normalizedTitle,
            CreatedAtUtc = nowUtc
        };

        for (int i = 0; i < distinctTagIds.Count; i++)
        {
            article._tags.Add(ArticleTag.Create(distinctTagIds[i], i));
        }

        return article;
    }

    /// <summary>Обновляет название и список тегов статьи</summary>
    /// <param name="title">Новое название статьи</param>
    /// <param name="nowUtc">Время UTC для UpdatedAt</param>
    /// <param name="tagIdsInOrder">Новый список идентификаторов тегов в исходном порядке</param>
    public void Update(
        string title,
        DateTimeOffset nowUtc,
        IEnumerable<Guid> tagIdsInOrder)
    {
        string normalizedTitle = NormalizeAndValidateTitle(title);
        List<Guid> distinctTagIds = NormalizeAndValidateTags(tagIdsInOrder);

        Title = normalizedTitle;
        UpdatedAtUtc = nowUtc;

        _tags.Clear();

        for (int i = 0; i < distinctTagIds.Count; i++)
        {
            Guid tagId = distinctTagIds[i];
            _tags.Add(ArticleTag.Create(tagId, i));
        }
    }

    /// <summary>
    /// Вычисляет ключ раздела как множество нормализованных имен тегов
    /// в отсортированном виде, объединенных через символ '|'
    /// </summary>
    /// <param name="tagIdToNormalizedName">Функция преобразования TagId в нормализованное имя</param>
    public string ComputeSectionKey(Func<Guid, string> tagIdToNormalizedName)
    {
        if (tagIdToNormalizedName is null)
            throw new ArgumentNullException(nameof(tagIdToNormalizedName));

        if (_tags.Count == 0)
            return string.Empty;

        var names = _tags.Select(t => tagIdToNormalizedName(t.TagId))
            .Distinct()
            .OrderBy(tg => tg);

        return string.Join("|", names);
    }


    private static string NormalizeAndValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty", nameof(title));
        }

        string trimmed = title.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Article title cannot be longer than {MaxLength} characters", nameof(title));
        }

        return trimmed;
    }

    private static List<Guid> NormalizeAndValidateTags(IEnumerable<Guid> tagIdsInOrder)
    {
        if (tagIdsInOrder is null)
        {
            throw new ArgumentNullException(nameof(tagIdsInOrder), "List of tags cannot be null");
        }

        List<Guid> tagList = tagIdsInOrder.ToList();

        if (tagList.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException("Tag ids cannot contain Guid.Empty", nameof(tagIdsInOrder));
        }

        List<Guid> distinct = tagList.Distinct().ToList();

        if (distinct.Count > MaxLength)
        {
            throw new ArgumentException($"Articles cannot have more than {MaxLength} distinct tags", nameof(tagIdsInOrder));
        }

        return distinct;
    }
}