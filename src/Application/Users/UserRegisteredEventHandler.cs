using Microsoft.Extensions.Logging;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Domain.Events;

namespace TradingSimulator.Application.Users;

internal sealed class UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
    : IDomainEventHandler<UserRegisteredEvent>
{
    public Task Handle(UserRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "UserRegistered {UserId} {Username}",
            domainEvent.UserId.Value,
            domainEvent.Username.Value);

        return Task.CompletedTask;
    }
}
