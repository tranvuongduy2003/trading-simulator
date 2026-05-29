using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Application.Abstractions.Cache;
using TradingSimulator.Contracts.Market;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Market;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetOrderBookSnapshotTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GetOrderBookSnapshot_RequiresAuthentication()
    {
        var client = fixture.Factory.CreateClient();

        using var response = await client.GetAsync("/api/market/orderbook?symbol=AAPL");

        await MarketTestHelpers.AssertUnauthorizedAsync(response);
    }

    [Fact]
    public async Task GetOrderBookSnapshot_Aapl_ReturnsEmptySnapshot()
    {
        var (_, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_empty");

        using var response = await client.GetAsync("/api/market/orderbook?symbol=AAPL");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var snapshot = await response.Content.ReadFromJsonAsync<OrderBookSnapshotResponse>(
            MarketTestHelpers.JsonOptions);
        snapshot.Should().NotBeNull();
        snapshot!.Symbol.Should().Be("AAPL");
        snapshot.Bids.Should().BeEmpty();
        snapshot.Asks.Should().BeEmpty();
        snapshot.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetOrderBookSnapshot_InvalidSymbol_Returns400()
    {
        var (_, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_bad_symbol");

        using var response = await client.GetAsync("/api/market/orderbook?symbol=MSFT");

        await MarketTestHelpers.AssertInvalidSymbolAsync(response);
    }

    [Fact]
    public async Task GetOrderBookSnapshot_BidOnly_ReturnsEmptyAsks()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_bid_only");

        await MarketTestHelpers.ResetAaplOrderBookAsync(fixture);
        await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.25m, 100);
        await MarketTestHelpers.ClearOrderBookSnapshotCacheAsync(fixture);

        using var response = await client.GetAsync("/api/market/orderbook?symbol=AAPL");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var snapshot = await response.Content.ReadFromJsonAsync<OrderBookSnapshotResponse>(
            MarketTestHelpers.JsonOptions);
        snapshot.Should().NotBeNull();
        snapshot!.Bids.Should().ContainSingle();
        snapshot.Bids[0].Price.Should().Be(150.25m);
        snapshot.Bids[0].Quantity.Should().Be(100);
        snapshot.Asks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrderBookSnapshot_AskOnly_ReturnsEmptyBids()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_ask_only");

        await MarketTestHelpers.ResetAaplOrderBookAsync(fixture);
        await MarketTestHelpers.SeedOpenAskAsync(fixture, userId, 150.50m, 50);
        await MarketTestHelpers.ClearOrderBookSnapshotCacheAsync(fixture);

        using var response = await client.GetAsync("/api/market/orderbook?symbol=AAPL");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var snapshot = await response.Content.ReadFromJsonAsync<OrderBookSnapshotResponse>(
            MarketTestHelpers.JsonOptions);
        snapshot.Should().NotBeNull();
        snapshot!.Bids.Should().BeEmpty();
        snapshot.Asks.Should().ContainSingle();
        snapshot.Asks[0].Price.Should().Be(150.50m);
        snapshot.Asks[0].Quantity.Should().Be(50);
    }

    [Fact]
    public async Task GetOrderBookSnapshot_WithSeededOrders_ReturnsBestBidAndAsk()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_seeded");

        await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 150.25m, 100);
        await MarketTestHelpers.SeedOpenAskAsync(fixture, userId, 150.50m, 50);

        using var response = await client.GetAsync("/api/market/orderbook?symbol=AAPL");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var snapshot = await response.Content.ReadFromJsonAsync<OrderBookSnapshotResponse>(
            MarketTestHelpers.JsonOptions);
        snapshot.Should().NotBeNull();
        snapshot!.Bids.Should().ContainSingle();
        snapshot.Bids[0].Price.Should().Be(150.25m);
        snapshot.Bids[0].Quantity.Should().Be(100);
        snapshot.Bids[0].OrderCount.Should().Be(1);

        snapshot.Asks.Should().ContainSingle();
        snapshot.Asks[0].Price.Should().Be(150.50m);
        snapshot.Asks[0].Quantity.Should().Be(50);
        snapshot.Asks[0].OrderCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOrderBookSnapshot_WhenRedisPopulated_ReturnsCachedLevels()
    {
        var (userId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "market_redis");

        await MarketTestHelpers.SeedOpenBidAsync(fixture, userId, 149.00m, 10);
        await MarketTestHelpers.SeedOpenAskAsync(fixture, userId, 151.00m, 10);

        var cachedSnapshot = new OrderBookSnapshotResponse(
            "AAPL",
            [new OrderBookLevelResponse(150.25m, 100, 2)],
            [new OrderBookLevelResponse(150.50m, 50, 1)],
            new DateTimeOffset(2026, 5, 29, 1, 0, 0, TimeSpan.Zero));

        await using (var scope = fixture.Factory.Services.CreateAsyncScope())
        {
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var json = JsonSerializer.Serialize(cachedSnapshot, MarketTestHelpers.JsonOptions);
            await cacheService.SetAsync(
                "orderbook:AAPL:snapshot",
                json,
                TimeSpan.FromMinutes(5));
        }

        using var response = await client.GetAsync("/api/market/orderbook?symbol=AAPL");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var snapshot = await response.Content.ReadFromJsonAsync<OrderBookSnapshotResponse>(
            MarketTestHelpers.JsonOptions);
        snapshot.Should().NotBeNull();
        snapshot!.UpdatedAt.Should().Be(cachedSnapshot.UpdatedAt);
        snapshot.Bids[0].Price.Should().Be(150.25m);
        snapshot.Asks[0].Price.Should().Be(150.50m);
    }
}
