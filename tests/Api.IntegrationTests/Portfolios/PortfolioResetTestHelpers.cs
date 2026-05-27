using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Infrastructure.Persistence.Entities;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Portfolios;

internal static class PortfolioResetTestHelpers
{
    public static async Task SeedWalletBalancesAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        decimal totalBalance,
        decimal reservedBalance,
        CancellationToken cancellationToken = default)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var rowsUpdated = await databaseContext.Wallets
            .Where(walletRecord => walletRecord.UserId == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(walletRecord => walletRecord.TotalBalance, totalBalance)
                    .SetProperty(walletRecord => walletRecord.ReservedBalance, reservedBalance),
                cancellationToken);

        rowsUpdated.Should().Be(1);
    }

    public static async Task SeedHoldingAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        string symbol,
        long quantity,
        decimal averagePrice,
        CancellationToken cancellationToken = default)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var portfolioId = await databaseContext.Portfolios
            .AsNoTracking()
            .Where(portfolioRecord => portfolioRecord.UserId == userId)
            .Select(portfolioRecord => portfolioRecord.Id)
            .SingleAsync(cancellationToken);

        await databaseContext.Holdings.AddAsync(
            new HoldingRecord
            {
                PortfolioId = portfolioId,
                Symbol = symbol,
                TotalQuantity = quantity,
                ReservedQuantity = 0,
                AveragePrice = averagePrice,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            cancellationToken);

        await databaseContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task<int> CountPortfolioResetsAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        return await databaseContext.PortfolioResets
            .CountAsync(portfolioReset => portfolioReset.UserId == userId, cancellationToken);
    }
}
