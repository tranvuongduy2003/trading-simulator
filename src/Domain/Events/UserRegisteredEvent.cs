using TradingSimulator.Domain.Abstractions;
using TradingSimulator.Domain.Portfolios;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Domain.Events;

public sealed record UserRegisteredEvent(
    UserId UserId,
    PortfolioId PortfolioId,
    Username Username,
    EmailAddress Email) : DomainEvent;
