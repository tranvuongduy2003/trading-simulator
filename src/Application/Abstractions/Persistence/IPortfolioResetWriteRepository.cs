using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IPortfolioResetWriteRepository
{
    Task<PortfolioResetWalletReadModel?> GetWalletByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task<PortfolioResetWriteModel?> ResetForUserAsync(
        UserId userId,
        decimal initialVirtualCash,
        DateTimeOffset resetAt,
        CancellationToken cancellationToken = default);
}

public sealed record PortfolioResetWalletReadModel(
    Guid UserId,
    string Currency,
    decimal TotalBalance,
    decimal ReservedBalance);

public sealed record PortfolioResetWriteModel(
    decimal TotalBalance,
    decimal ReservedBalance,
    decimal AvailableBalance,
    string Currency,
    IReadOnlyList<PortfolioResetCancelledOrderWriteModel> CancelledOrders);

public sealed record PortfolioResetCancelledOrderWriteModel(
    Guid OrderId,
    string Symbol);
