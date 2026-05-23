using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Infrastructure.Persistence;

namespace TradingSimulator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("tradingdb")
            ?? configuration.GetConnectionString("Database");

        void ConfigureApplicationDatabaseContext(DbContextOptionsBuilder options)
        {
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsHistoryTable(
                        "__ef_migrations_history",
                        ApplicationDatabaseContext.SchemaName));
            }
        }

        services.AddDbContext<ApplicationDatabaseContext>(ConfigureApplicationDatabaseContext);
        services.AddDbContextFactory<ApplicationDatabaseContext>(
            ConfigureApplicationDatabaseContext,
            ServiceLifetime.Scoped);

        services.AddScoped<IApplicationDatabaseContext>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationDatabaseContext>());
        services.AddScoped<IApplicationDatabaseContextFactory, ApplicationDatabaseContextFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
