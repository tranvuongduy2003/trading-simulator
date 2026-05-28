namespace TradingSimulator.Application.Abstractions.Market;

public interface IOrderBookMarketDataNotifier
{
    Task NotifyOrderBookChangedAsync(string symbol, CancellationToken cancellationToken = default);
}
