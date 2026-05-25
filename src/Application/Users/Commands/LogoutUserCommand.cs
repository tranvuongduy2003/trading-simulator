using TradingSimulator.Application.Abstractions.Messaging;

namespace TradingSimulator.Application.Users.Commands;

public sealed record LogoutUserCommand(Guid SessionId, Guid UserId) : ICommand;
