using TradingSimulator.Domain.Exceptions;

namespace TradingSimulator.Domain.Portfolios;

public readonly record struct PortfolioId(Guid Value)
{
    public static PortfolioId New() => From(Guid.NewGuid());

    public static PortfolioId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new BusinessRuleValidationException(
                "PORTFOLIO_ID_EMPTY",
                "Portfolio id cannot be empty.");
        }

        return new PortfolioId(value);
    }

    public override string ToString() => Value.ToString("D");
}
