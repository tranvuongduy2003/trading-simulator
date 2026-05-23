using MediatR;
using TradingSimulator.Api.Mapping;
using TradingSimulator.Application.Portfolios.Queries;
using TradingSimulator.Contracts.Portfolio;

namespace TradingSimulator.Api.Endpoints;

internal sealed class PortfolioEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                "/api/portfolio",
                async (ISender sender) =>
                {
                    var result = await sender.Send(new GetMyPortfolioQuery());
                    return result.ToHttpResult();
                })
            .WithName("GetMyPortfolio")
            .WithTags("Portfolio")
            .RequireAuthorization()
            .Produces<PortfolioResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
}
