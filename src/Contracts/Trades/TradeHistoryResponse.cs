namespace TradingSimulator.Contracts.Trades;

public sealed record TradeHistoryResponse(
    int PageNumber,
    int PageSize,
    int TotalCount,
    IReadOnlyList<TradeHistoryItemDto> Items);
