using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Api.IntegrationTests.Portfolios.Fakes;

internal sealed class ThrowOnPortfolioResetWriteRepository : IPortfolioResetWriteRepository
{
    public Task<PortfolioResetWalletReadModel?> GetWalletByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<PortfolioResetWalletReadModel?>(null);

    public Task<PortfolioResetWriteModel?> ResetForUserAsync(
        UserId userId,
        decimal initialVirtualCash,
        DateTimeOffset resetAt,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Simulated portfolio reset write failure.");
}
