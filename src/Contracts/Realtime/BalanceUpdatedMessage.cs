namespace TradingSimulator.Contracts.Realtime;

public sealed record BalanceUpdatedMessage(
    Guid UserIdentifier,
    decimal TotalCash,
    decimal ReservedCash,
    decimal AvailableCash,
    DateTimeOffset UpdatedAt);
