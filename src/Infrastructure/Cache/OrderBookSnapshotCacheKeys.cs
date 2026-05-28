namespace TradingSimulator.Infrastructure.Cache;

internal static class OrderBookSnapshotCacheKeys
{
    public static string ForSymbol(string symbol) => $"orderbook:{symbol}:snapshot";
}
