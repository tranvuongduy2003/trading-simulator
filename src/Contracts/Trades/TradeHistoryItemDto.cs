namespace TradingSimulator.Contracts.Trades;

public sealed record TradeHistoryItemDto(
    Guid TradeId,
    string Symbol,
    string Side,
    decimal Price,
    long Quantity,
    DateTimeOffset ExecutedAt);
