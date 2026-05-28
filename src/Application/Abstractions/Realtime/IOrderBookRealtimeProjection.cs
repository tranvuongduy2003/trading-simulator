namespace TradingSimulator.Application.Abstractions.Realtime;

public interface IOrderBookRealtimeProjection
{
    Task PublishForSymbolAsync(string symbol, CancellationToken cancellationToken = default);
}
