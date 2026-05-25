using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Api.IntegrationTests.Users.Fakes;

internal sealed class ThrowOnWalletReadRepository : IWalletReadRepository
{
    public Task<WalletReadModel?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Simulated wallet read failure.");
}
