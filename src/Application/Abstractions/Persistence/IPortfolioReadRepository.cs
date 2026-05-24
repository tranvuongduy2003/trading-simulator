using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IPortfolioReadRepository
{
    Task<PortfolioReadModel?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}

public sealed record PortfolioReadModel(
    Guid PortfolioId,
    Guid UserId,
    IReadOnlyList<PortfolioHoldingReadModel> Holdings);

public sealed record PortfolioHoldingReadModel(
    string Symbol,
    long TotalQuantity,
    long ReservedQuantity,
    decimal AveragePrice);
