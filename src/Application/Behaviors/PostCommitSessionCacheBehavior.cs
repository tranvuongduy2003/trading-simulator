using MediatR;
using Microsoft.Extensions.Logging;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Common;

namespace TradingSimulator.Application.Behaviors;

public sealed class PostCommitSessionCacheBehavior<TRequest, TResponse>(
    IPendingSessionCacheCollector pendingSessionCacheCollector,
    ISessionRedisCache sessionRedisCache,
    ILogger<PostCommitSessionCacheBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        if (response is not IResult { IsSuccess: true })
        {
            return response;
        }

        var pendingSessionCaches = pendingSessionCacheCollector.Drain();
        foreach (var entry in pendingSessionCaches)
        {
            try
            {
                await sessionRedisCache.TryWriteAsync(entry, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Failed to cache session {SessionId} in Redis after commit",
                    entry.SessionId);
            }
        }

        return response;
    }
}
