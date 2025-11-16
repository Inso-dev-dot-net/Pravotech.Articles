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
public sealed class SectionsController : ControllerBase
{
    private readonly ICatalogQueries _catalogQueries;

    public SectionsController(ICatalogQueries catalogQueries)
    {
        _catalogQueries = catalogQueries;
    }

    /// <summary>Возвращает список разделов </summary>
    [HttpGet]
    public async Task<IActionResult> GetSections(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SectionDto> sections = await _catalogQueries.GetSectionsAsync(cancellationToken);

        return Ok(sections);
    }

    /// <summary>Возвращает все статьи выбранного раздела</summary>
    [HttpGet("{id:guid}/articles")]
    public async Task<IActionResult> GetSectionArticles(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ArticleDto> articles = await _catalogQueries.GetSectionArticlesAsync(id, cancellationToken);
        return Ok(articles);
    }


}
