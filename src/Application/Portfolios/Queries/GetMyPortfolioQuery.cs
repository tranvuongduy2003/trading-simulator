using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Contracts.Portfolio;

namespace TradingSimulator.Application.Portfolios.Queries;

public sealed record GetMyPortfolioQuery : IQuery<PortfolioResponse>;
