using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Infrastructure.Persistence.Entities;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Portfolios;

[Collection(IntegrationTestCollection.Name)]
public sealed class ResetPortfolioOrdersHistoryTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ResetPortfolio_WithoutOpenOrdersAndHistory_Succeeds()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_no_data_{suffix}",
            $"reset_no_data_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(fixture, registration!.UserId, 55_000m, 0m);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var openOrdersResponse = await client.GetAsync("/api/orders/open");
        openOrdersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var openOrders = await openOrdersResponse.Content.ReadFromJsonAsync<List<TradingSimulator.Contracts.Orders.OpenOrderDto>>();
        openOrders.Should().NotBeNull();
        openOrders!.Should().BeEmpty();

        using var orderHistoryResponse = await client.GetAsync("/api/orders/history");
        orderHistoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderHistory = await orderHistoryResponse.Content.ReadFromJsonAsync<TradingSimulator.Contracts.Orders.OrderHistoryResponse>();
        orderHistory.Should().NotBeNull();
        orderHistory!.Items.Should().BeEmpty();

        using var tradeHistoryResponse = await client.GetAsync("/api/trades");
        tradeHistoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tradeHistory = await tradeHistoryResponse.Content.ReadFromJsonAsync<TradingSimulator.Contracts.Trades.TradeHistoryResponse>();
        tradeHistory.Should().NotBeNull();
        tradeHistory!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPortfolio_WhenUserHasOpenOrders_CancelsPendingAndPartiallyFilled()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_cancel_{suffix}",
            $"reset_cancel_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 100_000m,
            reservedBalance: 3_300m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(
            fixture,
            registration.UserId,
            symbol: "AAPL",
            quantity: 30,
            averagePrice: 150m,
            reservedQuantity: 4);

        var pendingOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            registration.UserId,
            side: 0,
            type: 0,
            price: 200m,
            originalQuantity: 10,
            filledQuantity: 0,
            status: PortfolioResetTestHelpers.PendingStatus);
        var partialOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            registration.UserId,
            side: 1,
            type: 0,
            price: 150m,
            originalQuantity: 6,
            filledQuantity: 2,
            status: PortfolioResetTestHelpers.PartiallyFilledStatus);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        var cancelledOrders = await databaseContext.Orders
            .Where(orderRecord => orderRecord.Id == pendingOrderId || orderRecord.Id == partialOrderId)
            .ToListAsync();

        cancelledOrders.Should().HaveCount(2);
        cancelledOrders.Should().OnlyContain(orderRecord => orderRecord.Status == PortfolioResetTestHelpers.CancelledStatus);
        cancelledOrders.Should().OnlyContain(orderRecord => orderRecord.TerminatedAt.HasValue);
    }

    [Fact]
    public async Task ResetPortfolio_CancelsOnlyCurrentUserOpenOrders()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userARequest = new RegisterUserRequest(
            $"reset_scope_a_{suffix}",
            $"reset_scope_a_{suffix}@example.com",
            "SecurePass1!");
        var userBRequest = new RegisterUserRequest(
            $"reset_scope_b_{suffix}",
            $"reset_scope_b_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var userARegisterResponse = await client.PostAsJsonAsync("/api/users", userARequest);
        userARegisterResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var userA = await userARegisterResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        userA.Should().NotBeNull();

        using var userBRegisterResponse = await client.PostAsJsonAsync("/api/users", userBRequest);
        userBRegisterResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var userB = await userBRegisterResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        userB.Should().NotBeNull();

        var userAOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            userA!.UserId,
            side: 0,
            type: 0,
            price: 100m,
            originalQuantity: 5,
            filledQuantity: 0,
            status: PortfolioResetTestHelpers.PendingStatus);
        var userBOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            userB!.UserId,
            side: 1,
            type: 0,
            price: 101m,
            originalQuantity: 7,
            filledQuantity: 0,
            status: PortfolioResetTestHelpers.PendingStatus);

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(userARequest.Email, userARequest.Password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        var userAOrder = await databaseContext.Orders.SingleAsync(orderRecord => orderRecord.Id == userAOrderId);
        var userBOrder = await databaseContext.Orders.SingleAsync(orderRecord => orderRecord.Id == userBOrderId);

        userAOrder.Status.Should().Be(PortfolioResetTestHelpers.CancelledStatus);
        userAOrder.TerminatedAt.Should().NotBeNull();
        userBOrder.Status.Should().Be(PortfolioResetTestHelpers.PendingStatus);
        userBOrder.TerminatedAt.Should().BeNull();
    }

    [Fact]
    public async Task ResetPortfolio_WithPartiallyFilledOrder_HistoryHiddenForCurrentUser()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var buyerRequest = new RegisterUserRequest(
            $"reset_partial_buyer_{suffix}",
            $"reset_partial_buyer_{suffix}@example.com",
            "SecurePass1!");
        var sellerRequest = new RegisterUserRequest(
            $"reset_partial_seller_{suffix}",
            $"reset_partial_seller_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var buyerRegisterResponse = await client.PostAsJsonAsync("/api/users", buyerRequest);
        buyerRegisterResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var buyer = await buyerRegisterResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        buyer.Should().NotBeNull();

        using var sellerRegisterResponse = await client.PostAsJsonAsync("/api/users", sellerRequest);
        sellerRegisterResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var seller = await sellerRegisterResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        seller.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(fixture, buyer!.UserId, 100_000m, 750m);

        var now = DateTimeOffset.UtcNow.AddMinutes(-2);
        var buyerOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            buyer.UserId,
            side: 0,
            type: 0,
            price: 150m,
            originalQuantity: 10,
            filledQuantity: 5,
            status: PortfolioResetTestHelpers.PartiallyFilledStatus);
        var sellerOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            seller!.UserId,
            side: 1,
            type: 0,
            price: 150m,
            originalQuantity: 5,
            filledQuantity: 5,
            status: PortfolioResetTestHelpers.PartiallyFilledStatus);

        await using (var scope = fixture.Factory.Services.CreateAsyncScope())
        {
            var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
            await databaseContext.Trades.AddAsync(
                new TradeRecord
                {
                    ExternalId = Guid.NewGuid(),
                    Symbol = "AAPL",
                    BuyOrderId = buyerOrderId,
                    SellOrderId = sellerOrderId,
                    BuyerUserId = buyer.UserId,
                    SellerUserId = seller.UserId,
                    Price = 150m,
                    Quantity = 5,
                    ExecutedAt = now,
                });
            await databaseContext.SaveChangesAsync();
        }

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(buyerRequest.Email, buyerRequest.Password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var orderHistoryResponse = await client.GetAsync("/api/orders/history");
        orderHistoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderHistory = await orderHistoryResponse.Content.ReadFromJsonAsync<TradingSimulator.Contracts.Orders.OrderHistoryResponse>();
        orderHistory.Should().NotBeNull();
        orderHistory!.Items.Should().BeEmpty();

        using var tradeHistoryResponse = await client.GetAsync("/api/trades");
        tradeHistoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tradeHistory = await tradeHistoryResponse.Content.ReadFromJsonAsync<TradingSimulator.Contracts.Trades.TradeHistoryResponse>();
        tradeHistory.Should().NotBeNull();
        tradeHistory!.Items.Should().BeEmpty();
    }
}
