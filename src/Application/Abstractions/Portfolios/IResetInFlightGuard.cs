using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Portfolios;

public interface IResetInFlightGuard
{
    bool TryBegin(UserId userId);

    void End(UserId userId);
}
