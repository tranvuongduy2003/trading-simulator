namespace TradingSimulator.Infrastructure.Persistence.Entities;

public sealed class TradeRecord
{
    public long Id { get; set; }

    public Guid ExternalId { get; set; }

    public required string Symbol { get; set; }

    public Guid BuyOrderId { get; set; }

    public Guid SellOrderId { get; set; }

    public Guid BuyerUserId { get; set; }

    public Guid SellerUserId { get; set; }

    public decimal Price { get; set; }

    public long Quantity { get; set; }

    public DateTimeOffset ExecutedAt { get; set; }

    public OrderRecord? BuyOrder { get; set; }

    public OrderRecord? SellOrder { get; set; }

    public SymbolRecord? SymbolNavigation { get; set; }
}
