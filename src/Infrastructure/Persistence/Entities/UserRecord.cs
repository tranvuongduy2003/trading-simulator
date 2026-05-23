namespace TradingSimulator.Infrastructure.Persistence.Entities;

public sealed class UserRecord
{
    public Guid Id { get; set; }

    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public long RowVersion { get; set; }

    public WalletRecord? Wallet { get; set; }

    public PortfolioRecord? Portfolio { get; set; }

    public ICollection<UserSessionRecord> Sessions { get; set; } = [];

    public ICollection<OrderRecord> Orders { get; set; } = [];

    public ICollection<PortfolioResetRecord> PortfolioResets { get; set; } = [];
}
