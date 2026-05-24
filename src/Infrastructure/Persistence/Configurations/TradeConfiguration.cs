using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Configurations;

internal sealed class TradeConfiguration : IEntityTypeConfiguration<TradeRecord>
{
    public void Configure(EntityTypeBuilder<TradeRecord> builder)
    {
        builder.ToTable(
            "trades",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("ck_trades_quantity_positive", "quantity > 0");
                tableBuilder.HasCheckConstraint("ck_trades_price_positive", "price > 0");
                tableBuilder.HasCheckConstraint("ck_trades_orders_distinct", "buy_order_id <> sell_order_id");
                tableBuilder.HasCheckConstraint("ck_trades_users_distinct", "buyer_user_id <> seller_user_id");
            });

        builder.HasKey(trade => trade.Id);
        builder.Property(trade => trade.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(trade => trade.ExternalId).HasColumnName("external_id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(trade => trade.Symbol).HasColumnName("symbol").HasMaxLength(8);
        builder.Property(trade => trade.BuyOrderId).HasColumnName("buy_order_id");
        builder.Property(trade => trade.SellOrderId).HasColumnName("sell_order_id");
        builder.Property(trade => trade.BuyerUserId).HasColumnName("buyer_user_id");
        builder.Property(trade => trade.SellerUserId).HasColumnName("seller_user_id");
        builder.Property(trade => trade.Price).HasColumnName("price").AsMoney();
        builder.Property(trade => trade.Quantity).HasColumnName("quantity");
        builder.Property(trade => trade.ExecutedAt).HasColumnName("executed_at");

        builder.HasIndex(trade => trade.ExternalId).IsUnique().HasDatabaseName("ux_trades_external_id");
        builder.HasIndex(trade => new { trade.Symbol, trade.ExecutedAt }).IsDescending(false, true).HasDatabaseName("ix_trades_symbol_time");
        builder.HasIndex(trade => new { trade.BuyerUserId, trade.ExecutedAt }).IsDescending(false, true).HasDatabaseName("ix_trades_buyer_time");
        builder.HasIndex(trade => new { trade.SellerUserId, trade.ExecutedAt }).IsDescending(false, true).HasDatabaseName("ix_trades_seller_time");
        builder.HasIndex(trade => trade.BuyOrderId).HasDatabaseName("ix_trades_buy_order");
        builder.HasIndex(trade => trade.SellOrderId).HasDatabaseName("ix_trades_sell_order");

        builder.HasOne(trade => trade.BuyOrder)
            .WithMany()
            .HasForeignKey(trade => trade.BuyOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(trade => trade.SellOrder)
            .WithMany()
            .HasForeignKey(trade => trade.SellOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(trade => trade.SymbolNavigation)
            .WithMany()
            .HasForeignKey(trade => trade.Symbol)
            .HasPrincipalKey(symbol => symbol.Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
