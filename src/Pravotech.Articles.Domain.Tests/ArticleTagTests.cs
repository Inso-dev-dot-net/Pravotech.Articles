using Pravotech.Articles.Domain.Entities;


namespace Pravotech.Articles.Domain.Tests.Entities;

public sealed class ArticleTagTests
{
    [Fact]
    public void Create_ValidInput_ShouldSetTagIdAndPosition()
    {
        // Arrange
        Guid tagId = Guid.NewGuid();
        int position = 0;

        // Act
        ArticleTag articleTag = ArticleTag.Create(tagId, position);

        // Assert
        Assert.Equal(tagId, articleTag.TagId);
        Assert.Equal(position, articleTag.Position);
    }

    [Fact]
    public void Create_NegativePosition_ShouldThrow()
    {
        // Arrange
        Guid tagId = Guid.NewGuid();

        // Act - Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => ArticleTag.Create(tagId, -1));
    }

    [Fact]
    public void Create_EmptyTagId_ShouldThrow()
    {
        // Arrange
        Guid tagId = Guid.Empty;

        // Act - Assert
        Assert.Throws<ArgumentException>(() => ArticleTag.Create(tagId, 0));
    }
}
