using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Api.IntegrationTests.Portfolios.Fakes;

internal sealed class DelayingPortfolioResetWriteRepository : IPortfolioResetWriteRepository
{
    public Task<PortfolioResetWalletReadModel?> GetWalletByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<PortfolioResetWalletReadModel?>(
            new PortfolioResetWalletReadModel(
                userId.Value,
                "USD",
                100_000m,
                0m));

    public async Task<PortfolioResetWriteModel?> ResetForUserAsync(
        UserId userId,
        decimal initialVirtualCash,
        DateTimeOffset resetAt,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(300, cancellationToken);

        return new PortfolioResetWriteModel(
            initialVirtualCash,
            0m,
            initialVirtualCash,
            "USD",
            []);
    }
}
