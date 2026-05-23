using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TradingSimulator.Infrastructure.Persistence;

internal sealed class DevelopmentDatabaseMigrationHostedService(
    IServiceProvider serviceProvider,
    IHostEnvironment hostEnvironment,
    ILogger<DevelopmentDatabaseMigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!hostEnvironment.IsDevelopment())
        {
            return;
        }

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("trading")
            ?? configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning(
                "Skipping EF Core migrations: no PostgreSQL connection string is configured.");
            return;
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        var migrator = scope.ServiceProvider.GetRequiredService<IDevelopmentDatabaseMigrator>();

        try
        {
            await migrator.ApplyPendingMigrationsAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to apply EF Core migrations during Development startup.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
