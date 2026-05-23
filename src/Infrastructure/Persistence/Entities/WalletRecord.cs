namespace TradingSimulator.Infrastructure.Persistence.Entities;

public sealed class WalletRecord
{
    public Guid UserId { get; set; }

    public decimal TotalBalance { get; set; }

    public decimal ReservedBalance { get; set; }

    public required string Currency { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public long RowVersion { get; set; }

    public UserRecord? User { get; set; }
}
