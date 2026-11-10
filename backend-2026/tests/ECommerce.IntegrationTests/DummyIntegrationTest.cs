using FluentAssertions;
using ECommerce.IntegrationTests.Infrastructure;
using Xunit;

namespace ECommerce.IntegrationTests;

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
