using TradingSimulator.Domain.Portfolios;
using TradingSimulator.Domain.Users;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Mapping;

internal static class UserPersistenceMapper
{
    public static UserRecord ToUserRecord(User user) =>
        new()
        {
            Id = user.Id.Value,
            Username = user.Username.Value,
            Email = user.Email.Value,
            PasswordHash = user.PasswordHash.Value,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            RowVersion = 1,
        };

    public static WalletRecord ToWalletRecord(Wallet wallet, DateTimeOffset updatedAt) =>
        new()
        {
            UserId = wallet.UserId.Value,
            TotalBalance = wallet.TotalBalance.Amount,
            ReservedBalance = wallet.ReservedBalance.Amount,
            Currency = wallet.TotalBalance.Currency,
            UpdatedAt = updatedAt,
            RowVersion = 1,
        };

    public static PortfolioRecord ToPortfolioRecord(Portfolio portfolio) =>
        new()
        {
            Id = portfolio.Id.Value,
            UserId = portfolio.UserId.Value,
            CreatedAt = portfolio.CreatedAt,
            UpdatedAt = portfolio.UpdatedAt,
            RowVersion = 1,
        };
}
