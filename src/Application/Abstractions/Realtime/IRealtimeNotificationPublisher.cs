using TradingSimulator.Contracts.Realtime;

namespace TradingSimulator.Application.Abstractions.Realtime;

public interface IRealtimeNotificationPublisher
{
    Task PublishOrderBookUpdatedAsync(
        string symbol,
        OrderBookUpdatedMessage message,
        CancellationToken cancellationToken = default);

    Task PublishTradeTapeEntryAsync(
        string symbol,
        TradeTapeEntryMessage message,
        CancellationToken cancellationToken = default);

    Task PublishLastTradePriceAsync(
        string symbol,
        LastTradePriceMessage message,
        CancellationToken cancellationToken = default);

    Task NotifyOrderFillAsync(
        Guid userIdentifier,
        OrderFillNotificationMessage message,
        CancellationToken cancellationToken = default);

    Task NotifyOrderCancellationAsync(
        Guid userIdentifier,
        OrderCancellationNotificationMessage message,
        CancellationToken cancellationToken = default);

    Task NotifyBalanceUpdatedAsync(
        Guid userIdentifier,
        BalanceUpdatedMessage message,
        CancellationToken cancellationToken = default);
}
