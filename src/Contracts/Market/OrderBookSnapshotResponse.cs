namespace TradingSimulator.Contracts.Market;

public sealed record OrderBookSnapshotResponse(
    string Symbol,
    IReadOnlyList<OrderBookLevelResponse> Bids,
    IReadOnlyList<OrderBookLevelResponse> Asks,
    DateTimeOffset UpdatedAt);
