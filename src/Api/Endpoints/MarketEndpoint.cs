using MediatR;
using TradingSimulator.Api.Mapping;
using TradingSimulator.Application.Market.Queries;
using TradingSimulator.Contracts.Market;

namespace TradingSimulator.Api.Endpoints;

internal sealed class MarketEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                "/api/market/orderbook",
                async (string symbol, int? depth, ISender sender) =>
                {
                    var result = await sender.Send(
                        new GetOrderBookSnapshotQuery(symbol, depth ?? 10));
                    return result.ToHttpResult();
                })
            .WithName("GetOrderBookSnapshot")
            .WithTags("Market")
            .RequireAuthorization()
            .Produces<OrderBookSnapshotResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
}
