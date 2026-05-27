namespace TradingSimulator.Contracts.Orders;

public sealed record OrderHistoryItemDto(
    Guid OrderId,
    string Symbol,
    string Side,
    string Type,
    decimal? Price,
    long OriginalQuantity,
    long FilledQuantity,
    long RemainingQuantity,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? TerminatedAt);
