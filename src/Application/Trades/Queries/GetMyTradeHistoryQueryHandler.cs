using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Common;
using TradingSimulator.Contracts.Trades;

namespace TradingSimulator.Application.Trades.Queries;

public sealed class GetMyTradeHistoryQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    ITradeReadRepository tradeReadRepository) : QueryHandler<GetMyTradeHistoryQuery, TradeHistoryResponse>
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 25;
    private const int MaximumPageSize = 100;

    public override async Task<Result<TradeHistoryResponse>> Handle(
        GetMyTradeHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId;
        if (userId is null)
        {
            return Error.Unauthorized("UNAUTHORIZED", "Authentication is required.");
        }

        var pageNumber = Math.Max(query.PageNumber ?? DefaultPageNumber, DefaultPageNumber);
        var pageSize = Math.Clamp(query.PageSize ?? DefaultPageSize, 1, MaximumPageSize);

        var historyPage = await tradeReadRepository.GetTradeHistoryAsync(
            userId.Value,
            pageNumber,
            pageSize,
            cancellationToken);

        var responseItems = historyPage.Items
            .Select(trade => new TradeHistoryItemDto(
                trade.TradeId,
                trade.Symbol,
                trade.IsBuyer ? "Buy" : "Sell",
                trade.Price,
                trade.Quantity,
                trade.ExecutedAt))
            .ToList();

        return new TradeHistoryResponse(pageNumber, pageSize, historyPage.TotalCount, responseItems);
    }
}
