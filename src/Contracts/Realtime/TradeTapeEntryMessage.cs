namespace TradingSimulator.Contracts.Realtime;

public sealed record TradeTapeEntryMessage(
    string Symbol,
    Guid TradeIdentifier,
    decimal Price,
    decimal Quantity,
    DateTimeOffset ExecutedAt);
