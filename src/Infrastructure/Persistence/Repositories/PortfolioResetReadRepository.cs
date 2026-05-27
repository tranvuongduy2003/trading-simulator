using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class PortfolioResetReadRepository(ApplicationDatabaseContext databaseContext)
    : IPortfolioResetReadRepository
{
    public Task<DateTimeOffset?> GetLatestResetAtByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default) =>
        databaseContext.PortfolioResets
            .AsNoTracking()
            .Where(portfolioReset => portfolioReset.UserId == userId.Value)
            .OrderByDescending(portfolioReset => portfolioReset.ResetAt)
            .Select(portfolioReset => (DateTimeOffset?)portfolioReset.ResetAt)
            .FirstOrDefaultAsync(cancellationToken);
}
