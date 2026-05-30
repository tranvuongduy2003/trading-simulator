using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Contracts.Market;

namespace TradingSimulator.Application.Market.Queries;

public sealed record GetRecentTradesQuery(string Symbol, int Limit = 50)
    : IQuery<RecentTradesResponse>;
