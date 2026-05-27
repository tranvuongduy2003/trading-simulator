using MediatR;
using TradingSimulator.Api.Mapping;
using TradingSimulator.Application.Orders.Queries;
using TradingSimulator.Contracts.Orders;

namespace TradingSimulator.Api.Endpoints;

internal sealed class OrdersEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/orders/open",
                async (ISender sender) =>
                {
                    var result = await sender.Send(new GetMyOpenOrdersQuery());
                    return result.ToHttpResult();
                })
            .WithName("GetMyOpenOrders")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<IReadOnlyList<OpenOrderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        endpoints.MapGet(
                "/api/orders/history",
                async (int? pageNumber, int? pageSize, ISender sender) =>
                {
                    var result = await sender.Send(new GetMyOrderHistoryQuery(pageNumber, pageSize));
                    return result.ToHttpResult();
                })
            .WithName("GetMyOrderHistory")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<OrderHistoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
