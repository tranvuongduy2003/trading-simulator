using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Abstractions.Realtime;
using TradingSimulator.Contracts.Realtime;

namespace TradingSimulator.Application.Market.Realtime;

public sealed class OrderBookRealtimeProjection(
    IOrderBookSnapshotReadRepository orderBookSnapshotReadRepository,
    IRealtimeNotificationPublisher realtimeNotificationPublisher) : IOrderBookRealtimeProjection
{
    private const string SupportedSymbol = "AAPL";
    private const int DefaultDepth = 10;

    public async Task PublishForSymbolAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        if (!string.Equals(normalizedSymbol, SupportedSymbol, StringComparison.Ordinal))
        {
            throw new ArgumentException("Only AAPL is supported in this release.", nameof(symbol));
        }

        var snapshot = await orderBookSnapshotReadRepository.GetSnapshotAsync(
            normalizedSymbol,
            DefaultDepth,
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
