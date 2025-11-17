
namespace Pravotech.Articles.WebApi.Tests;

/// <summary>
/// Общий TestWebApplicationFactory между тестами
/// </summary>
[CollectionDefinition("Api collection")]
public sealed class ApiCollection : ICollectionFixture<TestWebApplicationFactory>
{
}
