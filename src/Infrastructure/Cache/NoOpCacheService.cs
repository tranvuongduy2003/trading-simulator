using TradingSimulator.Application.Abstractions.Cache;

namespace TradingSimulator.Infrastructure.Cache;

internal sealed class NoOpCacheService : ICacheService
{
    public Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task SetAsync(
        string key,
        string value,
        TimeSpan expiry,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
