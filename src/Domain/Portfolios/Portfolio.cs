using TradingSimulator.Domain.Abstractions;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Domain.Portfolios;

public sealed class Portfolio : AggregateRoot<PortfolioId>
{
    private readonly List<Holding> _holdings = [];

    private Portfolio()
    {
    }

    public UserId UserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<Holding> Holdings => _holdings.AsReadOnly();

    internal static Portfolio CreateForUser(UserId userId, DateTimeOffset createdAt)
    {
        var portfolioId = PortfolioId.New();

        return new Portfolio
        {
            Id = portfolioId,
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
    }
}
