using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Contracts.Portfolio;

namespace TradingSimulator.Application.Portfolios.Commands;

public sealed record ResetPortfolioCommand : ICommand<PortfolioResetResponse>;
