namespace TradingSimulator.Domain.Abstractions;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
