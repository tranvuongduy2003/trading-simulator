using System.Collections.Concurrent;
using TradingSimulator.Application.Abstractions.Portfolios;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Infrastructure.Portfolios;

internal sealed class InMemoryResetInFlightGuard : IResetInFlightGuard
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _gates = new();

    public bool TryBegin(UserId userId)
    {
        var gate = _gates.GetOrAdd(userId.Value, static _ => new SemaphoreSlim(1, 1));
        return gate.Wait(0);
    }

    public void End(UserId userId)
    {
        if (_gates.TryGetValue(userId.Value, out var gate))
        {
            gate.Release();
        }
    }
}
