using TradingSimulator.Domain.Abstractions;
using TradingSimulator.Domain.Exceptions;

namespace TradingSimulator.Domain.Common;

public sealed class Money : ValueObject
{
    public const string UsdCurrency = "USD";

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money Create(decimal amount, string currency = UsdCurrency)
    {
        if (amount < 0)
        {
            throw new BusinessRuleValidationException(
                "MONEY_NEGATIVE",
                "Money amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new BusinessRuleValidationException(
                "MONEY_INVALID_CURRENCY",
                "Currency must be a three-letter ISO 4217 code.");
        }

        var normalizedAmount = decimal.Round(amount, 4, MidpointRounding.ToEven);
        return new Money(normalizedAmount, currency.ToUpperInvariant());
    }

    public static Money Usd(decimal amount) => Create(amount, UsdCurrency);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
