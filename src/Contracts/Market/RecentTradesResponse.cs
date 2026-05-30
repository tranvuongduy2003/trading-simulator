namespace TradingSimulator.Contracts.Market;

public sealed record RecentTradesResponse(
    string Symbol,
    IReadOnlyList<RecentTradeItemResponse> Trades,
    DateTimeOffset UpdatedAt);
