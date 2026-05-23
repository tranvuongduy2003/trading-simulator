using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TradingSimulator.Infrastructure.Persistence;

internal sealed class ApplicationDatabaseContextDesignTimeFactory
    : IDesignTimeDbContextFactory<ApplicationDatabaseContext>
{
    public ApplicationDatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDatabaseContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=trading;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsHistoryTable(
                "__ef_migrations_history",
                ApplicationDatabaseContext.SchemaName));

        return new ApplicationDatabaseContext(optionsBuilder.Options);
    }
}
