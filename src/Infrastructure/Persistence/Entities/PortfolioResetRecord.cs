namespace TradingSimulator.Infrastructure.Persistence.Entities;

public sealed class PortfolioResetRecord
{
    public long Id { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset ResetAt { get; set; }

    public string? Reason { get; set; }

    public UserRecord? User { get; set; }
}
