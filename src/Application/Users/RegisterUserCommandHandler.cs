using Microsoft.Extensions.Options;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Abstractions.Services;
using TradingSimulator.Application.Common;
using TradingSimulator.Application.Options;
using TradingSimulator.Domain.Common;
using TradingSimulator.Domain.Exceptions;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Users;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    ISessionStore sessionStore,
    IPasswordHasher passwordHasher,
    IOptions<TradingOptions> tradingOptions,
    IClock clock,
    IPendingDomainEventsCollector pendingDomainEventsCollector,
    IPendingSessionCacheCollector pendingSessionCacheCollector)
    : CommandHandler<RegisterUserCommand, RegisterUserResult>
{
    public override async Task<Result<RegisterUserResult>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailAddress.Create(command.Email).Value;

        if (await userRepository.ExistsByUsernameAsync(command.Username, cancellationToken))
        {
            return Error.Validation(
                "CONFLICT",
                "A user with this username already exists.");
        }

        if (await userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            return Error.Validation(
                "CONFLICT",
                "A user with this email already exists.");
        }

        try
        {
            var username = Username.Create(command.Username);
            var email = EmailAddress.Create(command.Email);
            var password = Password.Create(command.Password);
            var passwordHash = passwordHasher.Hash(password);
            var initialCash = Money.Usd(tradingOptions.Value.InitialVirtualCash);
            var createdAt = clock.UtcNow;

            var registration = User.Register(
                username,
                email,
                password,
                passwordHash,
                initialCash,
                createdAt);

            await userRepository.AddAsync(registration.User, registration.Portfolio, cancellationToken);

            var session = await sessionStore.CreateSessionAsync(registration.User.Id, cancellationToken);
            pendingSessionCacheCollector.Enqueue(
                new PendingSessionCacheEntry(
                    session.SessionId,
                    registration.User.Id.Value,
                    session.ExpiresAt));

            pendingDomainEventsCollector.AddRange(registration.User.DomainEvents);
            registration.User.ClearDomainEvents();

            return MapToResult(registration, email, session);
        }
        catch (BusinessRuleValidationException exception)
        {
            return Error.Validation(
                exception.Code ?? Error.ValidationFailedCode,
                exception.Message);
        }
    }

    private static RegisterUserResult MapToResult(
        UserRegistrationResult registration,
        EmailAddress email,
        SessionCreationResult session) =>
        new(
            registration.User.Id.Value,
            registration.User.Username.Value,
            email.DisplayValue,
            registration.User.CreatedAt,
            new RegisterUserWalletResult(
                registration.User.Wallet.TotalBalance.Currency,
                registration.User.Wallet.TotalBalance.Amount,
                registration.User.Wallet.ReservedBalance.Amount,
                registration.User.Wallet.AvailableBalance.Amount),
            session.SessionId,
            session.ExpiresAt);
}
