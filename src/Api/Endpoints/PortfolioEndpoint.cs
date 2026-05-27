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

        endpoints.MapGet(
                "/api/portfolio/reset/eligibility",
                async (ISender sender) =>
                {
                    var result = await sender.Send(new GetPortfolioResetEligibilityQuery());
                    return result.ToHttpResult();
                })
            .WithName("GetPortfolioResetEligibility")
            .WithTags("Portfolio")
            .RequireAuthorization()
            .Produces<PortfolioResetEligibilityResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

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
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
