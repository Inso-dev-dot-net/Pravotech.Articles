namespace Pravotech.Articles.Domain.Services;

/// <summary> Формирование ключей и идентификаторов разделов </summary>
public interface ISectionKeyService
{
    /// <summary>Строит SectionKey из списка нормализованных имен тегов </summary>
    /// <param name="normalizedTagNames">Нормализованные имена тегов</param>
    string BuildSectionKey(IEnumerable<string> normalizedTagNames);

    /// <summary>Вычисляет Guid раздела на основе SectionKey</summary>
    /// <param name="sectionKey">Ключ раздела</param>
    Guid BuildSectionId(string sectionKey);

    /// <summary> Строит отображаемое название раздела из списка исходных имен тегов </summary>
    /// <param name="tagNames">Имена тегов</param>
    string BuildSectionName(IEnumerable<string> tagNames);
}
