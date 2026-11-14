using FluentAssertions;
using Xunit;

namespace Dsw2025Tpi.UnitTest;

public class DummyUnitTest
{
    [Fact]
    public void DummyVerificationTest_ShouldPass()
    {
        // Assert
        true.Should().BeTrue();
    }
}

