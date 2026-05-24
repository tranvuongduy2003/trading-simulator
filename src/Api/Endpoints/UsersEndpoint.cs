using MediatR;
using Microsoft.Extensions.Options;
using TradingSimulator.Api.Auth;
using TradingSimulator.Api.Mapping;
using TradingSimulator.Application.Options;
using TradingSimulator.Application.Users;
using TradingSimulator.Contracts.Users;

namespace TradingSimulator.Api.Endpoints;

internal sealed class UsersEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/api/users", RegisterUser)
            .WithName("RegisterUser")
            .WithTags("Users")
            .AllowAnonymous()
            .Accepts<RegisterUserRequest>("application/json")
            .Produces<UserRegistrationResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status422UnprocessableEntity);

    private static async Task<IResult> RegisterUser(
        RegisterUserRequest request,
        ISender sender,
        HttpContext httpContext,
        IOptions<TradingSessionOptions> sessionOptions)
    {
        var result = await sender.Send(
            new RegisterUserCommand(request.Username, request.Email, request.Password));

        if (!result.IsSuccess)
        {
            return result.ToHttpResult();
        }

        var registeredUser = result.Value!;
        SessionCookieWriter.Append(
            httpContext,
            registeredUser.SessionId,
            registeredUser.SessionExpiresAt,
            sessionOptions.Value);

        return Results.Created(
            $"/api/users/{registeredUser.UserId:D}",
            new UserRegistrationResponse(
                registeredUser.UserId,
                registeredUser.Username,
                registeredUser.Email,
                registeredUser.CreatedAt,
                new WalletSummaryDto(
                    registeredUser.Wallet.Currency,
                    registeredUser.Wallet.TotalBalance,
                    registeredUser.Wallet.ReservedBalance,
                    registeredUser.Wallet.AvailableBalance)));
    }
}
