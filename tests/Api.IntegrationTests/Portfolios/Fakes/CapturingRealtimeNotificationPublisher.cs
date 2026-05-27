using TradingSimulator.Application.Abstractions.Realtime;
using TradingSimulator.Contracts.Realtime;

namespace TradingSimulator.Api.IntegrationTests.Portfolios.Fakes;

internal sealed class CapturingRealtimeNotificationPublisher : IRealtimeNotificationPublisher
{
    public List<(Guid UserIdentifier, OrderCancellationNotificationMessage Message)> CancellationNotifications { get; } = [];

    public List<(string Symbol, OrderBookUpdatedMessage Message)> OrderBookUpdates { get; } = [];

    public List<(Guid UserIdentifier, BalanceUpdatedMessage Message)> BalanceUpdates { get; } = [];

    public Task PublishOrderBookUpdatedAsync(
        string symbol,
        OrderBookUpdatedMessage message,
        CancellationToken cancellationToken = default)
    {
        OrderBookUpdates.Add((symbol, message));
        return Task.CompletedTask;
    }

    public Task PublishTradeTapeEntryAsync(
        string symbol,
        TradeTapeEntryMessage message,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task PublishLastTradePriceAsync(
        string symbol,
        LastTradePriceMessage message,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task NotifyOrderFillAsync(
        Guid userIdentifier,
        OrderFillNotificationMessage message,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task NotifyOrderCancellationAsync(
        Guid userIdentifier,
        OrderCancellationNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        CancellationNotifications.Add((userIdentifier, message));
        return Task.CompletedTask;
    }

    public Task NotifyBalanceUpdatedAsync(
        Guid userIdentifier,
        BalanceUpdatedMessage message,
        CancellationToken cancellationToken = default)
    {
        BalanceUpdates.Add((userIdentifier, message));
        return Task.CompletedTask;
    }
}
