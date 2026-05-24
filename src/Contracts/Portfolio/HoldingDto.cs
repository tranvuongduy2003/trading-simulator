namespace TradingSimulator.Contracts.Portfolio;

public sealed record HoldingDto(
    string Symbol,
    long TotalQuantity,
    long ReservedQuantity,
    long AvailableQuantity,
    decimal AveragePrice);
