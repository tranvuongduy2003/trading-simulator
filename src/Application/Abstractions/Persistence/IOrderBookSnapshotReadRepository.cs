namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IOrderBookSnapshotReadRepository
{
    Task<OrderBookSnapshotReadModel> GetSnapshotAsync(
        string symbol,
        int depth,
        CancellationToken cancellationToken = default);
}

public sealed record OrderBookSnapshotReadModel(
    string Symbol,
    IReadOnlyList<OrderBookLevelReadModel> Bids,
    IReadOnlyList<OrderBookLevelReadModel> Asks,
    DateTimeOffset UpdatedAt);

public sealed record OrderBookLevelReadModel(
    decimal Price,
    long Quantity,
    int OrderCount);
