using MediatR;
using TradingSimulator.Application.Common;

namespace TradingSimulator.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>, IUnitOfWorkRequest;
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IUnitOfWorkRequest;
