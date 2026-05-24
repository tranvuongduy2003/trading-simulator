using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Configurations;

internal sealed class HoldingConfiguration : IEntityTypeConfiguration<HoldingRecord>
{
    public void Configure(EntityTypeBuilder<HoldingRecord> builder)
    {
        builder.ToTable(
            "holdings",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("ck_holdings_total_non_negative", "total_quantity >= 0");
                tableBuilder.HasCheckConstraint("ck_holdings_reserved_non_negative", "reserved_quantity >= 0");
                tableBuilder.HasCheckConstraint("ck_holdings_reserved_le_total", "reserved_quantity <= total_quantity");
                tableBuilder.HasCheckConstraint("ck_holdings_avg_price_non_negative", "average_price >= 0");
            });

        builder.HasKey(holding => new { holding.PortfolioId, holding.Symbol });

        builder.Property(holding => holding.PortfolioId).HasColumnName("portfolio_id");
        builder.Property(holding => holding.Symbol).HasColumnName("symbol").HasMaxLength(8);
        builder.Property(holding => holding.TotalQuantity).HasColumnName("total_quantity").HasDefaultValue(0L);
        builder.Property(holding => holding.ReservedQuantity).HasColumnName("reserved_quantity").HasDefaultValue(0L);
        builder.Property(holding => holding.AveragePrice).HasColumnName("average_price").AsMoney().HasDefaultValue(0m);
        builder.Property(holding => holding.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(holding => holding.SymbolNavigation)
            .WithMany()
            .HasForeignKey(holding => holding.Symbol)
            .HasPrincipalKey(symbol => symbol.Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
