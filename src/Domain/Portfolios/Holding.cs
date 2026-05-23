namespace TradingSimulator.Domain.Portfolios;

public sealed class Holding
{
    private Holding()
    {
    }

    public PortfolioId PortfolioId { get; private set; }

    public string Symbol { get; private set; } = string.Empty;

    public long TotalQuantity { get; private set; }

    public long ReservedQuantity { get; private set; }

    public decimal AveragePrice { get; private set; }
}
