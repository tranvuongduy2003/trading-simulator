using MediatR;
using TradingSimulator.Api.Mapping;
using TradingSimulator.Application.Portfolios.Commands;
using TradingSimulator.Application.Portfolios.Queries;
using TradingSimulator.Contracts.Portfolio;

namespace TradingSimulator.Api.Endpoints;

internal sealed class PortfolioEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoints)
    {
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

        endpoints.MapPost(
                "/api/portfolio/reset",
                async (ISender sender) =>
                {
                    var result = await sender.Send(new ResetPortfolioCommand());
                    return result.ToHttpResult();
                })
            .WithName("ResetPortfolio")
            .WithTags("Portfolio")
            .RequireAuthorization()
            .Produces<PortfolioResetResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
