using TradingSimulator.Domain.Abstractions;

namespace TradingSimulator.Application.Abstractions.Services;

public interface IPendingDomainEventsCollector
{
    void AddRange(IEnumerable<IDomainEvent> domainEvents);

    IReadOnlyCollection<IDomainEvent> Drain();
}
