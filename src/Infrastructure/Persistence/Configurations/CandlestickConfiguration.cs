using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingSimulator.Infrastructure.Persistence.Entities;

namespace TradingSimulator.Infrastructure.Persistence.Configurations;

internal sealed class CandlestickConfiguration : IEntityTypeConfiguration<CandlestickRecord>
{
    public void Configure(EntityTypeBuilder<CandlestickRecord> builder)
    {
        builder.ToTable(
            "candlesticks",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_candlesticks_prices_positive",
                    "open_price > 0 AND high_price > 0 AND low_price > 0 AND close_price > 0");
                tableBuilder.HasCheckConstraint("ck_candlesticks_high_low", "high_price >= low_price");
                tableBuilder.HasCheckConstraint("ck_candlesticks_volume_non_negative", "volume >= 0");
            });

        builder.HasKey(candlestick => new { candlestick.Symbol, candlestick.Interval, candlestick.BucketStart });

        builder.Property(candlestick => candlestick.Symbol).HasColumnName("symbol").HasMaxLength(8);
        builder.Property(candlestick => candlestick.Interval).HasColumnName("interval");
        builder.Property(candlestick => candlestick.BucketStart).HasColumnName("bucket_start");
        builder.Property(candlestick => candlestick.OpenPrice).HasColumnName("open_price").AsMoney();
        builder.Property(candlestick => candlestick.HighPrice).HasColumnName("high_price").AsMoney();
        builder.Property(candlestick => candlestick.LowPrice).HasColumnName("low_price").AsMoney();
        builder.Property(candlestick => candlestick.ClosePrice).HasColumnName("close_price").AsMoney();
        builder.Property(candlestick => candlestick.Volume).HasColumnName("volume").HasDefaultValue(0L);
        builder.Property(candlestick => candlestick.TradeCount).HasColumnName("trade_count").HasDefaultValue(0);
        builder.Property(candlestick => candlestick.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(candlestick => candlestick.SymbolNavigation)
            .WithMany()
            .HasForeignKey(candlestick => candlestick.Symbol)
            .HasPrincipalKey(symbol => symbol.Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
