using System.Collections.Concurrent;
using TradingSimulator.Application.Abstractions.Realtime;

namespace TradingSimulator.Api.Realtime;

internal sealed class InMemoryRealtimeHubConnectionRegistry : IRealtimeHubConnectionRegistry
{
    private readonly ConcurrentDictionary<string, Guid?> _connections = new();

    public Task TrackConnectionAsync(
        string connectionIdentifier,
        Guid? userIdentifier,
        CancellationToken cancellationToken = default)
    {
        _connections[connectionIdentifier] = userIdentifier;
        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(
        string connectionIdentifier,
        CancellationToken cancellationToken = default)
    {
        _connections.TryRemove(connectionIdentifier, out _);
        return Task.CompletedTask;
    }
}
