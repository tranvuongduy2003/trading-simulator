using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IPortfolioResetReadRepository
{
    Task<DateTimeOffset?> GetLatestResetAtByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default);
}
