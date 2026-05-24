namespace TradingSimulator.Application.Options;

public sealed class TradingSessionOptions
{
    public const string SectionName = "Session";

    public string CookieName { get; set; } = "TradingSimulator.Session";

    public int ExpirationHours { get; set; } = 24;
}
