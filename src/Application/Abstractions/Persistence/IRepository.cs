using TradingSimulator.Domain.Abstractions;

namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IRepository<TAggregate, TId>
    where TAggregate : class, IAggregateRoot<TId>
    where TId : notnull;
