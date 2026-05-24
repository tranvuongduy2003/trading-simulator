using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Configurations;

internal sealed class PortfolioResetConfiguration : IEntityTypeConfiguration<PortfolioResetRecord>
{
    public void Configure(EntityTypeBuilder<PortfolioResetRecord> builder)
    {
        builder.ToTable("portfolio_resets");

        builder.HasKey(portfolioReset => portfolioReset.Id);
        builder.Property(portfolioReset => portfolioReset.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(portfolioReset => portfolioReset.UserId).HasColumnName("user_id");
        builder.Property(portfolioReset => portfolioReset.ResetAt).HasColumnName("reset_at");
        builder.Property(portfolioReset => portfolioReset.Reason).HasColumnName("reason").HasMaxLength(64);

        builder.HasIndex(portfolioReset => new { portfolioReset.UserId, portfolioReset.ResetAt })
            .IsDescending(false, true)
            .HasDatabaseName("ix_portfolio_resets_user_time");

        builder.HasOne(portfolioReset => portfolioReset.User)
            .WithMany(user => user.PortfolioResets)
            .HasForeignKey(portfolioReset => portfolioReset.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
