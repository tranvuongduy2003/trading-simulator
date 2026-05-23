namespace TradingSimulator.Contracts.Realtime;

public static class RealtimeHubClientMethodNames
{
    public const string SubscribeToMarket = nameof(SubscribeToMarket);

    public const string UnsubscribeFromMarket = nameof(UnsubscribeFromMarket);

    public const string SubscribeToUserNotifications = nameof(SubscribeToUserNotifications);

    public const string UnsubscribeFromUserNotifications = nameof(UnsubscribeFromUserNotifications);
}
