using MediatR;
using TradingSimulator.Application.Abstractions.Services;
using TradingSimulator.Application.Common;

namespace TradingSimulator.Application.Behaviors;

public sealed class DomainEventDispatchBehavior<TRequest, TResponse>(
    IPendingDomainEventsCollector pendingDomainEventsCollector,
    IDomainEventDispatcher domainEventDispatcher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        if (response is not IResult { IsSuccess: true })
        {
            return response;
        }

        var domainEvents = pendingDomainEventsCollector.Drain();
        if (domainEvents.Count > 0)
        {
            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return response;
    }
}
