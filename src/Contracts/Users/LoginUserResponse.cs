namespace TradingSimulator.Contracts.Users;

public sealed record LoginUserResponse(Guid UserId, string Username, string Email);
