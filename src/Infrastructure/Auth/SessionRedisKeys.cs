namespace TradingSimulator.Infrastructure.Auth;

internal static class SessionRedisKeys
{
    public static string Session(Guid sessionId) => $"session:{sessionId:D}";
}
