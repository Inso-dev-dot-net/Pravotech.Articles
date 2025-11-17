using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Pravotech.Articles.WebApi.Tests;

/// <summary>
/// Разделы
/// </summary>
[Collection("Api collection")]
public sealed class SectionsApiTests
{
    private readonly HttpClient _client;

    public SectionsApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<ArticleDto> CreateArticleAsync(
        string title,
        IList<string> tags)
    {
        UpsertArticleRequest request = new()
        {
            Title = title,
            Tags = tags.ToList()
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/articles", request);
        response.EnsureSuccessStatusCode();

        ArticleDto? created = await response.Content.ReadFromJsonAsync<ArticleDto>();
        created.Should().NotBeNull();

        return created!;
    }

    [Fact]
    public async Task GetSections_ShouldReturnEmptyList_WhenNoArticles()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/sections");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        List<SectionDto>? sections = await response.Content.ReadFromJsonAsync<List<SectionDto>>();
        sections.Should().NotBeNull();
        sections!.Should().BeEmpty();
    }

    [Fact]
    public async Task Sections_ShouldBeCreatedForUniqueTagSets_AndCountArticles()
    {
        // arrange
        ArticleDto a1 = await CreateArticleAsync(
            "Статья 1",
            new[] { "Backend", "C#" });

        ArticleDto a2 = await CreateArticleAsync(
            "Статья 2",
            new[] { "Backend", "Kafka" });

        ArticleDto a3 = await CreateArticleAsync(
            "Статья 3",
            new[] { "Backend", "C#" }); // тот же набор тегов, что у a1

        // act
        HttpResponseMessage response = await _client.GetAsync("/api/sections");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        List<SectionDto>? sections = await response.Content.ReadFromJsonAsync<List<SectionDto>>();
        sections.Should().NotBeNull();

        // ожидаем 2 раздела:
        // - один для {Backend, C#} с 2 статьями
        // - один для {Backend, Kafka} с 1 статьёй
        sections!.Should().HaveCount(2);

        SectionDto backendCsharp = sections!.Single(s =>
            s.Tags.Contains("Backend") &&
            s.Tags.Contains("C#") &&
            s.Tags.Count == 2);

        SectionDto backendKafka = sections!.Single(s =>
            s.Tags.Contains("Backend") &&
            s.Tags.Contains("Kafka") &&
            s.Tags.Count == 2);

        backendCsharp.ArticlesCount.Should().Be(2);
        backendKafka.ArticlesCount.Should().Be(1);

        // проверка сортировки по ArticlesCount по убыванию
        sections![0].Id.Should().Be(backendCsharp.Id);
        sections[0].ArticlesCount.Should().BeGreaterOrEqualTo(sections[1].ArticlesCount);
    }

    [Fact]
    public async Task Sections_ShouldIgnoreTagOrder_WhenGrouping()
    {
        // arrange: создаем две статьи с одинаковым набором тегов, но в разном порядке
        ArticleDto s1 = await CreateArticleAsync(
            "Первая",
            new[] { "Backend", "C#" });

        ArticleDto s2 = await CreateArticleAsync(
            "Вторая",
            new[] { "C#", "Backend" });

        // act
        HttpResponseMessage response = await _client.GetAsync("/api/sections");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        List<SectionDto>? sections = await response.Content.ReadFromJsonAsync<List<SectionDto>>();
        sections.Should().NotBeNull();

        // должен быть один раздел с ArticlesCount = 2
        sections!.Should().HaveCount(1);
        SectionDto section = sections!.Single();

        section.ArticlesCount.Should().Be(2);
        section.Tags.Should().BeEquivalentTo(new[] { "Backend", "C#" });
    }

    [Fact]
    public async Task GetSectionArticles_ShouldReturnArticlesSortedByUpdatedThenCreated()
    {
        // arrange: создаем две статьи в одном разделе
        ArticleDto a1 = await CreateArticleAsync(
            "Статья 1",
            new[] { "Backend", "C#" });

        ArticleDto a2 = await CreateArticleAsync(
            "Статья 2",
            new[] { "Backend", "C#" });

        // получаем раздел
        HttpResponseMessage sectionsResponse = await _client.GetAsync("/api/sections");
        sectionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        List<SectionDto>? sections = await sectionsResponse.Content.ReadFromJsonAsync<List<SectionDto>>();
        sections.Should().NotBeNull();

        SectionDto section = sections!.Single();

        // обновляем вторую статью, чтобы она стала "посвежее"
        UpsertArticleRequest updateSecond = new()
        {
            Title = "Статья 2 (обновленная)",
            Tags = new List<string> { "Backend", "C#" }
        };

        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync(
            $"/api/articles/{a2.Id}",
            updateSecond);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // act: запрашиваем статьи в разделе
        HttpResponseMessage articlesResponse = await _client.GetAsync($"/api/sections/{section.Id}/articles");
        articlesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        List<ArticleDto>? articles = await articlesResponse.Content.ReadFromJsonAsync<List<ArticleDto>>();
        articles.Should().NotBeNull();
        articles!.Should().HaveCount(2);

        // ожидаем, что обновленная статья будет первой по убыванию Updated/Created
        articles![0].Id.Should().Be(a2.Id);
        articles[0].Title.Should().Be(updateSecond.Title);
    }
}
