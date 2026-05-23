namespace TradingSimulator.Infrastructure.Persistence.Entities;

public sealed class SymbolRecord
{
    public required string Code { get; set; }

    public required string Name { get; set; }

    public bool IsActive { get; set; }

    public decimal TickSize { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
