using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Application.Abstractions.Market;
using TradingSimulator.Contracts.Realtime;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Market;

[Collection(IntegrationTestCollection.Name)]
public sealed class OrderBookUpdatedSignalRTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task OrderBookUpdated_AfterTouchChange_ReceivesNewBestBid()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_hub_touch");

        try
        {
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.25m, 100);
            await MarketTestHelpers.SeedOpenAskAsync(fixture, userId, 150.50m, 50);

            await using var hubConnection = await MarketTestHelpers.CreateConnectedSimulationHubAsync(
                fixture.Factory,
                client);

            var receivedMessages = new ConcurrentBag<OrderBookUpdatedMessage>();
            hubConnection.On<OrderBookUpdatedMessage>(
                nameof(ISimulationHubClient.OrderBookUpdated),
                message => receivedMessages.Add(message));

            await hubConnection.InvokeAsync("SubscribeToMarket", "AAPL");

            await using (var scope = fixture.Factory.Services.CreateAsyncScope())
            {
                var notifier = scope.ServiceProvider.GetRequiredService<IOrderBookMarketDataNotifier>();
                await notifier.NotifyOrderBookChangedAsync("AAPL");
            }

            await MarketTestHelpers.WaitUntilAsync(
                () => receivedMessages.Any(message => message.Bids.FirstOrDefault()?.Price == 150.25m),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(50));

            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.30m, 25);

            await using (var scope = fixture.Factory.Services.CreateAsyncScope())
            {
                var notifier = scope.ServiceProvider.GetRequiredService<IOrderBookMarketDataNotifier>();
                await notifier.NotifyOrderBookChangedAsync("AAPL");
            }

            await MarketTestHelpers.WaitUntilAsync(
                () => receivedMessages.Any(message => message.Bids.FirstOrDefault()?.Price == 150.30m),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(50));

            var latestTouch = receivedMessages
                .OrderByDescending(message => message.UpdatedAt)
                .First(message => message.Bids.Count > 0);

            latestTouch.Bids[0].Price.Should().Be(150.30m);
            latestTouch.Asks[0].Price.Should().Be(150.50m);
        }
        finally
        {
            await MarketTestHelpers.ClearUserMarketStateAsync(fixture, userId);
        }
    }
}
