using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TradingSimulator.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        if (!environment.IsDevelopment())
        {
            return;
        }

        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("Trading");

        var logger = host.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseMigrationExtensions));

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning(
                "Skipping EF Core migrations: no PostgreSQL connection string is configured.");
            return;
        }

        await using var scope = host.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        try
        {
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
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to apply EF Core migrations during Development startup.");
            throw;
        }
    }
}
