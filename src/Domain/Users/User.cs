using TradingSimulator.Domain.Abstractions;
using TradingSimulator.Domain.Common;
using TradingSimulator.Domain.Events;
using TradingSimulator.Domain.Exceptions;
using TradingSimulator.Domain.Portfolios;

namespace TradingSimulator.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    private User()
    {
    }

    public Username Username { get; private set; } = null!;

    public EmailAddress Email { get; private set; } = null!;

    public PasswordHash PasswordHash { get; private set; } = null!;

    public Wallet Wallet { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static UserRegistrationResult Register(
        Username username,
        EmailAddress email,
        Password password,
        PasswordHash passwordHash,
        Money initialCash,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (initialCash.Currency != Money.UsdCurrency)
        {
            throw new BusinessRuleValidationException(
                "INITIAL_CASH_CURRENCY",
                "Initial virtual cash must be USD for MVP.");
        }

        var userId = UserId.New();
        var user = new User
        {
            Id = userId,
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Wallet = Wallet.CreateInitial(userId, initialCash),
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };

        var portfolio = Portfolio.CreateForUser(userId, createdAt);

        user.Raise(new UserRegisteredEvent(userId, portfolio.Id, username, email));

        return new UserRegistrationResult(user, portfolio);
    }

    public static User FromPersistence(
        UserId id,
        Username username,
        EmailAddress email,
        PasswordHash passwordHash,
        Wallet wallet,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new()
        {
            Id = id,
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Wallet = wallet,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
        };
}
