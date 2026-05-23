using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;

namespace TradingSimulator.Infrastructure.Persistence;

internal sealed class UnitOfWork(ApplicationDatabaseContext databaseContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        databaseContext.SaveChangesAsync(cancellationToken);

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await databaseContext.Database.BeginTransactionAsync(cancellationToken);
        return new UnitOfWorkTransaction(transaction);
    }

    public bool IsConcurrencyConflict(Exception exception) =>
        exception is DbUpdateConcurrencyException;
}
