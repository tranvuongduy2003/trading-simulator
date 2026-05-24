using TradingSimulator.Domain.Portfolios;

namespace TradingSimulator.Domain.Users;

public sealed record UserRegistrationResult(User User, Portfolio Portfolio);
