using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence;

public sealed class ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options)
    : DbContext(options), IApplicationDatabaseContext
{
    public const string SchemaName = "trading";

    public DbSet<UserRecord> Users => Set<UserRecord>();

    public DbSet<WalletRecord> Wallets => Set<WalletRecord>();

    public DbSet<PortfolioRecord> Portfolios => Set<PortfolioRecord>();

    public DbSet<HoldingRecord> Holdings => Set<HoldingRecord>();

    public DbSet<SymbolRecord> Symbols => Set<SymbolRecord>();

    public DbSet<OrderRecord> Orders => Set<OrderRecord>();

    public DbSet<TradeRecord> Trades => Set<TradeRecord>();

    public DbSet<CandlestickRecord> Candlesticks => Set<CandlestickRecord>();

    public DbSet<UserSessionRecord> UserSessions => Set<UserSessionRecord>();

    public DbSet<PortfolioResetRecord> PortfolioResets => Set<PortfolioResetRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(global::TradingSimulator.Infrastructure.AssemblyReference.Assembly);
    }
}
