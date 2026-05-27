namespace TradingSimulator.Contracts.Portfolio;

public sealed record PortfolioResetWalletSnapshot(
    decimal TotalBalance,
    decimal ReservedBalance,
    decimal AvailableBalance,
    string Currency);
