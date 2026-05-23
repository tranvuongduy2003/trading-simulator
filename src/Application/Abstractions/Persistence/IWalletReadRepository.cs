using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IWalletReadRepository
{
    Task<WalletReadModel?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}

public sealed record WalletReadModel(
    Guid UserId,
    string Username,
    string Currency,
    decimal TotalBalance,
    decimal ReservedBalance);
