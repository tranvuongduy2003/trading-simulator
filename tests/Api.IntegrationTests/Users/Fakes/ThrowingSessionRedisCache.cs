using TradingSimulator.Application.Abstractions.Auth;

namespace TradingSimulator.Api.IntegrationTests.Users.Fakes;

internal sealed class ThrowingSessionRedisCache : ISessionRedisCache
{
    public Task TryWriteAsync(PendingSessionCacheEntry entry, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Simulated Redis cache write failure.");
}
