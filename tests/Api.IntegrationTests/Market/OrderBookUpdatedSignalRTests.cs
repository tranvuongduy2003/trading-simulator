using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
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

            await MarketTestHelpers.NotifyOrderBookChangedAsync(fixture);

            await MarketTestHelpers.WaitUntilAsync(
                () => receivedMessages.Any(message => message.Bids.FirstOrDefault()?.Price == 150.25m),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(50));

            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.30m, 25);

            await MarketTestHelpers.NotifyOrderBookChangedAsync(fixture);

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

    [Fact]
    public async Task OrderBookUpdated_PublishesMultipleBidLevels()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_hub_multi_bid");

        try
        {
            await MarketTestHelpers.ResetAaplOrderBookAsync(fixture);
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.20m, 100);
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.25m, 200);
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.30m, 300);
            await MarketTestHelpers.ClearOrderBookSnapshotCacheAsync(fixture);

            await using var hubConnection = await MarketTestHelpers.CreateConnectedSimulationHubAsync(
                fixture.Factory,
                client);

            var receivedMessages = new ConcurrentBag<OrderBookUpdatedMessage>();
            hubConnection.On<OrderBookUpdatedMessage>(
                nameof(ISimulationHubClient.OrderBookUpdated),
                message => receivedMessages.Add(message));

            await hubConnection.InvokeAsync("SubscribeToMarket", "AAPL");
            await MarketTestHelpers.NotifyOrderBookChangedAsync(fixture);

            OrderBookUpdatedMessage message = null!;
            await MarketTestHelpers.WaitUntilAsync(
                () =>
                {
                    message = receivedMessages
                        .OrderByDescending(receivedMessage => receivedMessage.UpdatedAt)
                        .FirstOrDefault(receivedMessage => receivedMessage.Bids.Count >= 3)!;

                    return message is not null;
                },
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(50));

            message.Bids.Should().HaveCountGreaterThanOrEqualTo(3);
            message.Bids.Select(level => level.Price).Should().Equal(150.30m, 150.25m, 150.20m);
            message.Bids.Should().OnlyContain(level => level.Quantity > 0);
        }
        finally
        {
            await MarketTestHelpers.ClearUserMarketStateAsync(fixture, userId);
        }
    }

    [Fact]
    public async Task OrderBookUpdated_RemovedLevelNotInPayload()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_hub_remove");

        try
        {
            await MarketTestHelpers.ResetAaplOrderBookAsync(fixture);
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.20m, 100);
            var removedOrderId = await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.25m, 200);
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.30m, 300);
            await MarketTestHelpers.ClearOrderBookSnapshotCacheAsync(fixture);

            await using var hubConnection = await MarketTestHelpers.CreateConnectedSimulationHubAsync(
                fixture.Factory,
                client);

            var receivedMessages = new ConcurrentBag<OrderBookUpdatedMessage>();
            hubConnection.On<OrderBookUpdatedMessage>(
                nameof(ISimulationHubClient.OrderBookUpdated),
                message => receivedMessages.Add(message));

            await hubConnection.InvokeAsync("SubscribeToMarket", "AAPL");
            await MarketTestHelpers.NotifyOrderBookChangedAsync(fixture);

            await MarketTestHelpers.WaitUntilAsync(
                () => receivedMessages.Any(receivedMessage =>
                    receivedMessage.Bids.Any(level => level.Price == 150.25m)),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(50));

            await MarketTestHelpers.CancelOrderAsync(fixture, removedOrderId);
            await MarketTestHelpers.NotifyOrderBookChangedAsync(fixture);

            OrderBookUpdatedMessage message = null!;
            await MarketTestHelpers.WaitUntilAsync(
                () =>
                {
                    message = receivedMessages
                        .OrderByDescending(receivedMessage => receivedMessage.UpdatedAt)
                        .FirstOrDefault(receivedMessage =>
                            receivedMessage.Bids.All(level => level.Price != 150.25m))!;

                    return message is not null;
                },
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(50));

            message.Bids.Should().NotContain(level => level.Price == 150.25m);
            message.Bids.Should().OnlyContain(level => level.Quantity > 0);
        }
        finally
        {
            await MarketTestHelpers.ClearUserMarketStateAsync(fixture, userId);
        }
    }

    [Fact]
    public async Task OrderBookUpdated_IncludesOrderCountOnAggregatedLevel()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_hub_order_count");

        try
        {
            await MarketTestHelpers.ResetAaplOrderBookAsync(fixture);
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.25m, 100);
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.25m, 200);
            await MarketTestHelpers.ClearOrderBookSnapshotCacheAsync(fixture);

            await using var hubConnection = await MarketTestHelpers.CreateConnectedSimulationHubAsync(
                fixture.Factory,
                client);

            var receivedMessages = new ConcurrentBag<OrderBookUpdatedMessage>();
            hubConnection.On<OrderBookUpdatedMessage>(
                nameof(ISimulationHubClient.OrderBookUpdated),
                message => receivedMessages.Add(message));

            await hubConnection.InvokeAsync("SubscribeToMarket", "AAPL");
            await MarketTestHelpers.NotifyOrderBookChangedAsync(fixture);

            OrderBookUpdatedMessage message = null!;
            await MarketTestHelpers.WaitUntilAsync(
                () =>
                {
                    message = receivedMessages
                        .OrderByDescending(receivedMessage => receivedMessage.UpdatedAt)
                        .FirstOrDefault(receivedMessage =>
                            receivedMessage.Bids.Any(level => level.Price == 150.25m))!;

                    return message is not null;
                },
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(50));

            var aggregatedLevel = message.Bids.Single(level => level.Price == 150.25m);
            aggregatedLevel.Quantity.Should().Be(300);
            aggregatedLevel.OrderCount.Should().Be(2);
        }
        finally
        {
            await MarketTestHelpers.ClearUserMarketStateAsync(fixture, userId);
        }
    }

    [Fact]
    public async Task OrderBookUpdated_FirstLevelMatchesHttpSnapshot()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_hub_http_parity");

        try
        {
            await MarketTestHelpers.ResetAaplOrderBookAsync(fixture);
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.10m, 100);
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.25m, 200);
            await MarketTestHelpers.SeedOpenAskAsync(fixture, userId, 150.30m, 150);
            await MarketTestHelpers.SeedOpenAskAsync(fixture, userId, 150.40m, 250);
            await MarketTestHelpers.ClearOrderBookSnapshotCacheAsync(fixture);

            await using var hubConnection = await MarketTestHelpers.CreateConnectedSimulationHubAsync(
                fixture.Factory,
                client);

            var receivedMessages = new ConcurrentBag<OrderBookUpdatedMessage>();
            hubConnection.On<OrderBookUpdatedMessage>(
                nameof(ISimulationHubClient.OrderBookUpdated),
                message => receivedMessages.Add(message));

            await hubConnection.InvokeAsync("SubscribeToMarket", "AAPL");
            await MarketTestHelpers.NotifyOrderBookChangedAsync(fixture);

            OrderBookUpdatedMessage message = null!;
            await MarketTestHelpers.WaitUntilAsync(
                () =>
                {
                    message = receivedMessages
                        .OrderByDescending(receivedMessage => receivedMessage.UpdatedAt)
                        .FirstOrDefault(receivedMessage =>
                            receivedMessage.Bids.Count > 0 && receivedMessage.Asks.Count > 0)!;

                    return message is not null;
                },
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(50));

            var httpSnapshot = await MarketTestHelpers.GetOrderBookSnapshotAsync(client);

            message.Bids[0].Price.Should().Be(httpSnapshot.Bids[0].Price);
            message.Bids[0].Quantity.Should().Be(httpSnapshot.Bids[0].Quantity);
            message.Bids[0].OrderCount.Should().Be(httpSnapshot.Bids[0].OrderCount);
            message.Asks[0].Price.Should().Be(httpSnapshot.Asks[0].Price);
            message.Asks[0].Quantity.Should().Be(httpSnapshot.Asks[0].Quantity);
            message.Asks[0].OrderCount.Should().Be(httpSnapshot.Asks[0].OrderCount);
        }
        finally
        {
            await MarketTestHelpers.ClearUserMarketStateAsync(fixture, userId);
        }
    }

    [Fact]
    public async Task OrderBookUpdated_EmptyPayload_ClearsBothSides()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_hub_empty");

        try
        {
            await MarketTestHelpers.ResetAaplOrderBookAsync(fixture);
            await MarketTestHelpers.ClearOrderBookSnapshotCacheAsync(fixture);

            await using var hubConnection = await MarketTestHelpers.CreateConnectedSimulationHubAsync(
                fixture.Factory,
                client);

            var receivedMessages = new ConcurrentBag<OrderBookUpdatedMessage>();
            hubConnection.On<OrderBookUpdatedMessage>(
                nameof(ISimulationHubClient.OrderBookUpdated),
                message => receivedMessages.Add(message));

            await hubConnection.InvokeAsync("SubscribeToMarket", "AAPL");
            await MarketTestHelpers.NotifyOrderBookChangedAsync(fixture);

            OrderBookUpdatedMessage message = null!;
            await MarketTestHelpers.WaitUntilAsync(
                () =>
                {
                    message = receivedMessages
                        .OrderByDescending(receivedMessage => receivedMessage.UpdatedAt)
                        .FirstOrDefault()!;

                    return message is not null;
                },
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(50));

            message.Bids.Should().BeEmpty();
            message.Asks.Should().BeEmpty();

            var httpSnapshot = await MarketTestHelpers.GetOrderBookSnapshotAsync(client);
            httpSnapshot.Bids.Should().BeEmpty();
            httpSnapshot.Asks.Should().BeEmpty();
        }
        finally
        {
            await MarketTestHelpers.ClearUserMarketStateAsync(fixture, userId);
        }
    }

    [Fact]
    public async Task OrderBookUpdated_BidOnlyPayload_EmptyAsks()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_hub_bid_only");

        try
        {
            await MarketTestHelpers.ResetAaplOrderBookAsync(fixture);
            await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.25m, 100);
            await MarketTestHelpers.ClearOrderBookSnapshotCacheAsync(fixture);

            await using var hubConnection = await MarketTestHelpers.CreateConnectedSimulationHubAsync(
                fixture.Factory,
                client);

            var receivedMessages = new ConcurrentBag<OrderBookUpdatedMessage>();
            hubConnection.On<OrderBookUpdatedMessage>(
                nameof(ISimulationHubClient.OrderBookUpdated),
                message => receivedMessages.Add(message));

            await hubConnection.InvokeAsync("SubscribeToMarket", "AAPL");
            await MarketTestHelpers.NotifyOrderBookChangedAsync(fixture);

            OrderBookUpdatedMessage message = null!;
            await MarketTestHelpers.WaitUntilAsync(
                () =>
                {
                    message = receivedMessages
                        .OrderByDescending(receivedMessage => receivedMessage.UpdatedAt)
                        .FirstOrDefault(receivedMessage => receivedMessage.Bids.Count > 0)!;

                    return message is not null;
                },
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(50));

            message.Bids.Should().NotBeEmpty();
            message.Asks.Should().BeEmpty();

            var httpSnapshot = await MarketTestHelpers.GetOrderBookSnapshotAsync(client);
            httpSnapshot.Bids.Should().NotBeEmpty();
            httpSnapshot.Asks.Should().BeEmpty();
        }
        finally
        {
            await MarketTestHelpers.ClearUserMarketStateAsync(fixture, userId);
        }
    }
}
