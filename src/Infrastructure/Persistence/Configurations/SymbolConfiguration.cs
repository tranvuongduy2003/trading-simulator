using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Configurations;

internal sealed class SymbolConfiguration : IEntityTypeConfiguration<SymbolRecord>
{
    private static readonly DateTimeOffset SeedCreatedAt =
        new(2026, 5, 23, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<SymbolRecord> builder)
    {
        builder.ToTable(
            "symbols",
            tableBuilder => tableBuilder.HasCheckConstraint("ck_symbols_tick_size_positive", "tick_size > 0"));

        builder.HasKey(symbol => symbol.Code);

        builder.Property(symbol => symbol.Code).HasColumnName("code").HasMaxLength(8);
        builder.Property(symbol => symbol.Name).HasColumnName("name").HasMaxLength(128);
        builder.Property(symbol => symbol.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(symbol => symbol.TickSize).HasColumnName("tick_size").AsMoney().HasDefaultValue(0.01m);
        builder.Property(symbol => symbol.CreatedAt).HasColumnName("created_at");

        builder.HasData(
            new SymbolRecord
            {
                Code = "AAPL",
                Name = "Apple Inc.",
                IsActive = true,
                TickSize = 0.01m,
                CreatedAt = SeedCreatedAt,
            });
    }
}
