using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<UserRecord>
{
    public void Configure(EntityTypeBuilder<UserRecord> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id).HasColumnName("id");
        builder.Property(user => user.Username).HasColumnName("username").HasMaxLength(32).IsRequired();
        builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        builder.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        builder.Property(user => user.CreatedAt).HasColumnName("created_at");
        builder.Property(user => user.UpdatedAt).HasColumnName("updated_at");
        builder.Property(user => user.RowVersion).AsRowVersion();

        builder.HasIndex(user => user.Username).IsUnique().HasDatabaseName("ux_users_username");
        builder.HasIndex(user => user.Email).IsUnique().HasDatabaseName("ux_users_email");

        builder.HasOne(user => user.Wallet)
            .WithOne(wallet => wallet.User)
            .HasForeignKey<WalletRecord>(wallet => wallet.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(user => user.Portfolio)
            .WithOne(portfolio => portfolio.User)
            .HasForeignKey<PortfolioRecord>(portfolio => portfolio.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
