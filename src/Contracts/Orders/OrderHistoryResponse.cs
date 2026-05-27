namespace TradingSimulator.Contracts.Orders;

public sealed record OrderHistoryResponse(
    int PageNumber,
    int PageSize,
    int TotalCount,
    IReadOnlyList<OrderHistoryItemDto> Items);
