namespace TradingSimulator.Infrastructure.Persistence.Entities;

public sealed class HoldingRecord
{
    public Guid PortfolioId { get; set; }

    public required string Symbol { get; set; }

    public long TotalQuantity { get; set; }

    public long ReservedQuantity { get; set; }

    public decimal AveragePrice { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PortfolioRecord? Portfolio { get; set; }

    public SymbolRecord? SymbolNavigation { get; set; }
}
