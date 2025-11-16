using Pravotech.Articles.Domain.ValueObjects;


namespace Pravotech.Articles.Domain.Tests.ValueObjects;

public sealed class TagNameTests
{
    [Fact]
    public void Ctor_ValidValue_ShouldSetValueAndNormalized()
    {
        // Arrange
        string raw = "  CSharp  ";

        // Act
        TagName tagName = new TagName(raw);

        // Assert
        Assert.Equal("CSharp", tagName.Value);
        Assert.Equal("csharp", tagName.Normalized);
    }

    [Fact]
    public void Ctor_EmptyValue_ShouldThrowArgumentException()
    {
        // Arrange
        string raw = "   ";

        // Act - Assert
        Assert.Throws<ArgumentException>(() => new TagName(raw));
    }

    [Fact]
    public void Ctor_TooLongValue_ShouldThrowArgumentException()
    {
        // Arrange
        string raw = new string('a', 300);

        // Act - Assert
        Assert.Throws<ArgumentException>(() => new TagName(raw));
    }
}
