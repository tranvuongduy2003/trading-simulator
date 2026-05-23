using TradingSimulator.Application.Abstractions.Services;

namespace TradingSimulator.Application.Services;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
