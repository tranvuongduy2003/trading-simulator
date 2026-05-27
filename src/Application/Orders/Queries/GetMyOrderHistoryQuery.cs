using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Contracts.Orders;

namespace TradingSimulator.Application.Orders.Queries;

public sealed record GetMyOrderHistoryQuery(int? PageNumber = null, int? PageSize = null)
    : IQuery<OrderHistoryResponse>;
