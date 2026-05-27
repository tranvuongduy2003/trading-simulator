using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Contracts.Orders;

namespace TradingSimulator.Application.Orders.Queries;

public sealed record GetMyOpenOrdersQuery : IQuery<IReadOnlyList<OpenOrderDto>>;
