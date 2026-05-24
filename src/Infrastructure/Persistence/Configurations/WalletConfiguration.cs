using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Configurations;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<WalletRecord>
{
    public void Configure(EntityTypeBuilder<WalletRecord> builder)
    {
        builder.ToTable(
            "wallets",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("ck_wallets_total_non_negative", "total_balance >= 0");
                tableBuilder.HasCheckConstraint("ck_wallets_reserved_non_negative", "reserved_balance >= 0");
                tableBuilder.HasCheckConstraint("ck_wallets_reserved_le_total", "reserved_balance <= total_balance");
            });

        builder.HasKey(wallet => wallet.UserId);

        builder.Property(wallet => wallet.UserId).HasColumnName("user_id");
        builder.Property(wallet => wallet.TotalBalance).HasColumnName("total_balance").AsMoney();
        builder.Property(wallet => wallet.ReservedBalance).HasColumnName("reserved_balance").AsMoney().HasDefaultValue(0m);
        builder.Property(wallet => wallet.Currency).HasColumnName("currency").HasMaxLength(3).IsFixedLength().HasDefaultValue("USD");
        builder.Property(wallet => wallet.UpdatedAt).HasColumnName("updated_at");
        builder.Property(wallet => wallet.RowVersion).AsRowVersion();
    }
}
