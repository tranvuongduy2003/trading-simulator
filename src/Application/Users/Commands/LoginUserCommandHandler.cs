using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Common;
using TradingSimulator.Application.Users;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Users.Commands;

public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    ISessionStore sessionStore,
    IPasswordHasher passwordHasher,
    IPendingSessionCacheCollector pendingSessionCacheCollector)
    : CommandHandler<LoginUserCommand, LoginUserResult>
{
    public override async Task<Result<LoginUserResult>> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email);
        var password = Password.ForCredentialVerification(command.Password);

        var user = await userRepository.GetByEmailAsync(email.Value, cancellationToken);
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
        {
            return LoginErrors.InvalidCredentials;
        }

        var session = await sessionStore.CreateSessionAsync(user.Id, cancellationToken);
        pendingSessionCacheCollector.Enqueue(
            new PendingSessionCacheEntry(
                session.SessionId,
                user.Id.Value,
                session.ExpiresAt));

        return new LoginUserResult(
            user.Id.Value,
            user.Username.Value,
            email.DisplayValue,
            session.SessionId,
            session.ExpiresAt);
    }
}
