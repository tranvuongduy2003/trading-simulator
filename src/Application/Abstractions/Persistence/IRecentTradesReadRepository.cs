namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IRecentTradesReadRepository
{
    Task<RecentTradesReadModel> GetRecentTradesAsync(
        string symbol,
        int limit,
        CancellationToken cancellationToken = default);
}

public sealed record RecentTradesReadModel(
    string Symbol,
    IReadOnlyList<RecentTradeReadModel> Trades,
    DateTimeOffset UpdatedAt);

public sealed record RecentTradeReadModel(
    Guid TradeIdentifier,
    decimal Price,
    long Quantity,
    DateTimeOffset ExecutedAt);
