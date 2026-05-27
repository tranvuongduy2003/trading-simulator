using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IPortfolioRepository
{
    Task<PortfolioResetWriteModel?> ResetForUserAsync(
        UserId userId,
        decimal initialVirtualCash,
        DateTimeOffset resetAt,
        CancellationToken cancellationToken = default);
}

public sealed record PortfolioResetWriteModel(
    decimal TotalBalance,
    decimal ReservedBalance,
    decimal AvailableBalance,
    string Currency);
