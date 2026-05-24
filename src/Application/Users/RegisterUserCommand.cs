using TradingSimulator.Application.Abstractions.Messaging;

namespace TradingSimulator.Application.Users;

public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Password) : ICommand<RegisterUserResult>;

public sealed record RegisterUserResult(
    Guid UserId,
    string Username,
    string Email,
    DateTimeOffset CreatedAt,
    RegisterUserWalletResult Wallet,
    Guid SessionId,
    DateTimeOffset SessionExpiresAt);

public sealed record RegisterUserWalletResult(
    string Currency,
    decimal TotalBalance,
    decimal ReservedBalance,
    decimal AvailableBalance);
