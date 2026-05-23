using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<OrderRecord>
{
    public void Configure(EntityTypeBuilder<OrderRecord> builder)
    {
        builder.ToTable(
            "orders",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("ck_orders_quantity_positive", "original_quantity > 0");
                tableBuilder.HasCheckConstraint("ck_orders_filled_non_negative", "filled_quantity >= 0");
                tableBuilder.HasCheckConstraint("ck_orders_filled_le_original", "filled_quantity <= original_quantity");
                tableBuilder.HasCheckConstraint("ck_orders_side_range", "side IN (0, 1)");
                tableBuilder.HasCheckConstraint("ck_orders_type_range", "type IN (0, 1)");
                tableBuilder.HasCheckConstraint("ck_orders_status_range", "status IN (0, 1, 2, 3, 4)");
                tableBuilder.HasCheckConstraint(
                    "ck_orders_limit_has_price",
                    "(type = 0 AND price IS NOT NULL AND price > 0) OR (type = 1 AND price IS NULL)");
                tableBuilder.HasCheckConstraint(
                    "ck_orders_terminal_has_terminated_at",
                    "(status IN (2, 3, 4)) = (terminated_at IS NOT NULL)");
            });

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id).HasColumnName("id");
        builder.Property(order => order.UserId).HasColumnName("user_id");
        builder.Property(order => order.Symbol).HasColumnName("symbol").HasMaxLength(8);
        builder.Property(order => order.Side).HasColumnName("side");
        builder.Property(order => order.Type).HasColumnName("type");
        builder.Property(order => order.Price).HasColumnName("price").AsMoney();
        builder.Property(order => order.OriginalQuantity).HasColumnName("original_quantity");
        builder.Property(order => order.FilledQuantity).HasColumnName("filled_quantity").HasDefaultValue(0L);
        builder.Property(order => order.Status).HasColumnName("status").HasDefaultValue((short)0);
        builder.Property(order => order.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(255);
        builder.Property(order => order.IsSimulated).HasColumnName("is_simulated").HasDefaultValue(false);
        builder.Property(order => order.CreatedAt).HasColumnName("created_at");
        builder.Property(order => order.UpdatedAt).HasColumnName("updated_at");
        builder.Property(order => order.TerminatedAt).HasColumnName("terminated_at");
        builder.Property(order => order.RowVersion).AsRowVersion();

        builder.HasIndex(order => new { order.UserId, order.Status, order.CreatedAt })
            .IsDescending(false, false, true)
            .HasDatabaseName("ix_orders_user_status");

        builder.HasIndex(order => new { order.Symbol, order.Side, order.Price, order.CreatedAt })
            .HasDatabaseName("ix_orders_active_book")
            .HasFilter("status IN (0, 1)");

        builder.HasIndex(order => order.TerminatedAt)
            .IsDescending()
            .HasDatabaseName("ix_orders_terminated")
            .HasFilter("terminated_at IS NOT NULL");

        builder.HasOne(order => order.User)
            .WithMany(user => user.Orders)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(order => order.SymbolNavigation)
            .WithMany()
            .HasForeignKey(order => order.Symbol)
            .HasPrincipalKey(symbol => symbol.Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
