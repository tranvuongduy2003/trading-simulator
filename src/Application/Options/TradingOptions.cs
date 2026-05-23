namespace TradingSimulator.Application.Options;

public sealed class TradingOptions
{
    public const string SectionName = "Trading";

    public decimal InitialVirtualCash { get; set; } = 100_000m;
}
