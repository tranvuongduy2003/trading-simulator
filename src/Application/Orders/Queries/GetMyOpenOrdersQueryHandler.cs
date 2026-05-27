using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Common;
using TradingSimulator.Contracts.Orders;

namespace TradingSimulator.Application.Orders.Queries;

public sealed class GetMyOpenOrdersQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IOrderReadRepository orderReadRepository) : QueryHandler<GetMyOpenOrdersQuery, IReadOnlyList<OpenOrderDto>>
{
    public override async Task<Result<IReadOnlyList<OpenOrderDto>>> Handle(
        GetMyOpenOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId;
        if (userId is null)
        {
            return Error.Unauthorized("UNAUTHORIZED", "Authentication is required.");
        }

        var openOrders = await orderReadRepository.GetOpenOrdersAsync(userId.Value, cancellationToken);

        return openOrders
            .Select(order => new OpenOrderDto(
                order.OrderId,
                order.Symbol,
                MapSide(order.Side),
                MapType(order.Type),
                order.Price,
                order.OriginalQuantity,
                order.FilledQuantity,
                order.OriginalQuantity - order.FilledQuantity,
                MapStatus(order.Status),
                order.CreatedAt))
            .ToList();
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
