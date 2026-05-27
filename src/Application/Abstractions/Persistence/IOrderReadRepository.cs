using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IOrderReadRepository
{
    Task<IReadOnlyList<OpenOrderReadModel>> GetOpenOrdersAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task<OrderHistoryPageReadModel> GetOrderHistoryAsync(
        UserId userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public sealed record OpenOrderReadModel(
    Guid OrderId,
    string Symbol,
    short Side,
    short Type,
    decimal? Price,
    long OriginalQuantity,
    long FilledQuantity,
    short Status,
    DateTimeOffset CreatedAt);

public sealed record OrderHistoryItemReadModel(
    Guid OrderId,
    string Symbol,
    short Side,
    short Type,
    decimal? Price,
    long OriginalQuantity,
    long FilledQuantity,
    short Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? TerminatedAt);

public sealed record OrderHistoryPageReadModel(
    int TotalCount,
    IReadOnlyList<OrderHistoryItemReadModel> Items);
