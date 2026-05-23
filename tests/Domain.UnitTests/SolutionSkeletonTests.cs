using FluentAssertions;

namespace TradingSimulator.Domain.UnitTests;

public class SolutionSkeletonTests
{
    [Fact]
    public void SolutionSkeleton_IsReadyForDomainWork()
    {
        true.Should().BeTrue();
    }
}
