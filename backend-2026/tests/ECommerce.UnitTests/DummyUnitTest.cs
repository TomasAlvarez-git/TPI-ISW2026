using FluentAssertions;
using Xunit;

namespace ECommerce.UnitTests;

public class DummyUnitTest
{
    [Fact]
    public void DummyVerificationTest_ShouldPass()
    {
        // Assert
        true.Should().BeTrue();
    }
}

