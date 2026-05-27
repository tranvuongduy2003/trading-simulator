using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Common;
using TradingSimulator.Contracts.Orders;

namespace TradingSimulator.Application.Orders.Queries;

public sealed class GetMyOrderHistoryQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IOrderReadRepository orderReadRepository) : QueryHandler<GetMyOrderHistoryQuery, OrderHistoryResponse>
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 25;
    private const int MaximumPageSize = 100;

    public override async Task<Result<OrderHistoryResponse>> Handle(
        GetMyOrderHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId;
        if (userId is null)
        {
            return Error.Unauthorized("UNAUTHORIZED", "Authentication is required.");
        }

        var pageNumber = Math.Max(query.PageNumber ?? DefaultPageNumber, DefaultPageNumber);
        var pageSize = Math.Clamp(query.PageSize ?? DefaultPageSize, 1, MaximumPageSize);

        var historyPage = await orderReadRepository.GetOrderHistoryAsync(
            userId.Value,
            pageNumber,
            pageSize,
            cancellationToken);

        var responseItems = historyPage.Items
            .Select(order => new OrderHistoryItemDto(
                order.OrderId,
                order.Symbol,
                MapSide(order.Side),
                MapType(order.Type),
                order.Price,
                order.OriginalQuantity,
                order.FilledQuantity,
                order.OriginalQuantity - order.FilledQuantity,
                MapStatus(order.Status),
                order.CreatedAt,
                order.TerminatedAt))
            .ToList();

        return new OrderHistoryResponse(pageNumber, pageSize, historyPage.TotalCount, responseItems);
    }

    private static string MapSide(short side) => side switch
    {
        0 => "Buy",
        1 => "Sell",
        _ => "Unknown",
    };

    private static string MapType(short type) => type switch
    {
        0 => "Limit",
        1 => "Market",
        _ => "Unknown",
    };

    private static string MapStatus(short status) => status switch
    {
        0 => "Pending",
        1 => "PartiallyFilled",
        2 => "Filled",
        3 => "Cancelled",
        4 => "Rejected",
        _ => "Unknown",
    };
}
