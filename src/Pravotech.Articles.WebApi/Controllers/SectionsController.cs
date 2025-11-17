using Microsoft.AspNetCore.Mvc;
using Pravotech.Articles.Application.Abstractions;
using Pravotech.Articles.Application.Contracts.Articles;
using Pravotech.Articles.Application.Contracts.Sections;

namespace Pravotech.Articles.WebApi.Controllers;

/// <summary>
/// HTTP API для работы с разделами статей
/// </summary>
[ApiController]
[Route("api/sections")]
[Produces("application/json")]
public sealed class SectionsController : ControllerBase
{
    private readonly ICatalogQueries _catalogQueries;

    public SectionsController(ICatalogQueries catalogQueries)
    {
        _catalogQueries = catalogQueries;
    }

    /// <summary>Возвращает список разделов</summary>
    /// <remarks>
    /// Возвращает список разделов каталога статей  
    /// Разделы формируются автоматически для каждого уникального набора тегов без учета порядка  
    /// Список разделов сортируется по убыванию количества статей в разделе
    /// </remarks>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <response code="200">Список разделов успешно получен</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SectionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSections(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SectionDto> sections = await _catalogQueries.GetSectionsAsync(cancellationToken);

        return Ok(sections);
    }

    /// <summary>Возвращает все статьи выбранного раздела</summary>
    /// <remarks>
    /// Возвращает список статей, принадлежащих выбранному разделу  
    /// Принадлежность к разделу определяется совпадающим набором тегов без учета порядка  
    /// Список статей сортируется по дате и времени изменения, а при отсутствии UpdatedAtUtc по CreatedAtUtc по убыванию
    /// </remarks>
    /// <param name="id">Идентификатор раздела</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <response code="200">Список статей раздела успешно получен</response>
    [HttpGet("{id:guid}/articles")]
    [ProducesResponseType(typeof(IReadOnlyList<ArticleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSectionArticles(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ArticleDto> articles = await _catalogQueries.GetSectionArticlesAsync(id, cancellationToken);

        return Ok(articles);
    }
}