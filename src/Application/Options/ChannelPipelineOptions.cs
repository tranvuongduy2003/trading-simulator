namespace TradingSimulator.Application.Options;

public sealed class ChannelPipelineOptions
{
    public const string SectionName = "Channels";

    public int IncomingOrderCapacity { get; set; } = 1000;
}
