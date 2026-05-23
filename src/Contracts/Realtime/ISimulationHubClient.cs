namespace TradingSimulator.Contracts.Realtime;

public interface ISimulationHubClient
{
    Task OrderBookUpdated(OrderBookUpdatedMessage message);

    Task TradeTapeEntryPublished(TradeTapeEntryMessage message);

    Task LastTradePriceChanged(LastTradePriceMessage message);

    Task OrderFillNotified(OrderFillNotificationMessage message);

    Task OrderCancellationNotified(OrderCancellationNotificationMessage message);

    Task BalanceUpdated(BalanceUpdatedMessage message);
}
