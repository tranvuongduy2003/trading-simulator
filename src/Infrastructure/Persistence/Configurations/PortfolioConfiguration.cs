using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Configurations;

internal sealed class PortfolioConfiguration : IEntityTypeConfiguration<PortfolioRecord>
{
    public void Configure(EntityTypeBuilder<PortfolioRecord> builder)
    {
        builder.ToTable("portfolios");

        builder.HasKey(portfolio => portfolio.Id);

        builder.Property(portfolio => portfolio.Id).HasColumnName("id");
        builder.Property(portfolio => portfolio.UserId).HasColumnName("user_id");
        builder.Property(portfolio => portfolio.CreatedAt).HasColumnName("created_at");
        builder.Property(portfolio => portfolio.UpdatedAt).HasColumnName("updated_at");
        builder.Property(portfolio => portfolio.RowVersion).AsRowVersion();

        builder.HasIndex(portfolio => portfolio.UserId).IsUnique().HasDatabaseName("ux_portfolios_user");

        builder.HasMany(portfolio => portfolio.Holdings)
            .WithOne(holding => holding.Portfolio)
            .HasForeignKey(holding => holding.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
