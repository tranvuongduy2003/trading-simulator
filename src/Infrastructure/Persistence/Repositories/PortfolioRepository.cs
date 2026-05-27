using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class PortfolioRepository(ApplicationDatabaseContext databaseContext) : IPortfolioRepository
{
    public async Task<PortfolioResetWriteModel?> ResetForUserAsync(
        UserId userId,
        decimal initialVirtualCash,
        DateTimeOffset resetAt,
        CancellationToken cancellationToken = default)
    {
        var wallet = await databaseContext.Wallets
            .SingleOrDefaultAsync(record => record.UserId == userId.Value, cancellationToken);

        if (wallet is null)
        {
            return null;
        }

        var portfolio = await databaseContext.Portfolios
            .SingleOrDefaultAsync(record => record.UserId == userId.Value, cancellationToken);

        if (portfolio is null)
        {
            return null;
        }

        wallet.TotalBalance = initialVirtualCash;
        wallet.ReservedBalance = 0m;
        wallet.UpdatedAt = resetAt;

        var holdings = await databaseContext.Holdings
            .Where(record => record.PortfolioId == portfolio.Id)
            .ToListAsync(cancellationToken);
        if (holdings.Count > 0)
        {
            databaseContext.Holdings.RemoveRange(holdings);
        }

        portfolio.UpdatedAt = resetAt;

        databaseContext.PortfolioResets.Add(
            new PortfolioResetRecord
            {
                UserId = userId.Value,
                ResetAt = resetAt,
                Reason = "user_initiated"
            });

        return new PortfolioResetWriteModel(
            wallet.TotalBalance,
            wallet.ReservedBalance,
            wallet.TotalBalance - wallet.ReservedBalance,
            wallet.Currency);
    }
}
