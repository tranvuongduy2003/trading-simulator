using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;
using TradingSimulator.Infrastructure.Persistence;

namespace TradingSimulator.Api.IntegrationTests.Portfolios.Fakes;

internal sealed class FailingPortfolioRepository(ApplicationDatabaseContext databaseContext) : IPortfolioRepository
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

        wallet.TotalBalance = initialVirtualCash;
        wallet.ReservedBalance = 0m;
        wallet.UpdatedAt = resetAt;

        throw new InvalidOperationException("Deterministic test failure during portfolio reset mutation.");
    }
}
