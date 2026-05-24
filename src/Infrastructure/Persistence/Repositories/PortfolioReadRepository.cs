using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class PortfolioReadRepository(ApplicationDatabaseContext databaseContext) : IPortfolioReadRepository
{
    public async Task<PortfolioReadModel?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await databaseContext.Portfolios
            .AsNoTracking()
            .Where(portfolioRecord => portfolioRecord.UserId == userId.Value)
            .Select(portfolioRecord => new
            {
                portfolioRecord.Id,
                portfolioRecord.UserId,
                Holdings = portfolioRecord.Holdings
                    .Select(holding => new PortfolioHoldingReadModel(
                        holding.Symbol,
                        holding.TotalQuantity,
                        holding.ReservedQuantity,
                        holding.AveragePrice))
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return portfolio is null
            ? null
            : new PortfolioReadModel(portfolio.Id, portfolio.UserId, portfolio.Holdings);
    }
}
