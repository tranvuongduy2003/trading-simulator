using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TradingSimulator.Application.Abstractions.Auth;

namespace TradingSimulator.Infrastructure.Auth;

internal sealed class SessionRedisCache(
    IServiceProvider serviceProvider,
    ILogger<SessionRedisCache> logger) : ISessionRedisCache
{
    public async Task TryWriteAsync(
        PendingSessionCacheEntry entry,
        CancellationToken cancellationToken = default)
    {
        var multiplexer = serviceProvider.GetService<IConnectionMultiplexer>();
        if (multiplexer is null)
        {
            return;
        }

        try
        {
            var timeToLive = entry.ExpiresAt - DateTimeOffset.UtcNow;
            if (timeToLive <= TimeSpan.Zero)
            {
                return;
            }

            await multiplexer
                .GetDatabase()
                .StringSetAsync(
                    SessionRedisKeys.Session(entry.SessionId),
                    entry.UserId.ToString("D"),
                    timeToLive);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to cache session {SessionId} in Redis",
                entry.SessionId);
        }
    }
}
