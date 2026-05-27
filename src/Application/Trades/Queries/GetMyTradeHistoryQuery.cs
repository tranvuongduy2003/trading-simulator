using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Contracts.Trades;

namespace TradingSimulator.Application.Trades.Queries;

public sealed record GetMyTradeHistoryQuery(int? PageNumber = null, int? PageSize = null)
    : IQuery<TradeHistoryResponse>;
