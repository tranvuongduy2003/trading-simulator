using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class WalletReadRepository(ApplicationDatabaseContext databaseContext) : IWalletReadRepository
{
    public Task<WalletReadModel?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default) =>
        (
            from wallet in databaseContext.Wallets.AsNoTracking()
            join user in databaseContext.Users.AsNoTracking() on wallet.UserId equals user.Id
            where wallet.UserId == userId.Value
            select new WalletReadModel(
                wallet.UserId,
                user.Username,
                wallet.Currency,
                wallet.TotalBalance,
                wallet.ReservedBalance))
            .FirstOrDefaultAsync(cancellationToken);
}
