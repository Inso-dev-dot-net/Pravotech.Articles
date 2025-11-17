using Microsoft.AspNetCore.Mvc;
using Pravotech.Articles.Application.Abstractions;
using Pravotech.Articles.Application.Contracts.Articles;

namespace Pravotech.Articles.WebApi.Controllers;

/// <summary>
/// HTTP API для работы со статьями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
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
    /// <remarks>
    /// Возвращает полную информацию о статье по идентификатору  
    /// Список тегов в ответе возвращается в том порядке, в котором он был задан при создании или последнем обновлении статьи
    /// </remarks>
    /// <param name="id">Идентификатор статьи</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="200">Статья найдена и возвращена</response>
    /// <response code="404">Статья с указанным идентификатором не найдена</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// <remarks>
    /// Создает новую статью с указанным названием и списком тегов  
    /// Дата и время создания устанавливаются автоматически на сервере  
    /// Список тегов сохраняется и возвращается в том же порядке, который указал клиент  
    /// Максимальная длина названия статьи 256 символов, максимальное количество тегов 256
    /// </remarks>
    /// <param name="request">Данные для создания статьи</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="201">Статья успешно создана</response>
    /// <response code="400">Невалидные данные запроса</response>
    [HttpPost]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    /// <remarks>
    /// Обновляет название статьи и список тегов по идентификатору  
    /// Дата и время изменения устанавливаются автоматически на сервере  
    /// Список тегов сохраняется и возвращается в том же порядке, который указал клиент  
    /// Если статья не найдена, возвращается статус 404
    /// </remarks>
    /// <param name="id">Идентификатор статьи</param>
    /// <param name="request">Новые данные статьи</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="204">Статья успешно обновлена</response>
    /// <response code="400">Невалидные данные запроса</response>
    /// <response code="404">Статья с указанным идентификатором не найдена</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// <remarks>
    /// Удаляет статью по идентификатору  
    /// Если статья не найдена, возвращается статус 404  
    /// При успешном удалении возвращается статус 204 без тела ответа
    /// </remarks>
    /// <param name="id">Идентификатор статьи</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="204">Статья успешно удалена</response>
    /// <response code="404">Статья с указанным идентификатором не найдена</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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