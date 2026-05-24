using FluentAssertions;
using TradingSimulator.Domain.Common;
using TradingSimulator.Domain.Events;
using TradingSimulator.Domain.Exceptions;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Domain.UnitTests.Users;

public class UserRegisterTests
{
    private static readonly DateTimeOffset RegisteredAt = new(2026, 5, 23, 12, 0, 0, TimeSpan.Zero);

    private static readonly Money InitialCash = Money.Usd(100_000m);

    [Fact]
    public void Register_WithValidInput_CreatesWalletWithInitialCash()
    {
        var result = RegisterValidUser();

        result.User.Wallet.TotalBalance.Should().Be(InitialCash);
        result.User.Wallet.ReservedBalance.Amount.Should().Be(0m);
        result.User.Wallet.AvailableBalance.Should().Be(InitialCash);
        result.Portfolio.Holdings.Should().BeEmpty();
        result.User.Id.Value.Should().NotBe(Guid.Empty);
        result.Portfolio.UserId.Should().Be(result.User.Id);
    }

    [Fact]
    public void Register_WithInvalidUsername_Throws()
    {
        var act = () => Username.Create("ab");

        act.Should()
            .Throw<BusinessRuleValidationException>()
            .Which.Code.Should().Be("USERNAME_LENGTH");
    }

    [Fact]
    public void Register_RaisesUserRegisteredEvent()
    {
        var result = RegisterValidUser();

        result.User.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredEvent>()
            .Which.Should().BeEquivalentTo(
                new UserRegisteredEvent(
                    result.User.Id,
                    result.Portfolio.Id,
                    result.User.Username,
                    result.User.Email),
                options => options.Excluding(domainEvent => domainEvent.OccurredOn));
    }

    private static UserRegistrationResult RegisterValidUser() =>
        User.Register(
            Username.Create("trader_jane"),
            EmailAddress.Create("Jane@Example.com"),
            Password.Create("SecurePass1!"),
            PasswordHash.Create("hashed-password-stub"),
            InitialCash,
            RegisteredAt);
}
