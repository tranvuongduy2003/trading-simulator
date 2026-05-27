using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class OrderReadRepository(ApplicationDatabaseContext databaseContext) : IOrderReadRepository
{
    private const short PendingStatus = 0;
    private const short PartiallyFilledStatus = 1;

    public async Task<IReadOnlyList<OpenOrderReadModel>> GetOpenOrdersAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var latestResetAt = await GetLatestResetAtAsync(userId, cancellationToken);

        var query = databaseContext.Orders
            .AsNoTracking()
            .Where(orderRecord =>
                orderRecord.UserId == userId.Value
                && (orderRecord.Status == PendingStatus || orderRecord.Status == PartiallyFilledStatus));

        if (latestResetAt is not null)
        {
            query = query.Where(orderRecord => orderRecord.CreatedAt >= latestResetAt.Value);
        }

        return await query
            .OrderByDescending(orderRecord => orderRecord.CreatedAt)
            .Select(orderRecord => new OpenOrderReadModel(
                orderRecord.Id,
                orderRecord.Symbol,
                orderRecord.Side,
                orderRecord.Type,
                orderRecord.Price,
                orderRecord.OriginalQuantity,
                orderRecord.FilledQuantity,
                orderRecord.Status,
                orderRecord.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderHistoryPageReadModel> GetOrderHistoryAsync(
        UserId userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var latestResetAt = await GetLatestResetAtAsync(userId, cancellationToken);

        var query = databaseContext.Orders
            .AsNoTracking()
            .Where(orderRecord =>
                orderRecord.UserId == userId.Value
                && orderRecord.Status != PendingStatus
                && orderRecord.Status != PartiallyFilledStatus);

        if (latestResetAt is not null)
        {
            query = query.Where(orderRecord => orderRecord.CreatedAt >= latestResetAt.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var skippedCount = (pageNumber - 1) * pageSize;

        var items = await query
            .OrderByDescending(orderRecord => orderRecord.CreatedAt)
            .Skip(skippedCount)
            .Take(pageSize)
            .Select(orderRecord => new OrderHistoryItemReadModel(
                orderRecord.Id,
                orderRecord.Symbol,
                orderRecord.Side,
                orderRecord.Type,
                orderRecord.Price,
                orderRecord.OriginalQuantity,
                orderRecord.FilledQuantity,
                orderRecord.Status,
                orderRecord.CreatedAt,
                orderRecord.TerminatedAt))
            .ToListAsync(cancellationToken);

        return new OrderHistoryPageReadModel(totalCount, items);
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
