using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class PortfolioResetWriteRepository(ApplicationDatabaseContext databaseContext)
    : IPortfolioResetWriteRepository
{
    private const short BuySide = 0;
    private const short SellSide = 1;
    private const short LimitOrderType = 0;
    private const short PendingStatus = 0;
    private const short PartiallyFilledStatus = 1;
    private const short CancelledStatus = 3;

    public Task<PortfolioResetWalletReadModel?> GetWalletByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default) =>
        databaseContext.Wallets
            .AsNoTracking()
            .Where(wallet => wallet.UserId == userId.Value)
            .Select(wallet => new PortfolioResetWalletReadModel(
                wallet.UserId,
                wallet.Currency,
                wallet.TotalBalance,
                wallet.ReservedBalance))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PortfolioResetWriteModel?> ResetForUserAsync(
        UserId userId,
        decimal initialVirtualCash,
        DateTimeOffset resetAt,
        CancellationToken cancellationToken = default)
    {
        var portfolioId = await databaseContext.Portfolios
            .AsNoTracking()
            .Where(portfolioRecord => portfolioRecord.UserId == userId.Value)
            .Select(portfolioRecord => (Guid?)portfolioRecord.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (portfolioId is null)
        {
            return null;
        }

        var wallet = await databaseContext.Wallets
            .AsTracking()
            .SingleOrDefaultAsync(walletRecord => walletRecord.UserId == userId.Value, cancellationToken);

        if (wallet is null)
        {
            return null;
        }

        var userOpenOrders = await databaseContext.Orders
            .AsTracking()
            .Where(orderRecord =>
                orderRecord.UserId == userId.Value
                && (orderRecord.Status == PendingStatus || orderRecord.Status == PartiallyFilledStatus))
            .ToListAsync(cancellationToken);

        foreach (var order in userOpenOrders)
        {
            var remainingQuantity = Math.Max(order.OriginalQuantity - order.FilledQuantity, 0);
            if (remainingQuantity <= 0)
            {
                continue;
            }

            if (order.Side == BuySide && order.Type == LimitOrderType && order.Price is not null)
            {
                var releasedAmount = remainingQuantity * order.Price.Value;
                wallet.ReservedBalance = Math.Max(wallet.ReservedBalance - releasedAmount, 0m);
            }
        }

        var releasedSellQuantityBySymbol = userOpenOrders
            .Where(order => order.Side == SellSide)
            .Select(order => new
            {
                order.Symbol,
                RemainingQuantity = Math.Max(order.OriginalQuantity - order.FilledQuantity, 0),
            })
            .Where(order => order.RemainingQuantity > 0)
            .GroupBy(order => order.Symbol)
            .ToDictionary(group => group.Key, group => group.Sum(order => order.RemainingQuantity));

        foreach (var releasedSellQuantity in releasedSellQuantityBySymbol)
        {
            await databaseContext.Holdings
                .Where(holdingRecord =>
                    holdingRecord.PortfolioId == portfolioId.Value
                    && holdingRecord.Symbol == releasedSellQuantity.Key)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            holdingRecord => holdingRecord.ReservedQuantity,
                            holdingRecord => holdingRecord.ReservedQuantity > releasedSellQuantity.Value
                                ? holdingRecord.ReservedQuantity - releasedSellQuantity.Value
                                : 0)
                        .SetProperty(holdingRecord => holdingRecord.UpdatedAt, resetAt),
                    cancellationToken);
        }

        var cancelledOrders = new List<PortfolioResetCancelledOrderWriteModel>(userOpenOrders.Count);

        foreach (var order in userOpenOrders)
        {
            order.Status = CancelledStatus;
            order.TerminatedAt = resetAt;
            order.UpdatedAt = resetAt;
            cancelledOrders.Add(new PortfolioResetCancelledOrderWriteModel(order.Id, order.Symbol));
        }

        wallet.TotalBalance = initialVirtualCash;
        wallet.ReservedBalance = 0m;
        wallet.UpdatedAt = resetAt;

        await databaseContext.Holdings
            .Where(holdingRecord => holdingRecord.PortfolioId == portfolioId.Value)
            .ExecuteDeleteAsync(cancellationToken);

        await databaseContext.PortfolioResets.AddAsync(
            new PortfolioResetRecord
            {
                UserId = userId.Value,
                ResetAt = resetAt,
                Reason = "user_initiated",
            },
            cancellationToken);

        return new PortfolioResetWriteModel(
            wallet.TotalBalance,
            wallet.ReservedBalance,
            wallet.TotalBalance - wallet.ReservedBalance,
            wallet.Currency,
            cancelledOrders);
    }
}
