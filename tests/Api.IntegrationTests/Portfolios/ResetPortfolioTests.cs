using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TradingSimulator.Api.Common;
using TradingSimulator.Api.IntegrationTests.Portfolios.Fakes;
using TradingSimulator.Application.Abstractions.Realtime;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Contracts.Portfolio;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Portfolios;

[Collection(IntegrationTestCollection.Name)]
public sealed class ResetPortfolioTests(IntegrationTestFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const short PendingStatus = 0;
    private const short PartiallyFilledStatus = 1;
    private const short CancelledStatus = 3;

    [Fact]
    public async Task ResetPortfolio_WithoutSession_Returns401_UNAUTHORIZED()
    {
        var client = fixture.Factory.CreateClient();
        var userId = await RegisterAndGetUserIdAsync();
        var beforeCount = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, userId);

        using var response = await client.PostAsync("/api/portfolio/reset", null);

        await AssertUnauthorizedAsync(response);

        var afterCount = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, userId);
        afterCount.Should().Be(beforeCount);
    }

    [Fact]
    public async Task ResetPortfolio_WithSession_Returns200_WithContractShape()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_portfolio_{suffix}",
            $"reset_portfolio_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);

        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resetResponse.Content.ReadFromJsonAsync<PortfolioResetResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.ResetAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        body.NextEligibleAt.Should().BeAfter(body.ResetAt);
        body.NextEligibleAt.Should().BeCloseTo(
            body.ResetAt.AddMinutes(1440),
            TimeSpan.FromMinutes(1));

        body.Wallet.Should().NotBeNull();
        body.Wallet.Currency.Should().Be("USD");
        body.Wallet.TotalBalance.Should().Be(100_000m);
        body.Wallet.ReservedBalance.Should().Be(0m);
        body.Wallet.AvailableBalance.Should().Be(100_000m);
        body.Wallet.AvailableBalance.Should().Be(
            body.Wallet.TotalBalance - body.Wallet.ReservedBalance);

        using var walletResponse = await client.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(JsonOptions);
        wallet.Should().NotBeNull();
        wallet!.TotalBalance.Should().Be(100_000m);
        wallet.ReservedBalance.Should().Be(0m);
        wallet.AvailableBalance.Should().Be(100_000m);
    }

    [Fact]
    public async Task ResetPortfolio_WhenInFlight_SecondRequestReturns409()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_inflight_{suffix}",
            $"reset_inflight_{suffix}@example.com",
            "SecurePass1!");

        await using var factory = fixture.CreateFactory(ConfigureDelayingPortfolioResetWriteRepository);
        var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var firstResetTask = client.PostAsync("/api/portfolio/reset", null);
        var secondResetTask = client.PostAsync("/api/portfolio/reset", null);

        using var firstResetResponse = await firstResetTask;
        using var secondResetResponse = await secondResetTask;

        var statusCodes = new[] { firstResetResponse.StatusCode, secondResetResponse.StatusCode };
        statusCodes.Should().Contain(HttpStatusCode.OK);
        statusCodes.Should().Contain(HttpStatusCode.Conflict);

        var conflictResponse = firstResetResponse.StatusCode == HttpStatusCode.Conflict
            ? firstResetResponse
            : secondResetResponse;

        var responseBody = await conflictResponse.Content.ReadAsStringAsync();
        conflictResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = JsonSerializer.Deserialize<ApiProblemDetails>(responseBody, JsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(409);
        problem.Code.Should().Be("RESET_IN_PROGRESS");
    }

    [Fact]
    public async Task ResetPortfolio_AfterDepletedWallet_Returns100k_OnGetWallet()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_wallet_{suffix}",
            $"reset_wallet_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(
            fixture,
            registration.UserId,
            symbol: "AAPL",
            quantity: 50,
            averagePrice: 150m);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resetBody = await resetResponse.Content.ReadFromJsonAsync<PortfolioResetResponse>(JsonOptions);
        resetBody.Should().NotBeNull();
        resetBody!.Wallet.TotalBalance.Should().Be(100_000m);
        resetBody.Wallet.ReservedBalance.Should().Be(0m);
        resetBody.Wallet.AvailableBalance.Should().Be(100_000m);

        using var walletResponse = await client.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var walletBody = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(JsonOptions);
        walletBody.Should().NotBeNull();
        walletBody!.TotalBalance.Should().Be(100_000m);
        walletBody.ReservedBalance.Should().Be(0m);
        walletBody.AvailableBalance.Should().Be(100_000m);
    }

    [Fact]
    public async Task ResetPortfolio_ClearsHoldings_OnGetPortfolio()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_holdings_{suffix}",
            $"reset_holdings_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(
            fixture,
            registration.UserId,
            symbol: "AAPL",
            quantity: 50,
            averagePrice: 150m);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var portfolioResponse = await client.GetAsync("/api/portfolio");
        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolioBody = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>(JsonOptions);
        portfolioBody.Should().NotBeNull();
        portfolioBody!.Holdings.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPortfolio_InsertsPortfolioResetsRow()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_audit_{suffix}",
            $"reset_audit_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(
            fixture,
            registration.UserId,
            symbol: "AAPL",
            quantity: 50,
            averagePrice: 150m);

        var beforeCount = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterCount = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);
        afterCount.Should().Be(beforeCount + 1);
    }

    [Fact]
    public async Task ResetPortfolio_WhenWriteFails_PreResetStateUnchanged()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_rollback_{suffix}",
            $"reset_rollback_{suffix}@example.com",
            "SecurePass1!");

        var sessionClient = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await sessionClient.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(
            fixture,
            registration.UserId,
            symbol: "AAPL",
            quantity: 50,
            averagePrice: 150m);

        var beforeCount = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);

        await using var throwFactory = fixture.CreateFactory(ConfigureThrowOnPortfolioResetWriteRepository);
        var failingClient = throwFactory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var loginResponse = await failingClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(request.Email, request.Password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var resetResponse = await failingClient.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using var walletResponse = await sessionClient.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var walletBody = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(JsonOptions);
        walletBody.Should().NotBeNull();
        walletBody!.TotalBalance.Should().Be(42_000m);
        walletBody.ReservedBalance.Should().Be(5_000m);
        walletBody.AvailableBalance.Should().Be(37_000m);

        using var portfolioResponse = await sessionClient.GetAsync("/api/portfolio");
        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var portfolioBody = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>(JsonOptions);
        portfolioBody.Should().NotBeNull();
        portfolioBody!.Holdings.Should().ContainSingle();
        portfolioBody.Holdings[0].Symbol.Should().Be("AAPL");
        portfolioBody.Holdings[0].TotalQuantity.Should().Be(50);

        var afterCount = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);
        afterCount.Should().Be(beforeCount);
    }

    [Fact]
    public async Task ResetPortfolio_WithZeroHoldings_StillRestoresCash()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_zero_holdings_{suffix}",
            $"reset_zero_holdings_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 500m,
            reservedBalance: 0m);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resetBody = await resetResponse.Content.ReadFromJsonAsync<PortfolioResetResponse>(JsonOptions);
        resetBody.Should().NotBeNull();
        resetBody!.Wallet.TotalBalance.Should().Be(100_000m);
        resetBody.Wallet.ReservedBalance.Should().Be(0m);
        resetBody.Wallet.AvailableBalance.Should().Be(100_000m);

        using var portfolioResponse = await client.GetAsync("/api/portfolio");
        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var portfolioBody = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>(JsonOptions);
        portfolioBody.Should().NotBeNull();
        portfolioBody!.Holdings.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPortfolio_WithoutOpenOrdersAndHistory_Succeeds()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_no_data_{suffix}",
            $"reset_no_data_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 55_000m,
            reservedBalance: 0m);

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
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
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
            status: PendingStatus);
        var partialOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            registration.UserId,
            side: 1,
            type: 0,
            price: 150m,
            originalQuantity: 6,
            filledQuantity: 2,
            status: PartiallyFilledStatus);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        var cancelledOrders = await databaseContext.Orders
            .Where(orderRecord => orderRecord.Id == pendingOrderId || orderRecord.Id == partialOrderId)
            .ToListAsync();

        cancelledOrders.Should().HaveCount(2);
        cancelledOrders.Should().OnlyContain(orderRecord => orderRecord.Status == CancelledStatus);
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
            new WebApplicationFactoryClientOptions { HandleCookies = true });

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
            status: PendingStatus);
        var userBOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            userB!.UserId,
            side: 1,
            type: 0,
            price: 101m,
            originalQuantity: 7,
            filledQuantity: 0,
            status: PendingStatus);

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

        userAOrder.Status.Should().Be(CancelledStatus);
        userAOrder.TerminatedAt.Should().NotBeNull();
        userBOrder.Status.Should().Be(PendingStatus);
        userBOrder.TerminatedAt.Should().BeNull();
    }

    [Fact]
    public async Task ResetPortfolio_ReleasesReservationsBeforeWalletReset()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_release_{suffix}",
            $"reset_release_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 80_000m,
            reservedBalance: 2_000m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(
            fixture,
            registration.UserId,
            symbol: "AAPL",
            quantity: 40,
            averagePrice: 150m,
            reservedQuantity: 10);

        await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            registration.UserId,
            side: 0,
            type: 0,
            price: 200m,
            originalQuantity: 10,
            filledQuantity: 0,
            status: PendingStatus);
        await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            registration.UserId,
            side: 1,
            type: 0,
            price: 150m,
            originalQuantity: 10,
            filledQuantity: 0,
            status: PendingStatus);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var walletResponse = await client.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(JsonOptions);
        wallet.Should().NotBeNull();
        wallet!.TotalBalance.Should().Be(100_000m);
        wallet.ReservedBalance.Should().Be(0m);
        wallet.AvailableBalance.Should().Be(100_000m);

        using var portfolioResponse = await client.GetAsync("/api/portfolio");
        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var portfolio = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>(JsonOptions);
        portfolio.Should().NotBeNull();
        portfolio!.Holdings.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPortfolio_PublishesOrderCancellationNotifications()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_notify_{suffix}",
            $"reset_notify_{suffix}@example.com",
            "SecurePass1!");

        await using var factory = fixture.CreateFactory(ConfigureCapturingRealtimeNotificationPublisher);
        var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 100_000m,
            reservedBalance: 2_000m);
        await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            registration.UserId,
            side: 0,
            type: 0,
            price: 200m,
            originalQuantity: 10,
            filledQuantity: 0,
            status: PendingStatus);
        await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            registration.UserId,
            side: 1,
            type: 0,
            price: 201m,
            originalQuantity: 7,
            filledQuantity: 2,
            status: PartiallyFilledStatus);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var capturingPublisher = factory.Services.GetRequiredService<CapturingRealtimeNotificationPublisher>();
        capturingPublisher.CancellationNotifications.Should().HaveCount(2);
        capturingPublisher.CancellationNotifications
            .Should()
            .OnlyContain(notification => notification.UserIdentifier == registration.UserId);
        capturingPublisher.OrderBookUpdates.Should().ContainSingle();
        capturingPublisher.OrderBookUpdates[0].Symbol.Should().Be("AAPL");
        capturingPublisher.BalanceUpdates.Should().ContainSingle();
        capturingPublisher.BalanceUpdates[0].Message.AvailableCash.Should().Be(100_000m);
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
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var buyerRegisterResponse = await client.PostAsJsonAsync("/api/users", buyerRequest);
        buyerRegisterResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var buyer = await buyerRegisterResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        buyer.Should().NotBeNull();

        using var sellerRegisterResponse = await client.PostAsJsonAsync("/api/users", sellerRequest);
        sellerRegisterResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var seller = await sellerRegisterResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        seller.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            buyer!.UserId,
            totalBalance: 100_000m,
            reservedBalance: 750m);

        var now = DateTimeOffset.UtcNow.AddMinutes(-2);
        var buyerOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            buyer.UserId,
            side: 0,
            type: 0,
            price: 150m,
            originalQuantity: 10,
            filledQuantity: 5,
            status: PartiallyFilledStatus);
        var sellerOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            seller!.UserId,
            side: 1,
            type: 0,
            price: 150m,
            originalQuantity: 5,
            filledQuantity: 5,
            status: PartiallyFilledStatus);

        await using (var scope = fixture.Factory.Services.CreateAsyncScope())
        {
            var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
            await databaseContext.Trades.AddAsync(
                new TradingSimulator.Infrastructure.Persistence.Entities.TradeRecord
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

    private static void ConfigureDelayingWalletReadRepository(IServiceCollection services)
    {
        services.RemoveAll<IWalletReadRepository>();
        services.AddScoped<IWalletReadRepository, DelayingWalletReadRepository>();
    }

    private static void ConfigureDelayingPortfolioResetWriteRepository(IServiceCollection services)
    {
        services.RemoveAll<IPortfolioResetWriteRepository>();
        services.AddScoped<IPortfolioResetWriteRepository, DelayingPortfolioResetWriteRepository>();
    }

    private static void ConfigureThrowOnPortfolioResetWriteRepository(IServiceCollection services)
    {
        services.RemoveAll<IPortfolioResetWriteRepository>();
        services.AddScoped<IPortfolioResetWriteRepository, ThrowOnPortfolioResetWriteRepository>();
    }

    private static void ConfigureCapturingRealtimeNotificationPublisher(IServiceCollection services)
    {
        services.RemoveAll<IRealtimeNotificationPublisher>();
        services.AddSingleton<CapturingRealtimeNotificationPublisher>();
        services.AddSingleton<IRealtimeNotificationPublisher>(serviceProvider =>
            serviceProvider.GetRequiredService<CapturingRealtimeNotificationPublisher>());
    }

    private async Task<Guid> RegisterAndGetUserIdAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_unauth_{suffix}",
            $"reset_unauth_{suffix}@example.com",
            "SecurePass1!");

        var sessionClient = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await sessionClient.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();
        return registration!.UserId;
    }

    private static async Task AssertUnauthorizedAsync(HttpResponseMessage response)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized,
            $"expected 401 but got {(int)response.StatusCode} with body: {responseBody}");

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return;
        }

        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = JsonSerializer.Deserialize<ApiProblemDetails>(responseBody, JsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(401);
        problem.Code.Should().Be("UNAUTHORIZED");
    }
}
