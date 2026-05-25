using Microsoft.Extensions.Logging;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Common;

namespace TradingSimulator.Application.Users.Commands;

public sealed class LogoutUserCommandHandler(
    ISessionStore sessionStore,
    ILogger<LogoutUserCommandHandler> logger)
    : CommandHandler<LogoutUserCommand>
{
    public override async Task<Result> Handle(
        LogoutUserCommand command,
        CancellationToken cancellationToken)
    {
        await sessionStore.RevokeSessionAsync(
            command.SessionId,
            command.UserId,
            cancellationToken);

        logger.LogInformation(
            "UserLoggedOut {UserId} {SessionId}",
            command.UserId,
            command.SessionId);

        return Result.Success();
    }
}
