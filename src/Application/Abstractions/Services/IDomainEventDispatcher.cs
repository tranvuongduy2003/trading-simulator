using TradingSimulator.Domain.Abstractions;

namespace TradingSimulator.Application.Abstractions.Services;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
