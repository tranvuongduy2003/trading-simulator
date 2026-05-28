using TradingSimulator.Application.Abstractions.Market;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Abstractions.Realtime;
using TradingSimulator.Contracts.Realtime;

namespace TradingSimulator.Application.Market;

// Call after Redis snapshot write and PostgreSQL commit from the matching-engine consumer.
public sealed class OrderBookMarketDataNotifier(
    IOrderBookSnapshotReadRepository orderBookSnapshotReadRepository,
    IRealtimeNotificationPublisher realtimeNotificationPublisher)
    : IOrderBookMarketDataNotifier
{
    private const string SupportedSymbol = "AAPL";
    private const int PublishDepth = 10;

    public async Task NotifyOrderBookChangedAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        if (!string.Equals(normalizedSymbol, SupportedSymbol, StringComparison.Ordinal))
        {
            return;
        }

        var snapshot = await orderBookSnapshotReadRepository.GetSnapshotAsync(
            normalizedSymbol,
            PublishDepth,
            cancellationToken);

        var message = new OrderBookUpdatedMessage(
            snapshot.Symbol,
            snapshot.Bids.Select(MapLevel).ToList(),
            snapshot.Asks.Select(MapLevel).ToList(),
            snapshot.UpdatedAt);

        await realtimeNotificationPublisher.PublishOrderBookUpdatedAsync(
            normalizedSymbol,
            message,
            cancellationToken);
    }

    private static OrderBookLevelMessage MapLevel(OrderBookLevelReadModel level) =>
        new(level.Price, level.Quantity);
}
