namespace TradingSimulator.Contracts.Users;

public sealed record WalletSummaryDto(
    string Currency,
    decimal TotalBalance,
    decimal ReservedBalance,
    decimal AvailableBalance);
