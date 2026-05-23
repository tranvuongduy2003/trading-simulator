using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Contracts.Users;

namespace TradingSimulator.Application.Users.Queries;

public sealed record GetMyWalletQuery : IQuery<WalletResponse>;
