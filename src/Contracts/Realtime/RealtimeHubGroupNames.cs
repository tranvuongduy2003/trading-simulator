namespace TradingSimulator.Contracts.Realtime;

public static class RealtimeHubGroupNames
{
    public const string MarketGroupPrefix = "market";

    public const string UserGroupPrefix = "user";

    public static string MarketForSymbol(string symbol) => $"{MarketGroupPrefix}:{symbol}";

    public static string UserForIdentifier(Guid userIdentifier) => $"{UserGroupPrefix}:{userIdentifier}";
}
