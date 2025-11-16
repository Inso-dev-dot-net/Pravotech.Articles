namespace Pravotech.Articles.Application.Contracts.Sections;

/// <summary>
/// DTO раздела каталога
/// </summary>
public sealed class SectionDto
{
    public Guid Id { get; init; }

    /// <summary>Название раздела (конкатенация тегов через запятую до макс. длинны)</summary>
    public string Name { get; init; } = default!;

    /// <summary>Список тегов раздела</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Количество статей внутри раздела</summary>
    public int ArticlesCount { get; init; }
}