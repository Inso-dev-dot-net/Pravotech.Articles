namespace Pravotech.Articles.Application.Contracts.Sections;

/// <summary>Раздел каталога статей</summary>
public sealed class SectionDto
{
    /// <summary>Идентификатор раздела</summary>
    public Guid Id { get; set; }

    /// <summary>Человекочитаемое название раздела</summary>
    /// <remarks>
    /// Формируется конкатенацией названий тегов через запятую
    /// Может быть обрезано до максимально допустимой длины названия раздела
    /// </remarks>
    public string Name { get; set; } = default!;

    /// <summary>Список тегов раздела</summary>
    /// <remarks>
    /// Набор тегов определяет раздел без учета порядка
    /// Порядок в этом списке не влияет на принадлежность статей к разделу
    /// </remarks>
    public List<string> Tags { get; set; } = new();

    /// <summary>Количество статей в разделе</summary>
    public int ArticlesCount { get; set; }
}