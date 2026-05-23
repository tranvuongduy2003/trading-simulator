namespace TradingSimulator.Contracts.Realtime;

public sealed record OrderFillNotificationMessage(
    Guid OrderIdentifier,
    string Symbol,
    decimal FilledQuantity,
    decimal AveragePrice,
    string OrderStatus,
    DateTimeOffset FilledAt);
