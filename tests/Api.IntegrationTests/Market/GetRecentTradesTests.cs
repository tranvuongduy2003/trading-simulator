using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TradingSimulator.Contracts.Market;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Market;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetRecentTradesTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GetRecentTrades_RequiresAuthentication()
    {
        var client = fixture.Factory.CreateClient();

        using var response = await client.GetAsync("/api/market/trades?symbol=AAPL&limit=50");

        await MarketTestHelpers.AssertUnauthorizedAsync(response);
    }

    [Fact]
    public async Task GetRecentTrades_Aapl_ReturnsEmptySnapshot()
    {
        var (_, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "tape_empty");

        await MarketTestHelpers.ResetAaplMarketTradesAsync(fixture);

        using var response = await client.GetAsync("/api/market/trades?symbol=AAPL&limit=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var snapshot = await response.Content.ReadFromJsonAsync<RecentTradesResponse>(
            MarketTestHelpers.JsonOptions);
        snapshot.Should().NotBeNull();
        snapshot!.Symbol.Should().Be("AAPL");
        snapshot.Trades.Should().BeEmpty();
        snapshot.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetRecentTrades_InvalidSymbol_Returns400()
    {
        var (_, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "tape_bad_symbol");

        using var response = await client.GetAsync("/api/market/trades?symbol=MSFT&limit=50");

        await MarketTestHelpers.AssertInvalidSymbolAsync(response);
    }

    [Fact]
    public async Task GetRecentTrades_InvalidLimit_Returns400()
    {
        var (_, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "tape_bad_limit");

        using var response = await client.GetAsync("/api/market/trades?symbol=AAPL&limit=51");

        await MarketTestHelpers.AssertInvalidLimitAsync(response);
    }

    [Fact]
    public async Task GetRecentTrades_ZeroLimit_Returns400()
    {
        var (_, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "tape_zero_limit");

        using var response = await client.GetAsync("/api/market/trades?symbol=AAPL&limit=0");

        await MarketTestHelpers.AssertInvalidLimitAsync(response);
    }

    [Fact]
    public async Task GetRecentTrades_WithSeededTrades_ReturnsNewestFirst()
    {
        var (buyerUserId, _) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "tape_buyer");
        var (sellerUserId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "tape_seller");

        await MarketTestHelpers.ResetAaplMarketTradesAsync(fixture);

        var oldestIdentifier = await MarketTestHelpers.SeedMarketTradeAsync(
            fixture,
            buyerUserId,
            sellerUserId,
            150.00m,
            10,
            DateTimeOffset.UtcNow.AddMinutes(-30));

        var middleIdentifier = await MarketTestHelpers.SeedMarketTradeAsync(
            fixture,
            buyerUserId,
            sellerUserId,
            150.25m,
            100,
            DateTimeOffset.UtcNow.AddMinutes(-20));

        var newestIdentifier = await MarketTestHelpers.SeedMarketTradeAsync(
            fixture,
            buyerUserId,
            sellerUserId,
            151.00m,
            25,
            DateTimeOffset.UtcNow.AddMinutes(-10));

        using var response = await client.GetAsync("/api/market/trades?symbol=AAPL&limit=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var snapshot = await response.Content.ReadFromJsonAsync<RecentTradesResponse>(
            MarketTestHelpers.JsonOptions);
        snapshot.Should().NotBeNull();
        snapshot!.Trades.Should().HaveCount(3);
        snapshot.Trades[0].TradeIdentifier.Should().Be(newestIdentifier);
        snapshot.Trades[0].Price.Should().Be(151.00m);
        snapshot.Trades[0].Quantity.Should().Be(25);
        snapshot.Trades[1].TradeIdentifier.Should().Be(middleIdentifier);
        snapshot.Trades[1].Price.Should().Be(150.25m);
        snapshot.Trades[1].Quantity.Should().Be(100);
        snapshot.Trades[2].TradeIdentifier.Should().Be(oldestIdentifier);
        snapshot.Trades[2].Price.Should().Be(150.00m);
        snapshot.Trades[2].Quantity.Should().Be(10);
        snapshot.Trades.Should().OnlyContain(
            trade => trade.TradeIdentifier != Guid.Empty
                && trade.Price > 0
                && trade.Quantity > 0
                && trade.ExecutedAt != default);
    }

    [Fact]
    public async Task GetRecentTrades_MoreThanFiftyTrades_ReturnsCap()
    {
        var (buyerUserId, _) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "tape_cap_buy");
        var (sellerUserId, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "tape_cap_sel");

        await MarketTestHelpers.ResetAaplMarketTradesAsync(fixture);

        Guid? expectedNewestIdentifier = null;
        for (var index = 0; index < 55; index++)
        {
            var executedAt = DateTimeOffset.UtcNow.AddMinutes(index);
            var tradeIdentifier = await MarketTestHelpers.SeedMarketTradeAsync(
                fixture,
                buyerUserId,
                sellerUserId,
                150m + index,
                1,
                executedAt);

            if (index == 54)
            {
                expectedNewestIdentifier = tradeIdentifier;
            }
        }

        using var response = await client.GetAsync("/api/market/trades?symbol=AAPL&limit=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var snapshot = await response.Content.ReadFromJsonAsync<RecentTradesResponse>(
            MarketTestHelpers.JsonOptions);
        snapshot.Should().NotBeNull();
        snapshot!.Trades.Should().HaveCount(50);
        expectedNewestIdentifier.Should().NotBeNull();
        snapshot.Trades[0].TradeIdentifier.Should().Be(expectedNewestIdentifier!.Value);
        snapshot.Trades.Select(trade => trade.ExecutedAt)
            .Should()
            .BeInDescendingOrder();
    }

    [Fact]
    public async Task GetRecentTrades_RedisCacheHit_ReturnsCachedTrades()
    {
        var (_, client) = await MarketTestHelpers.RegisterAndLoginAsync(fixture, "tape_cache");

        await MarketTestHelpers.ResetAaplMarketTradesAsync(fixture);

        var tradeIdentifier = Guid.NewGuid();
        var executedAt = DateTimeOffset.UtcNow;
        var cachedSnapshot = new RecentTradesResponse(
            "AAPL",
            [
                new RecentTradeItemResponse(tradeIdentifier, 150.25m, 100, executedAt),
            ],
            executedAt);

        await MarketTestHelpers.SeedRecentTradesCacheAsync(fixture, cachedSnapshot);

        using var response = await client.GetAsync("/api/market/trades?symbol=AAPL&limit=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var snapshot = await response.Content.ReadFromJsonAsync<RecentTradesResponse>(
            MarketTestHelpers.JsonOptions);
        snapshot.Should().NotBeNull();
        snapshot!.Trades.Should().ContainSingle();
        snapshot.Trades[0].TradeIdentifier.Should().Be(tradeIdentifier);
        snapshot.Trades[0].Price.Should().Be(150.25m);
        snapshot.Trades[0].Quantity.Should().Be(100);
    }
}
