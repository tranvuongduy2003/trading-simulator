namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IApplicationDatabaseContextFactory
{
    Task<IApplicationDatabaseContext> CreateApplicationDatabaseContextAsync(
        CancellationToken cancellationToken = default);
}
