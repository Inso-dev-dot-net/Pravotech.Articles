using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Pravotech.Articles.WebApi.Tests;

/// <summary>
/// Поведенческие тесты CRUD статей по HTTP API
/// </summary>
[Collection("Api collection")]
public sealed class ArticlesApiTests
{
    private readonly HttpClient _client;

    public ArticlesApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostArticle_ShouldCreateArticle_WithOrderedTags_AndDates()
    {
        // arrange
        UpsertArticleRequest request = new()
        {
            Title = "Первая статья",
            Tags = new List<string> { "Backend", "C#", "Kafka" }
        };

        // act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/articles", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        ArticleDto? article = await response.Content.ReadFromJsonAsync<ArticleDto>();
        article.Should().NotBeNull();
        article!.Id.Should().NotBe(Guid.Empty);
        article.Title.Should().Be(request.Title);
        article.Tags.Should().ContainInOrder(request.Tags);
        article.CreatedAtUtc.Should().NotBe(default);
        article.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task GetArticle_ShouldReturnArticleById()
    {
        // arrange - сначала создаем
        UpsertArticleRequest create = new()
        {
            Title = "Статья для чтения",
            Tags = new List<string> { "Tag1", "Tag2" }
        };

        HttpResponseMessage createdResponse = await _client.PostAsJsonAsync("/api/articles", create);
        ArticleDto? created = await createdResponse.Content.ReadFromJsonAsync<ArticleDto>();
        created.Should().NotBeNull();

        Guid id = created!.Id;

        // act
        HttpResponseMessage response = await _client.GetAsync($"/api/articles/{id}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ArticleDto? article = await response.Content.ReadFromJsonAsync<ArticleDto>();
        article.Should().NotBeNull();
        article!.Id.Should().Be(id);
        article.Title.Should().Be(create.Title);
        article.Tags.Should().ContainInOrder(create.Tags);
    }

    [Fact]
    public async Task GetArticle_ShouldReturnNotFound_ForUnknownId()
    {
        Guid id = Guid.NewGuid();

        HttpResponseMessage response = await _client.GetAsync($"/api/articles/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutArticle_ShouldUpdateTitleAndTags_AndSetUpdatedAt()
    {
        // arrange - создаем статью
        UpsertArticleRequest create = new()
        {
            Title = "Исходный тайтл",
            Tags = new List<string> { "Backend", "C#" }
        };

        HttpResponseMessage createdResponse = await _client.PostAsJsonAsync("/api/articles", create);
        ArticleDto? created = await createdResponse.Content.ReadFromJsonAsync<ArticleDto>();
        created.Should().NotBeNull();

        Guid id = created!.Id;

        // act - обновляем
        UpsertArticleRequest update = new()
        {
            Title = "Обновленный тайтл",
            Tags = new List<string> { "Kafka", "Backend" }
        };

        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync($"/api/articles/{id}", update);

        // assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // проверяем, что GET отдаёт обновление
        HttpResponseMessage getResponse = await _client.GetAsync($"/api/articles/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        ArticleDto? article = await getResponse.Content.ReadFromJsonAsync<ArticleDto>();
        article.Should().NotBeNull();
        article!.Title.Should().Be(update.Title);
        article.Tags.Should().ContainInOrder(update.Tags);
        article.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task PutArticle_ShouldReturnNotFound_ForUnknownId()
    {
        UpsertArticleRequest request = new()
        {
            Title = "Новый тайтл",
            Tags = new List<string> { "Tag" }
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/articles/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteArticle_ShouldRemoveArticle()
    {
        // arrange - создаем статью
        UpsertArticleRequest create = new()
        {
            Title = "Статья для удаления",
            Tags = new List<string> { "Delete", "Me" }
        };

        HttpResponseMessage createdResponse = await _client.PostAsJsonAsync("/api/articles", create);
        ArticleDto? created = await createdResponse.Content.ReadFromJsonAsync<ArticleDto>();
        created.Should().NotBeNull();

        Guid id = created!.Id;

        // act
        HttpResponseMessage deleteResponse = await _client.DeleteAsync($"/api/articles/{id}");

        // assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage getResponse = await _client.GetAsync($"/api/articles/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteArticle_ShouldReturnNotFound_ForUnknownId()
    {
        HttpResponseMessage response = await _client.DeleteAsync($"/api/articles/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
