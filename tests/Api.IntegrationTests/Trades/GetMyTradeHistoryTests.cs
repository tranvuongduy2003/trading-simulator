using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Contracts.Trades;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Infrastructure.Persistence.Entities;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Trades;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetMyTradeHistoryTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GetMyTradeHistory_AfterReset_ReturnsEmpty()
    {
        var buyerUser = await RegisterUserAsync("trade_history_buyer");
        var sellerUser = await RegisterUserAsync("trade_history_seller");

        await SeedTradeAsync(
            buyerUser.UserId,
            sellerUser.UserId,
            DateTimeOffset.UtcNow.AddMinutes(-10));

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(buyerUser.Email, buyerUser.Password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var historyResponse = await client.GetAsync("/api/trades");
        historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await historyResponse.Content.ReadFromJsonAsync<TradeHistoryResponse>();
        history.Should().NotBeNull();
        history!.PageNumber.Should().Be(1);
        history.PageSize.Should().Be(25);
        history.TotalCount.Should().Be(0);
        history.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPortfolio_DoesNotHideCounterpartyOwnHistory()
    {
        var buyerUser = await RegisterUserAsync("trade_rst_buy");
        var sellerUser = await RegisterUserAsync("trade_rst_sel");

        await SeedTradeAsync(
            buyerUser.UserId,
            sellerUser.UserId,
            DateTimeOffset.UtcNow.AddMinutes(-10));

        var buyerClient = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });
        using var buyerLoginResponse = await buyerClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(buyerUser.Email, buyerUser.Password));
        buyerLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var buyerResetResponse = await buyerClient.PostAsync("/api/portfolio/reset", null);
        buyerResetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var sellerClient = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });
        using var sellerLoginResponse = await sellerClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(sellerUser.Email, sellerUser.Password));
        sellerLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var sellerHistoryResponse = await sellerClient.GetAsync("/api/trades");
        sellerHistoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var sellerHistory = await sellerHistoryResponse.Content.ReadFromJsonAsync<TradeHistoryResponse>();
        sellerHistory.Should().NotBeNull();
        sellerHistory!.TotalCount.Should().Be(1);
        sellerHistory.Items.Should().ContainSingle();
    }

    private async Task<(Guid UserId, string Email, string Password)> RegisterUserAsync(string prefix)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"{prefix}_{suffix}@example.com";
        const string password = "SecurePass1!";
        var username = $"{prefix}_{suffix}";

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest(username, email, password));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        return (registration!.UserId, email, password);
    }

    private async Task SeedTradeAsync(
        Guid buyerUserId,
        Guid sellerUserId,
        DateTimeOffset executedAt)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var buyOrderId = Guid.NewGuid();
        var sellOrderId = Guid.NewGuid();

        await databaseContext.Orders.AddRangeAsync(
            new OrderRecord
            {
                Id = buyOrderId,
                UserId = buyerUserId,
                Symbol = "AAPL",
                Side = 0,
                Type = 0,
                Price = 150m,
                OriginalQuantity = 5,
                FilledQuantity = 5,
                Status = 2,
                IsSimulated = false,
                CreatedAt = executedAt.AddMinutes(-1),
                UpdatedAt = executedAt,
                TerminatedAt = executedAt,
            },
            new OrderRecord
            {
                Id = sellOrderId,
                UserId = sellerUserId,
                Symbol = "AAPL",
                Side = 1,
                Type = 0,
                Price = 150m,
                OriginalQuantity = 5,
                FilledQuantity = 5,
                Status = 2,
                IsSimulated = false,
                CreatedAt = executedAt.AddMinutes(-1),
                UpdatedAt = executedAt,
                TerminatedAt = executedAt,
            });

        await databaseContext.Trades.AddAsync(
            new TradeRecord
            {
                ExternalId = Guid.NewGuid(),
                Symbol = "AAPL",
                BuyOrderId = buyOrderId,
                SellOrderId = sellOrderId,
                BuyerUserId = buyerUserId,
                SellerUserId = sellerUserId,
                Price = 150m,
                Quantity = 5,
                ExecutedAt = executedAt,
            });

        await databaseContext.SaveChangesAsync();
    }
}
