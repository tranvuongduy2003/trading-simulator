using MediatR;
using Microsoft.Extensions.Options;
using TradingSimulator.Api.Auth;
using TradingSimulator.Api.Http;
using TradingSimulator.Api.Mapping;
using TradingSimulator.Application.Options;
using TradingSimulator.Application.Users.Commands;
using TradingSimulator.Contracts.Users;

namespace TradingSimulator.Api.Endpoints;

internal sealed class AuthEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/api/auth/login", LoginUser)
            .WithName("LoginUser")
            .WithTags("Auth")
            .AllowAnonymous()
            .RequireCompleteJsonBody<LoginUserRequest>()
            .Accepts<LoginUserRequest>("application/json")
            .Produces<LoginUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

    private static async Task<IResult> LoginUser(
        LoginUserRequest request,
        ISender sender,
        HttpContext httpContext,
        IOptions<TradingSessionOptions> sessionOptions)
    {
        var result = await sender.Send(new LoginUserCommand(request.Email, request.Password));

        if (!result.IsSuccess)
        {
            return result.ToHttpResult();
        }

        var loginUser = result.Value!;
        SessionCookieWriter.Append(
            httpContext,
            loginUser.SessionId,
            loginUser.SessionExpiresAt,
            sessionOptions.Value);

        return Results.Ok(
            new LoginUserResponse(loginUser.UserId, loginUser.Username, loginUser.Email));
    }
}
