using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingSimulator.Application.Abstractions.Cache;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Infrastructure.Cache;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class RecentTradesReadRepository(
    ApplicationDatabaseContext databaseContext,
    ICacheService cacheService,
    ILogger<RecentTradesReadRepository> logger) : IRecentTradesReadRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<RecentTradesReadModel> GetRecentTradesAsync(
        string symbol,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = RecentTradesCacheKeys.ForSymbol(symbol);
        var cachedJson = await cacheService.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(cachedJson))
        {
            var cachedSnapshot = TryDeserializeSnapshot(cachedJson, symbol);
            if (cachedSnapshot is not null)
            {
                return NormalizeSnapshot(cachedSnapshot, limit);
            }
        }

        return await BuildFromPostgreSqlAsync(symbol, limit, cancellationToken);
    }

    private RecentTradesReadModel? TryDeserializeSnapshot(string json, string expectedSymbol)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<CachedRecentTradesPayload>(json, JsonOptions);
            if (payload is null || !string.Equals(payload.Symbol, expectedSymbol, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var trades = payload.Trades?
                .Select(trade => new RecentTradeReadModel(
                    trade.TradeIdentifier,
                    trade.Price,
                    trade.Quantity,
                    trade.ExecutedAt))
                .ToList()
                ?? [];

            return new RecentTradesReadModel(
                payload.Symbol.ToUpperInvariant(),
                trades,
                payload.UpdatedAt);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Failed to deserialize recent trades from cache");
            return null;
        }
    }

    private static RecentTradesReadModel NormalizeSnapshot(RecentTradesReadModel snapshot, int limit) =>
        new(
            snapshot.Symbol,
            snapshot.Trades.Take(limit).ToList(),
            snapshot.UpdatedAt);

    private async Task<RecentTradesReadModel> BuildFromPostgreSqlAsync(
        string symbol,
        int limit,
        CancellationToken cancellationToken)
    {
        var tradeRows = await databaseContext.Trades
            .AsNoTracking()
            .Where(tradeRecord => tradeRecord.Symbol == symbol)
            .OrderByDescending(tradeRecord => tradeRecord.ExecutedAt)
            .Take(limit)
            .Select(tradeRecord => new RecentTradeReadModel(
                tradeRecord.ExternalId,
                tradeRecord.Price,
                tradeRecord.Quantity,
                tradeRecord.ExecutedAt))
            .ToListAsync(cancellationToken);

        var updatedAt = tradeRows.Count > 0
            ? tradeRows[0].ExecutedAt
            : DateTimeOffset.UtcNow;

        return new RecentTradesReadModel(symbol, tradeRows, updatedAt);
    }

    private sealed record CachedRecentTradesPayload(
        string Symbol,
        IReadOnlyList<CachedRecentTradePayload>? Trades,
        DateTimeOffset UpdatedAt);

    private sealed record CachedRecentTradePayload(
        Guid TradeIdentifier,
        decimal Price,
        long Quantity,
        DateTimeOffset ExecutedAt);
}
