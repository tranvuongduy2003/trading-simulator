namespace TradingSimulator.Contracts.Realtime;

public sealed record LastTradePriceMessage(
    string Symbol,
    decimal Price,
    DateTimeOffset UpdatedAt);
