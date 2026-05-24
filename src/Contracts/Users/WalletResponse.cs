namespace TradingSimulator.Contracts.Users;

public sealed record WalletResponse(
    Guid UserId,
    string Username,
    string Currency,
    decimal TotalBalance,
    decimal ReservedBalance,
    decimal AvailableBalance);
