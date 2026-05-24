namespace TradingSimulator.Infrastructure.Persistence.Entities;

public sealed class PortfolioRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public long RowVersion { get; set; }

    public UserRecord? User { get; set; }

    public ICollection<HoldingRecord> Holdings { get; set; } = [];
}
