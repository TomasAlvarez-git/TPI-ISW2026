using Xunit;

namespace Dsw2025Tpi.IntegrationTest.Infrastructure;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}

