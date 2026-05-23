namespace TradingSimulator.Infrastructure.Persistence.Entities;

public sealed class CandlestickRecord
{
    public required string Symbol { get; set; }

    public short Interval { get; set; }

    public DateTimeOffset BucketStart { get; set; }

    public decimal OpenPrice { get; set; }

    public decimal HighPrice { get; set; }

    public decimal LowPrice { get; set; }

    public decimal ClosePrice { get; set; }

    public long Volume { get; set; }

    public int TradeCount { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public SymbolRecord? SymbolNavigation { get; set; }
}
