using Pravotech.Articles.Domain.ValueObjects;

namespace Pravotech.Articles.Domain.Entities;

public sealed class Tag
{
    /// <summary>Идентификатор тега</summary>
    public Guid Id { get; private set; }

    /// <summary>Оригинальное имя тега</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Нормализованное имя для уникальности</summary>
    public string NameNormalized { get; private set; } = default!;

    //TODO: Добавить навигационные свойства

    private Tag() { }

    /// <summary>Создает новый тег</summary>
    /// <param name="id">Идентификатор тега</param>
    /// <param name="name">Имя тега</param>
    public static Tag Create(Guid id, TagName name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Article id cannot be empty", nameof(id));

        Tag tag = new()
        {
            Id = id,
            Name = name.Value,
            NameNormalized = name.Normalized
        };

        return tag;
    }
}
