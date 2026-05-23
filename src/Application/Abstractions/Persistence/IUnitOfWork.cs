namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    bool IsConcurrencyConflict(Exception exception);

    bool IsUniqueConstraintViolation(Exception exception);
}
