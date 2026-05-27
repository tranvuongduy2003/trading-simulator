using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class TradeReadRepository(ApplicationDatabaseContext databaseContext) : ITradeReadRepository
{
    public async Task<TradeHistoryPageReadModel> GetTradeHistoryAsync(
        UserId userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var latestResetAt = await GetLatestResetAtAsync(userId, cancellationToken);

        var query = databaseContext.Trades
            .AsNoTracking()
            .Where(tradeRecord =>
                tradeRecord.BuyerUserId == userId.Value
                || tradeRecord.SellerUserId == userId.Value);

        if (latestResetAt is not null)
        {
            query = query.Where(tradeRecord => tradeRecord.ExecutedAt >= latestResetAt.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var skippedCount = (pageNumber - 1) * pageSize;

        var items = await query
            .OrderByDescending(tradeRecord => tradeRecord.ExecutedAt)
            .Skip(skippedCount)
            .Take(pageSize)
            .Select(tradeRecord => new TradeHistoryItemReadModel(
                tradeRecord.ExternalId,
                tradeRecord.Symbol,
                tradeRecord.BuyerUserId == userId.Value,
                tradeRecord.Price,
                tradeRecord.Quantity,
                tradeRecord.ExecutedAt))
            .ToListAsync(cancellationToken);

        return new TradeHistoryPageReadModel(totalCount, items);
    }

    private async Task<DateTimeOffset?> GetLatestResetAtAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        return await databaseContext.PortfolioResets
            .AsNoTracking()
            .Where(portfolioResetRecord => portfolioResetRecord.UserId == userId.Value)
            .OrderByDescending(portfolioResetRecord => portfolioResetRecord.ResetAt)
            .Select(portfolioResetRecord => (DateTimeOffset?)portfolioResetRecord.ResetAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
