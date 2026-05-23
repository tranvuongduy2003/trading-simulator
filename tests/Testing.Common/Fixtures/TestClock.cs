using TradingSimulator.Application.Abstractions.Services;

namespace TradingSimulator.Testing.Common.Fixtures;

public sealed class TestClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
}
