using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingSimulator.Application.Abstractions.Cache;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Infrastructure.Cache;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class OrderBookSnapshotReadRepository(
    ApplicationDatabaseContext databaseContext,
    ICacheService cacheService,
    ILogger<OrderBookSnapshotReadRepository> logger) : IOrderBookSnapshotReadRepository
{
    private const short PendingStatus = 0;
    private const short PartiallyFilledStatus = 1;
    private const short BuySide = 0;
    private const short SellSide = 1;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<OrderBookSnapshotReadModel> GetSnapshotAsync(
        string symbol,
        int depth,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = OrderBookSnapshotCacheKeys.ForSymbol(symbol);
        var cachedJson = await cacheService.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(cachedJson))
        {
            var cachedSnapshot = TryDeserializeSnapshot(cachedJson, symbol);
            if (cachedSnapshot is not null)
            {
                return NormalizeSnapshot(cachedSnapshot, depth);
            }
        }

        return await BuildFromPostgreSqlAsync(symbol, depth, cancellationToken);
    }

    private OrderBookSnapshotReadModel? TryDeserializeSnapshot(string json, string expectedSymbol)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<CachedOrderBookSnapshotPayload>(json, JsonOptions);
            if (payload is null || !string.Equals(payload.Symbol, expectedSymbol, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new OrderBookSnapshotReadModel(
                payload.Symbol.ToUpperInvariant(),
                MapLevels(payload.Bids),
                MapLevels(payload.Asks),
                payload.UpdatedAt);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Failed to deserialize order book snapshot from cache");
            return null;
        }
    }

    private static IReadOnlyList<OrderBookLevelReadModel> MapLevels(
        IReadOnlyList<CachedOrderBookLevelPayload>? levels) =>
        levels?
            .Select(level => new OrderBookLevelReadModel(level.Price, level.Quantity, level.OrderCount))
            .ToList()
        ?? [];

    private static OrderBookSnapshotReadModel NormalizeSnapshot(
        OrderBookSnapshotReadModel snapshot,
        int depth) =>
        new(
            snapshot.Symbol,
            SortBids(snapshot.Bids).Take(depth).ToList(),
            SortAsks(snapshot.Asks).Take(depth).ToList(),
            snapshot.UpdatedAt);

    private async Task<OrderBookSnapshotReadModel> BuildFromPostgreSqlAsync(
        string symbol,
        int depth,
        CancellationToken cancellationToken)
    {
        var activeBookQuery = databaseContext.Orders
            .AsNoTracking()
            .Where(orderRecord =>
                orderRecord.Symbol == symbol
                && orderRecord.Price != null
                && (orderRecord.Status == PendingStatus || orderRecord.Status == PartiallyFilledStatus));

        var bidLevels = await BuildLevelsAsync(
            activeBookQuery,
            BuySide,
            descending: true,
            depth,
            cancellationToken);

        var askLevels = await BuildLevelsAsync(
            activeBookQuery,
            SellSide,
            descending: false,
            depth,
            cancellationToken);

        var updatedAt = await activeBookQuery
            .Select(orderRecord => (DateTimeOffset?)orderRecord.UpdatedAt)
            .MaxAsync(cancellationToken)
            ?? DateTimeOffset.UtcNow;

        return new OrderBookSnapshotReadModel(symbol, bidLevels, askLevels, updatedAt);
    }

    private static async Task<IReadOnlyList<OrderBookLevelReadModel>> BuildLevelsAsync(
        IQueryable<Entities.OrderRecord> activeBookQuery,
        short side,
        bool descending,
        int depth,
        CancellationToken cancellationToken)
    {
        var rows = await activeBookQuery
            .Where(orderRecord => orderRecord.Side == side && orderRecord.Price != null)
            .Select(orderRecord => new
            {
                Price = orderRecord.Price!.Value,
                RemainingQuantity = orderRecord.OriginalQuantity - orderRecord.FilledQuantity,
            })
            .ToListAsync(cancellationToken);

        var levels = rows
            .GroupBy(row => row.Price)
            .Select(priceGroup => new OrderBookLevelReadModel(
                priceGroup.Key,
                priceGroup.Sum(row => row.RemainingQuantity),
                priceGroup.Count()))
            .ToList();

        IEnumerable<OrderBookLevelReadModel> orderedLevels = descending
            ? levels.OrderByDescending(level => level.Price)
            : levels.OrderBy(level => level.Price);

        return orderedLevels.Take(depth).ToList();
    }

    private static IEnumerable<OrderBookLevelReadModel> SortBids(IReadOnlyList<OrderBookLevelReadModel> bids) =>
        bids.OrderByDescending(level => level.Price);

    private static IEnumerable<OrderBookLevelReadModel> SortAsks(IReadOnlyList<OrderBookLevelReadModel> asks) =>
        asks.OrderBy(level => level.Price);

    private sealed record CachedOrderBookSnapshotPayload(
        string Symbol,
        IReadOnlyList<CachedOrderBookLevelPayload>? Bids,
        IReadOnlyList<CachedOrderBookLevelPayload>? Asks,
        DateTimeOffset UpdatedAt);

    private sealed record CachedOrderBookLevelPayload(
        decimal Price,
        long Quantity,
        int OrderCount);
}
