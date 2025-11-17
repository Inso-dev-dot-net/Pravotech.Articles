using Microsoft.AspNetCore.Mvc;
using Pravotech.Articles.Application.Abstractions;
using Pravotech.Articles.Application.Contracts.Articles;

namespace Pravotech.Articles.WebApi.Controllers;

/// <summary>
/// HTTP API для работы со статьями
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ArticlesController : ControllerBase
{
    private readonly IArticleCommands _articleCommands;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(
        IArticleCommands articleCommands,
        ILogger<ArticlesController> logger)
    {
        _articleCommands = articleCommands;
        _logger = logger;
    }

    /// <summary>Возвращает статью по идентификатору</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct = default)
    {
        ArticleDto? article = await _articleCommands.GetByIdAsync(id, ct);

        if (article is null)
        {
            _logger.LogWarning(
                "Article not found {ArticleId}",
                id);

            return NotFound();
        }

        return Ok(article);
    }

    /// <summary>Создает новую статью</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] UpsertArticleRequest request,
        CancellationToken ct = default)
    {
        if (request is null)
        {
            return BadRequest();
        }

        ArticleDto article = await _articleCommands.CreateAsync(request, ct);

        _logger.LogInformation(
            "Article created {ArticleId} with {TagCount} tags",
            article.Id,
            article.Tags.Count);

        return CreatedAtAction(
            nameof(GetById),
            new { id = article.Id },
            article);
    }

    /// <summary>Обновляет существующую статью</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpsertArticleRequest request,
        CancellationToken ct = default)
    {
        if (request is null)
        {
            return BadRequest();
        }

        bool updated = await _articleCommands.UpdateAsync(id, request, ct);

        if (!updated)
        {
            _logger.LogWarning(
                "Attempt to update not existing article {ArticleId}",
                id);

            return NotFound();
        }

        _logger.LogInformation(
            "Article updated {ArticleId}",
            id);

        return NoContent();
    }

    /// <summary>Удаляет статью</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct = default)
    {
        bool deleted = await _articleCommands.DeleteAsync(id, ct);

        if (!deleted)
        {
            _logger.LogWarning(
                "Attempt to delete not existing article {ArticleId}",
                id);

            return NotFound();
        }

        _logger.LogInformation(
            "Article deleted {ArticleId}",
            id);

        return NoContent();
    }
}
