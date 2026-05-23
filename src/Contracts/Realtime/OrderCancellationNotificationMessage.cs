namespace TradingSimulator.Contracts.Realtime;

public sealed record OrderCancellationNotificationMessage(
    Guid OrderIdentifier,
    string Symbol,
    DateTimeOffset CancelledAt);
