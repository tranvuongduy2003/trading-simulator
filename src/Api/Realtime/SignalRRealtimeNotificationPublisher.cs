using Microsoft.AspNetCore.SignalR;
using TradingSimulator.Api.Hubs;
using TradingSimulator.Application.Abstractions.Realtime;
using TradingSimulator.Contracts.Realtime;

namespace TradingSimulator.Api.Realtime;

internal sealed class SignalRRealtimeNotificationPublisher(
    IHubContext<SimulationHub, ISimulationHubClient> hubContext)
    : IRealtimeNotificationPublisher
{
    public Task PublishOrderBookUpdatedAsync(
        string symbol,
        OrderBookUpdatedMessage message,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(RealtimeHubGroupNames.MarketForSymbol(symbol))
            .OrderBookUpdated(message);

    public Task PublishTradeTapeEntryAsync(
        string symbol,
        TradeTapeEntryMessage message,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(RealtimeHubGroupNames.MarketForSymbol(symbol))
            .TradeTapeEntryPublished(message);

    public Task PublishLastTradePriceAsync(
        string symbol,
        LastTradePriceMessage message,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(RealtimeHubGroupNames.MarketForSymbol(symbol))
            .LastTradePriceChanged(message);

    public Task NotifyOrderFillAsync(
        Guid userIdentifier,
        OrderFillNotificationMessage message,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(RealtimeHubGroupNames.UserForIdentifier(userIdentifier))
            .OrderFillNotified(message);

    public Task NotifyOrderCancellationAsync(
        Guid userIdentifier,
        OrderCancellationNotificationMessage message,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(RealtimeHubGroupNames.UserForIdentifier(userIdentifier))
            .OrderCancellationNotified(message);

    public Task NotifyBalanceUpdatedAsync(
        Guid userIdentifier,
        BalanceUpdatedMessage message,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(RealtimeHubGroupNames.UserForIdentifier(userIdentifier))
            .BalanceUpdated(message);
}
