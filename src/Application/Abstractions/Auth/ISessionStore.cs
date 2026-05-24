using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Auth;

public interface ISessionStore
{
    Task<SessionCreationResult> CreateSessionAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<UserId?> ResolveUserIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
