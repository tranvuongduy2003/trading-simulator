using FluentValidation;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Users;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty()
            .Must(BeValidUsername)
            .WithMessage("Username must be 3–32 characters and contain only letters, digits, and underscores.");

        RuleFor(command => command.Email)
            .NotEmpty()
            .Must(BeValidEmail)
            .WithMessage("Email address format is invalid.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .Must(BeValidPassword)
            .WithMessage(
                "Password must be at least 8 characters and include a letter, a digit, and a special character.");
    }

    private static bool BeValidUsername(string username)
    {
        try
        {
            _ = Username.Create(username);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeValidEmail(string email)
    {
        try
        {
            _ = EmailAddress.Create(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeValidPassword(string password)
    {
        try
        {
            _ = Password.Create(password);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
