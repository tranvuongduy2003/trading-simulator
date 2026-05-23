using System.Text.RegularExpressions;
using TradingSimulator.Domain.Abstractions;
using TradingSimulator.Domain.Exceptions;

namespace TradingSimulator.Domain.Users;

public sealed partial class Username : ValueObject
{
    private const int MinimumLength = 3;
    private const int MaximumLength = 32;

    private Username(string value) => Value = value;

    public string Value { get; }

    public static Username Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleValidationException(
                "USERNAME_REQUIRED",
                "Username is required.");
        }

        if (value.Length is < MinimumLength or > MaximumLength)
        {
            throw new BusinessRuleValidationException(
                "USERNAME_LENGTH",
                $"Username must be between {MinimumLength} and {MaximumLength} characters.");
        }

        if (!UsernamePattern().IsMatch(value))
        {
            throw new BusinessRuleValidationException(
                "USERNAME_INVALID_CHARACTERS",
                "Username may only contain letters, digits, and underscores.");
        }

        return new Username(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex("^[a-zA-Z0-9_]+$")]
    private static partial Regex UsernamePattern();
}
