using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TradingSimulator.Application.Abstractions.Auth;
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
    IServiceProvider serviceProvider,
    ILogger<SessionStore> logger) : ISessionStore
{
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
        var multiplexer = serviceProvider.GetService<IConnectionMultiplexer>();
        if (multiplexer is not null)
        {
            try
            {
                var cachedUserId = await multiplexer
                    .GetDatabase()
                    .StringGetAsync(SessionRedisKeys.Session(sessionId));

                if (cachedUserId.HasValue
                    && Guid.TryParse(cachedUserId.ToString(), out var userIdFromCache))
                {
                    return UserId.From(userIdFromCache);
                }
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Redis session lookup failed for {SessionId}; falling back to PostgreSQL",
                    sessionId);
            }
        }

        var now = clock.UtcNow;
        var session = await databaseContext.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                userSession =>
                    userSession.Id == sessionId
                    && userSession.RevokedAt == null
                    && userSession.ExpiresAt > now,
                cancellationToken);

        return session is null ? null : UserId.From(session.UserId);
    }
}
