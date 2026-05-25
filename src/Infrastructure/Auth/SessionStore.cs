using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Cache;
using TradingSimulator.Application.Abstractions.Services;
using TradingSimulator.Application.Options;
using TradingSimulator.Domain.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Auth;

internal sealed class SessionStore(
    ApplicationDatabaseContext databaseContext,
    IClock clock,
    IOptions<TradingSessionOptions> sessionOptions,
    ICacheService cacheService,
    ILogger<SessionStore> logger) : ISessionStore
{
    private static string SessionKey(Guid sessionId) => $"session:{sessionId:D}";

    public async Task<SessionCreationResult> CreateSessionAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid();
        var createdAt = clock.UtcNow;
        var expiresAt = createdAt.AddHours(sessionOptions.Value.ExpirationHours);

        var record = new UserSessionRecord
        {
            Id = sessionId,
            UserId = userId.Value,
            CreatedAt = createdAt,
            LastSeenAt = createdAt,
            ExpiresAt = expiresAt,
        };

        await databaseContext.UserSessions.AddAsync(record, cancellationToken);

        return new SessionCreationResult(sessionId, expiresAt);
    }

    public async Task<UserId?> ResolveUserIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var cacheHadEntry = await cacheService.KeyExistsAsync(SessionKey(sessionId), cancellationToken);

        var now = clock.UtcNow;
        var session = await databaseContext.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                userSession =>
                    userSession.Id == sessionId
                    && userSession.RevokedAt == null
                    && userSession.ExpiresAt > now,
                cancellationToken);

        if (session is not null)
        {
            return UserId.From(session.UserId);
        }

        if (cacheHadEntry)
        {
            await cacheService.DeleteAsync(SessionKey(sessionId), cancellationToken);
        }

        return null;
    }

    public async Task TryWriteCacheAsync(
        PendingSessionCacheEntry entry,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var timeToLive = entry.ExpiresAt - DateTimeOffset.UtcNow;
            if (timeToLive <= TimeSpan.Zero)
            {
                return;
            }

            await cacheService.SetAsync(
                SessionKey(entry.SessionId),
                entry.UserId.ToString("D"),
                timeToLive,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to write session {SessionId} to cache",
                entry.SessionId);
        }
    }
}
