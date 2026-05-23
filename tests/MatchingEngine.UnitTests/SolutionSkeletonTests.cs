using FluentAssertions;

namespace TradingSimulator.MatchingEngine.UnitTests;

public class SolutionSkeletonTests
{
    [Fact]
    public void SolutionSkeleton_IsReadyForMatchingEngineWork()
    {
        true.Should().BeTrue();
    }
}
