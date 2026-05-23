using System.Text.RegularExpressions;
using TradingSimulator.Domain.Abstractions;
using TradingSimulator.Domain.Exceptions;

namespace TradingSimulator.Domain.Users;

public sealed partial class Password : ValueObject
{
    private const int MinimumLength = 8;

    private Password(string value) => Value = value;

    public string Value { get; }

    public static Password Create(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            throw new BusinessRuleValidationException(
                "PASSWORD_REQUIRED",
                "Password is required.");
        }

        if (plaintext.Length < MinimumLength)
        {
            throw new BusinessRuleValidationException(
                "PASSWORD_TOO_SHORT",
                $"Password must be at least {MinimumLength} characters.");
        }

        if (!PasswordHasLetter().IsMatch(plaintext))
        {
            throw new BusinessRuleValidationException(
                "PASSWORD_MISSING_LETTER",
                "Password must include at least one letter.");
        }

        if (!PasswordHasDigit().IsMatch(plaintext))
        {
            throw new BusinessRuleValidationException(
                "PASSWORD_MISSING_DIGIT",
                "Password must include at least one digit.");
        }

        if (!PasswordHasSpecialCharacter().IsMatch(plaintext))
        {
            throw new BusinessRuleValidationException(
                "PASSWORD_MISSING_SPECIAL",
                "Password must include at least one special character.");
        }

        return new Password(plaintext);
    }

    protected override IEnumerable<object?> GetEqualityComponents() => [];

    [GeneratedRegex("[A-Za-z]")]
    private static partial Regex PasswordHasLetter();

    [GeneratedRegex("[0-9]")]
    private static partial Regex PasswordHasDigit();

    [GeneratedRegex(@"[!@#$%^&*()_+\-=\[\]{}|;:'"",.<>?/\\`~]")]
    private static partial Regex PasswordHasSpecialCharacter();
}
