using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Common;
using TradingSimulator.Contracts.Market;

namespace TradingSimulator.Application.Market.Queries;

public sealed class GetOrderBookSnapshotQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IOrderBookSnapshotReadRepository orderBookSnapshotReadRepository)
    : QueryHandler<GetOrderBookSnapshotQuery, OrderBookSnapshotResponse>
{
    private const string SupportedSymbol = "AAPL";

    public override async Task<Result<OrderBookSnapshotResponse>> Handle(
        GetOrderBookSnapshotQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUserAccessor.UserId is null)
        {
            return Error.Unauthorized("UNAUTHORIZED", "Authentication is required.");
        }

        var normalizedSymbol = query.Symbol.Trim().ToUpperInvariant();
        if (!string.Equals(normalizedSymbol, SupportedSymbol, StringComparison.Ordinal))
        {
            return Error.BadRequest("INVALID_SYMBOL", "Only AAPL is supported in this release.");
        }

        var depth = query.Depth <= 0 ? 10 : query.Depth;
        var snapshot = await orderBookSnapshotReadRepository.GetSnapshotAsync(
            normalizedSymbol,
            depth,
            cancellationToken);

        return MapToResponse(snapshot);
    }

    private static OrderBookSnapshotResponse MapToResponse(OrderBookSnapshotReadModel snapshot) =>
        new(
            snapshot.Symbol,
            snapshot.Bids.Select(MapLevel).ToList(),
            snapshot.Asks.Select(MapLevel).ToList(),
            snapshot.UpdatedAt);

    private static OrderBookLevelResponse MapLevel(OrderBookLevelReadModel level) =>
        new(level.Price, level.Quantity, level.OrderCount);
}
