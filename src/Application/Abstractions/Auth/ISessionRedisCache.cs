namespace TradingSimulator.Application.Abstractions.Auth;

public interface ISessionRedisCache
{
    Task TryWriteAsync(PendingSessionCacheEntry entry, CancellationToken cancellationToken = default);
}
