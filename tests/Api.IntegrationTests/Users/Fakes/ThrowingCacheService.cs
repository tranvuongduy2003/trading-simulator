using TradingSimulator.Application.Abstractions.Cache;

namespace TradingSimulator.Api.IntegrationTests.Users.Fakes;

internal sealed class ThrowingCacheService : ICacheService
{
    public Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task SetAsync(
        string key,
        string value,
        TimeSpan expiry,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Simulated cache write failure.");

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
