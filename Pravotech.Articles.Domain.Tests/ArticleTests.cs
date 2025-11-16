using Pravotech.Articles.Domain.Entities;

namespace Pravotech.Articles.Domain.Tests.Entities;

public sealed class ArticleTests
{
    [Fact]
    public void Create_ValidInput_ShouldSetBasicProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string title = "  Hello World  ";
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        List<Guid> tags = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        Article article = Article.Create(id, title, nowUtc, tags);

        // Assert
        Assert.Equal(id, article.Id);
        Assert.Equal("Hello World", article.Title);
        Assert.Equal(nowUtc, article.CreatedAtUtc);
        Assert.Null(article.UpdatedAtUtc);
        Assert.Equal(2, article.Tags.Count);
    }

    [Fact]
    public void Create_DuplicateTags_ShouldKeepDistinctInOrder()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid tagA = Guid.NewGuid();
        Guid tagB = Guid.NewGuid();

        List<Guid> tags = new List<Guid>
        {
            tagA,
            tagB,
            tagA,
            tagB
        };

        // Act
        Article article = Article.Create(id, "Test", DateTimeOffset.UtcNow, tags);

        // Assert
        Assert.Equal(2, article.Tags.Count);
        Assert.Equal(tagA, article.Tags[0].TagId);
        Assert.Equal(0, article.Tags[0].Position);
        Assert.Equal(tagB, article.Tags[1].TagId);
        Assert.Equal(1, article.Tags[1].Position);
    }

    [Fact]
    public void Update_ShouldChangeTitleAndTagsAndSetUpdatedAt()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid tagA = Guid.NewGuid();
        Guid tagB = Guid.NewGuid();
        Guid tagC = Guid.NewGuid();

        Article article = Article.Create(
            id,
            "Original",
            DateTimeOffset.UtcNow,
            new List<Guid> { tagA });

        DateTimeOffset updateTime = DateTimeOffset.UtcNow.AddMinutes(5);

        // Act
        article.Update("  Updated  ", updateTime, new List<Guid> { tagB, tagC, tagB });

        // Assert
        Assert.Equal("Updated", article.Title);
        Assert.Equal(updateTime, article.UpdatedAtUtc);
        Assert.Equal(2, article.Tags.Count);
        Assert.Equal(tagB, article.Tags[0].TagId);
        Assert.Equal(tagC, article.Tags[1].TagId);
    }

    [Fact]
    public void ComputeSectionKey_ShouldBeIndependentOfTagOrder()
    {
        // Arrange
        Guid tagA = Guid.NewGuid();
        Guid tagB = Guid.NewGuid();

        Article article1 = Article.Create(
            Guid.NewGuid(),
            "Article 1",
            DateTimeOffset.UtcNow,
            new List<Guid> { tagA, tagB });

        Article article2 = Article.Create(
            Guid.NewGuid(),
            "Article 2",
            DateTimeOffset.UtcNow,
            new List<Guid> { tagB, tagA });

        string Resolver(Guid id) => id == tagA ? "a" : "b";

        // Act
        string key1 = article1.ComputeSectionKey(Resolver);
        string key2 = article2.ComputeSectionKey(Resolver);

        // Assert
        Assert.Equal(key1, key2);
    }
}
