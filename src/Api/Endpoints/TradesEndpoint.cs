using MediatR;
using TradingSimulator.Api.Mapping;
using TradingSimulator.Application.Trades.Queries;
using TradingSimulator.Contracts.Trades;

namespace TradingSimulator.Api.Endpoints;

internal sealed class TradesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                "/api/trades",
                async (int? pageNumber, int? pageSize, ISender sender) =>
                {
                    var result = await sender.Send(new GetMyTradeHistoryQuery(pageNumber, pageSize));
                    return result.ToHttpResult();
                })
            .WithName("GetMyTradeHistory")
            .WithTags("Trades")
            .RequireAuthorization()
            .Produces<TradeHistoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
}
