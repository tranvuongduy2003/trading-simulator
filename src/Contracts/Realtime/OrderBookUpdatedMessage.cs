namespace TradingSimulator.Contracts.Realtime;

public sealed record OrderBookUpdatedMessage(
    string Symbol,
    IReadOnlyList<OrderBookLevelMessage> Bids,
    IReadOnlyList<OrderBookLevelMessage> Asks,
    DateTimeOffset UpdatedAt);

public sealed record OrderBookLevelMessage(decimal Price, decimal Quantity, int OrderCount);
