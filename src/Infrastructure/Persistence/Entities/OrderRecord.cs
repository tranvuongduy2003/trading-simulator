namespace TradingSimulator.Infrastructure.Persistence.Entities;

public sealed class OrderRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Symbol { get; set; }

    public short Side { get; set; }

    public short Type { get; set; }

    public decimal? Price { get; set; }

    public long OriginalQuantity { get; set; }

    public long FilledQuantity { get; set; }

    public short Status { get; set; }

    public string? RejectionReason { get; set; }

    public bool IsSimulated { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? TerminatedAt { get; set; }

    public long RowVersion { get; set; }

    public UserRecord? User { get; set; }

    public SymbolRecord? SymbolNavigation { get; set; }
}
