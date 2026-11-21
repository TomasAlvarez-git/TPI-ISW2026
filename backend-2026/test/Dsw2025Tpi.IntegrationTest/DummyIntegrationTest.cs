using FluentAssertions;
using Dsw2025Tpi.IntegrationTest.Infrastructure;
using Xunit;

namespace Dsw2025Tpi.IntegrationTest;

[Collection("IntegrationTests")]
public class DummyIntegrationTest
{
    private readonly CustomWebApplicationFactory _factory;

    public DummyIntegrationTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void DummyVerificationTest_HostAndDatabase_ShouldInitializeSuccessfully()
    {
        // Act
        var client = _factory.CreateClient();

        // Assert
        client.Should().NotBeNull();
        true.Should().BeTrue();
    }
}

