namespace Pravotech.Articles.WebApi.Tests;

internal sealed class ArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public List<string> Tags { get; set; } = new();
}

internal sealed class SectionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<string> Tags { get; set; } = new();
    public int ArticlesCount { get; set; }
}

internal sealed class UpsertArticleRequest
{
    public string Title { get; set; } = default!;
    public List<string> Tags { get; set; } = new();
}
