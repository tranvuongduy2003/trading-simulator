using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Common;
using TradingSimulator.Contracts.Market;

namespace TradingSimulator.Application.Market.Queries;

public sealed class GetRecentTradesQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IRecentTradesReadRepository recentTradesReadRepository)
    : QueryHandler<GetRecentTradesQuery, RecentTradesResponse>
{
    private const string SupportedSymbol = "AAPL";
    private const int MaxLimit = 50;

    public override async Task<Result<RecentTradesResponse>> Handle(
        GetRecentTradesQuery query,
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

        if (query.Limit <= 0 || query.Limit > MaxLimit)
        {
            return Error.BadRequest("INVALID_LIMIT", "Limit must be between 1 and 50.");
        }

        var limit = query.Limit;

        var snapshot = await recentTradesReadRepository.GetRecentTradesAsync(
            normalizedSymbol,
            limit,
            cancellationToken);

        return MapToResponse(snapshot);
    }

    private static RecentTradesResponse MapToResponse(RecentTradesReadModel snapshot) =>
        new(
            snapshot.Symbol,
            snapshot.Trades.Select(MapTrade).ToList(),
            snapshot.UpdatedAt);

    private static RecentTradeItemResponse MapTrade(RecentTradeReadModel trade) =>
        new(trade.TradeIdentifier, trade.Price, trade.Quantity, trade.ExecutedAt);
}
