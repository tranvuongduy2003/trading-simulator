using TradingSimulator.Domain.Common;

namespace TradingSimulator.Domain.Users;

public sealed class Wallet
{
    private Wallet(UserId userId, Money totalBalance, Money reservedBalance)
    {
        UserId = userId;
        TotalBalance = totalBalance;
        ReservedBalance = reservedBalance;
    }

    public UserId UserId { get; }

    public Money TotalBalance { get; }

    public Money ReservedBalance { get; }

    public Money AvailableBalance => Money.Create(
        TotalBalance.Amount - ReservedBalance.Amount,
        TotalBalance.Currency);

    internal static Wallet CreateInitial(UserId userId, Money initialCash) =>
        new(userId, initialCash, Money.Usd(0m));

    public static Wallet FromPersistence(UserId userId, Money totalBalance, Money reservedBalance) =>
        new(userId, totalBalance, reservedBalance);
}
