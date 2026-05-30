namespace TradingSimulator.Infrastructure.Cache;

internal static class RecentTradesCacheKeys
{
    public static string ForSymbol(string symbol) => $"trades:{symbol}:recent";
}
