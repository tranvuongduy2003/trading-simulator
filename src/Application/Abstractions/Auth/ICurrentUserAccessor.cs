using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Auth;

public interface ICurrentUserAccessor
{
    UserId? UserId { get; }

    bool IsAuthenticated { get; }
}
