namespace TradingSimulator.Contracts.Market;

public sealed record OrderBookLevelResponse(
    decimal Price,
    long Quantity,
    int OrderCount);
