using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class PortfolioResetWriteRepository(ApplicationDatabaseContext databaseContext)
    : IPortfolioResetWriteRepository
{
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
            wallet.Currency);
    }
}
