using Pravotech.Articles.Domain.Entities;
using Pravotech.Articles.Domain.ValueObjects;

namespace Pravotech.Articles.Domain.Tests.Entities;

public sealed class TagTests
{
    [Fact]
    public void Create_ValidInput_ShouldSetProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        TagName name = new TagName("Backend");

        // Act
        Tag tag = Tag.Create(id, name);

        // Assert
        Assert.Equal(id, tag.Id);
        Assert.Equal("Backend", tag.Name);
        Assert.Equal("backend", tag.NameNormalized);
    }

    [Fact]
    public void Create_EmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        Guid id = Guid.Empty;
        TagName name = new TagName("Tag");

        // Act - Assert
        Assert.Throws<ArgumentException>(() => Tag.Create(id, name));
    }
}
