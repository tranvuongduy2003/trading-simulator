using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Persistence;

public interface ITradeReadRepository
{
    Task<TradeHistoryPageReadModel> GetTradeHistoryAsync(
        UserId userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public sealed record TradeHistoryItemReadModel(
    Guid TradeId,
    string Symbol,
    bool IsBuyer,
    decimal Price,
    long Quantity,
    DateTimeOffset ExecutedAt);

public sealed record TradeHistoryPageReadModel(
    int TotalCount,
    IReadOnlyList<TradeHistoryItemReadModel> Items);
