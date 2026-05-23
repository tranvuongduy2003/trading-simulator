namespace TradingSimulator.Application.Abstractions.Realtime;

public interface IRealtimeHubConnectionRegistry
{
    Task TrackConnectionAsync(
        string connectionIdentifier,
        Guid? userIdentifier,
        CancellationToken cancellationToken = default);

    Task RemoveConnectionAsync(
        string connectionIdentifier,
        CancellationToken cancellationToken = default);
}
