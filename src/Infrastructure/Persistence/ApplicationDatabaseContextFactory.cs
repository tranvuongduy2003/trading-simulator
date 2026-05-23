using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;

namespace TradingSimulator.Infrastructure.Persistence;

internal sealed class ApplicationDatabaseContextFactory(
    IDbContextFactory<ApplicationDatabaseContext> databaseContextFactory)
    : IApplicationDatabaseContextFactory
{
    public async Task<IApplicationDatabaseContext> CreateApplicationDatabaseContextAsync(
        CancellationToken cancellationToken = default) =>
        await databaseContextFactory.CreateDbContextAsync(cancellationToken);
}
