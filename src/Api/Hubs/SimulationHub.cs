using Microsoft.AspNetCore.SignalR;
using TradingSimulator.Contracts.Realtime;

namespace TradingSimulator.Api.Hubs;

public sealed class SimulationHub : Hub<ISimulationHubClient>
{
    public async Task SubscribeToMarket(string symbol)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            RealtimeHubGroupNames.MarketForSymbol(symbol));
    }

    public async Task UnsubscribeFromMarket(string symbol)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            RealtimeHubGroupNames.MarketForSymbol(symbol));
    }

    public async Task SubscribeToUserNotifications()
    {
        var userIdentifier = GetAuthenticatedUserIdentifier();
        if (userIdentifier is null)
        {
            throw new HubException("Authentication is required for user notifications.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            RealtimeHubGroupNames.UserForIdentifier(userIdentifier.Value));
    }

    public async Task UnsubscribeFromUserNotifications()
    {
        var userIdentifier = GetAuthenticatedUserIdentifier();
        if (userIdentifier is null)
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            RealtimeHubGroupNames.UserForIdentifier(userIdentifier.Value));
    }

    private Guid? GetAuthenticatedUserIdentifier()
    {
        var claim = Context.User?.FindFirst("sub") ?? Context.User?.FindFirst("user_id");
        return claim is not null && Guid.TryParse(claim.Value, out var userIdentifier)
            ? userIdentifier
            : null;
    }
}
