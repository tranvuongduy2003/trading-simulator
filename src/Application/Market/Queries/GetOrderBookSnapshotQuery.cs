using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Contracts.Market;

namespace TradingSimulator.Application.Market.Queries;

public sealed record GetOrderBookSnapshotQuery(string Symbol, int Depth = 10)
    : IQuery<OrderBookSnapshotResponse>;
