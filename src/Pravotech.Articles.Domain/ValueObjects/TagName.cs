namespace Pravotech.Articles.Domain.ValueObjects;

public readonly record struct TagName
{
    private const int MaxLength = 256;

    /// <summary>Оригинальное имя тега</summary>
    public string Value { get; }

    /// <summary>Нормализованное имя</summary>
    public string Normalized => Value.ToLowerInvariant();


    /// <summary>Создает новый TagName</summary>
    /// <param name="value">Исходная строка имени тега</param>
    /// <exception cref="ArgumentException">Если имя пустое или длина больше 256 символов</exception>
    public TagName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tag name cannot be empty or whitespace", nameof(value));
        }

        string trimmed = value.Trim();


        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Tag name cannot be longer than {MaxLength} characters", nameof(value));
        }

        Value = trimmed;
    }

    public override string ToString() => Value;

}