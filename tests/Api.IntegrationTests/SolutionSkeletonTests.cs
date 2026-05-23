using FluentAssertions;

namespace TradingSimulator.Api.IntegrationTests;

public class SolutionSkeletonTests
{
    [Fact]
    public void SolutionSkeleton_IsReadyForIntegrationTests()
    {
        true.Should().BeTrue();
    }
}
