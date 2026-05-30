namespace TradingSimulator.Contracts.Market;

public sealed record RecentTradeItemResponse(
    Guid TradeIdentifier,
    decimal Price,
    long Quantity,
    DateTimeOffset ExecutedAt);
