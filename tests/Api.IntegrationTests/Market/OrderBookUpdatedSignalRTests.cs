using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Application.Abstractions.Realtime;
using TradingSimulator.Contracts.Realtime;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Market;

[Collection(IntegrationTestCollection.Name)]
public sealed class OrderBookUpdatedSignalRTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task OrderBookUpdated_AfterProjectionPublish_ReflectsImprovedBid()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_signalr");
        await using var factory = fixture.CreateFactory();

        var receivedMessages = new ConcurrentBag<OrderBookUpdatedMessage>();
        HubConnection? hubConnection = null;

        try
        {
        hubConnection = await MarketTestHelpers.ConnectSimulationHubAsync(
            factory,
            factory.Server.CreateHandler(),
            message => receivedMessages.Add(message));

        await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.25m, 100);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var projection = scope.ServiceProvider.GetRequiredService<IOrderBookRealtimeProjection>();
            await projection.PublishForSymbolAsync("AAPL");
        }

        var firstMessage = await MarketTestHelpers.WaitForOrderBookUpdateAsync(
            receivedMessages,
            message => message.Bids.Count > 0 && message.Bids[0].Price == 150.25m,
            TimeSpan.FromSeconds(10));
        firstMessage.Bids[0].Price.Should().Be(150.25m);

        await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.30m, 100);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var projection = scope.ServiceProvider.GetRequiredService<IOrderBookRealtimeProjection>();
            await projection.PublishForSymbolAsync("AAPL");
        }

        var improvedMessage = await MarketTestHelpers.WaitForOrderBookUpdateAsync(
            receivedMessages,
            message => message.Bids.Count > 0 && message.Bids[0].Price == 150.30m,
            TimeSpan.FromSeconds(10));
        improvedMessage.Bids[0].Price.Should().Be(150.30m);
        }
        finally
        {
            if (hubConnection is not null)
            {
                await hubConnection.StopAsync();
                await hubConnection.DisposeAsync();
            }

            await MarketTestHelpers.ResetUserPortfolioAsync(client);
            await MarketTestHelpers.CleanupAaplOrderBookAsync(fixture);
            client.Dispose();
        }
    }
}
