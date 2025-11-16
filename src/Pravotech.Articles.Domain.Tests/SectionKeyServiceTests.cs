using Pravotech.Articles.Domain.Services;

namespace Pravotech.Articles.Domain.Tests.Services;

public sealed class SectionKeyServiceTests
{
    [Fact]
    public void BuildSectionKey_ShouldSortAndDistinctNames()
    {
        // Arrange
        ISectionKeyService service = new SectionKeyService();
        List<string> names = new List<string> { "tag2", "tag1", "tag2" };

        // Act
        string key = service.BuildSectionKey(names);

        // Assert
        Assert.Equal("tag1|tag2", key);
    }

    [Fact]
    public void BuildSectionId_SameKey_ShouldProduceSameGuid()
    {
        // Arrange
        ISectionKeyService service = new SectionKeyService();
        string key = "tag1|tag2";

        // Act
        Guid id1 = service.BuildSectionId(key);
        Guid id2 = service.BuildSectionId(key);

        // Assert
        Assert.Equal(id1, id2);
    }
}
