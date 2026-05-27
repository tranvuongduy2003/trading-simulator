using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;
using TradingSimulator.Infrastructure.Persistence;

namespace TradingSimulator.Api.IntegrationTests.Portfolios.Fakes;

internal sealed class DelayingWalletReadRepository(ApplicationDatabaseContext databaseContext) : IWalletReadRepository
{
    public async Task<WalletReadModel?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(300, cancellationToken);

        return await (
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
}
