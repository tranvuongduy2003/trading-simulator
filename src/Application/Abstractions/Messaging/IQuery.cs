using MediatR;
using TradingSimulator.Application.Common;

namespace TradingSimulator.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
