using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;

namespace TradingSimulator.Infrastructure.Persistence;

public sealed class ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options)
    : DbContext(options), IApplicationDatabaseContext
{
    public const string SchemaName = "trading";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(global::TradingSimulator.Infrastructure.AssemblyReference.Assembly);
    }
}
