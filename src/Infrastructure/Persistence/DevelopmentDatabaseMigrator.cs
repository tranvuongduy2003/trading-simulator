using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TradingSimulator.Infrastructure.Persistence;

public interface IDevelopmentDatabaseMigrator
{
    Task ApplyPendingMigrationsAsync(CancellationToken cancellationToken = default);
}

internal sealed class DevelopmentDatabaseMigrator(
    ApplicationDatabaseContext databaseContext,
    IHostEnvironment hostEnvironment,
    ILogger<DevelopmentDatabaseMigrator> logger) : IDevelopmentDatabaseMigrator
{
    public async Task ApplyPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        if (!hostEnvironment.IsDevelopment())
        {
            return;
        }

        var pendingMigrations = await databaseContext.Database
            .GetPendingMigrationsAsync(cancellationToken);

        if (!pendingMigrations.Any())
        {
            return;
        }

        logger.LogInformation(
            "Applying {MigrationCount} pending EF Core migration(s) to PostgreSQL.",
            pendingMigrations.Count());

        await databaseContext.Database.MigrateAsync(cancellationToken);
    }
}
