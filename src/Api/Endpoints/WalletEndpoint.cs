using MediatR;
using TradingSimulator.Api.Mapping;
using TradingSimulator.Application.Users.Queries;
using TradingSimulator.Contracts.Users;

namespace TradingSimulator.Api.Endpoints;

internal sealed class WalletEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                "/api/wallet",
                async (ISender sender) =>
                {
                    var result = await sender.Send(new GetMyWalletQuery());
                    return result.ToHttpResult();
                })
            .WithName("GetMyWallet")
            .WithTags("Wallet")
            .RequireAuthorization()
            .Produces<WalletResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
}
