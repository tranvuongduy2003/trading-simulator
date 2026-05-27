using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TradingSimulator.Api.Common;
using TradingSimulator.Contracts.Portfolio;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Portfolios;

[Collection(IntegrationTestCollection.Name)]
public sealed class ResetPortfolioEligibilityTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GetResetEligibility_WithoutPriorReset_IsEligible()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_elig_none_{suffix}",
            $"reset_elig_none_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var eligibilityResponse = await client.GetAsync("/api/portfolio/reset/eligibility");
        eligibilityResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await eligibilityResponse.Content.ReadFromJsonAsync<PortfolioResetEligibilityResponse>(PortfolioResetTestHelpers.JsonOptions);
        body.Should().NotBeNull();
        body!.IsEligible.Should().BeTrue();
        body.NextEligibleAt.Should().BeNull();
    }

    [Fact]
    public async Task GetResetEligibility_WithRecentReset_ReturnsNextEligibleAt()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_elig_recent_{suffix}",
            $"reset_elig_recent_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        var resetAt = DateTimeOffset.UtcNow.AddHours(-2);
        await PortfolioResetTestHelpers.SeedPortfolioResetAsync(fixture, registration!.UserId, resetAt);

        using var eligibilityResponse = await client.GetAsync("/api/portfolio/reset/eligibility");
        eligibilityResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await eligibilityResponse.Content.ReadFromJsonAsync<PortfolioResetEligibilityResponse>(PortfolioResetTestHelpers.JsonOptions);
        body.Should().NotBeNull();
        body!.IsEligible.Should().BeFalse();
        body.NextEligibleAt.Should().NotBeNull();
        body.NextEligibleAt.Should().BeCloseTo(resetAt.AddMinutes(1440), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ResetPortfolio_WhenCooldownActive_Returns422_WithNextEligibleAt()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_cooldown_{suffix}",
            $"reset_cooldown_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        var resetAt = DateTimeOffset.UtcNow.AddHours(-2);
        var expectedNextEligibleAt = resetAt.AddMinutes(1440);
        await PortfolioResetTestHelpers.SeedPortfolioResetAsync(fixture, registration!.UserId, resetAt);

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m);

        var resetCountBefore = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var responseBody = await resetResponse.Content.ReadAsStringAsync();
        resetResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = JsonSerializer.Deserialize<ApiProblemDetails>(responseBody, PortfolioResetTestHelpers.JsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(422);
        problem.Code.Should().Be("RESET_COOLDOWN_ACTIVE");

        var nextEligibleAt = PortfolioResetTestHelpers.ParseNextEligibleAt(problem);
        nextEligibleAt.Should().BeCloseTo(expectedNextEligibleAt, TimeSpan.FromMinutes(1));

        var resetCountAfter = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);
        resetCountAfter.Should().Be(resetCountBefore);

        using var walletResponse = await client.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(PortfolioResetTestHelpers.JsonOptions);
        wallet.Should().NotBeNull();
        wallet!.TotalBalance.Should().Be(42_000m);
        wallet.ReservedBalance.Should().Be(5_000m);
    }

    [Fact]
    public async Task ResetPortfolio_AfterTwentyFiveHours_SucceedsAndAppendsResetRow()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_cooldown_ok_{suffix}",
            $"reset_cooldown_ok_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        var priorResetAt = DateTimeOffset.UtcNow.AddHours(-25);
        await PortfolioResetTestHelpers.SeedPortfolioResetAsync(fixture, registration!.UserId, priorResetAt);

        var resetCountBefore = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resetResponse.Content.ReadFromJsonAsync<PortfolioResetResponse>(PortfolioResetTestHelpers.JsonOptions);
        body.Should().NotBeNull();
        body!.ResetAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        body.NextEligibleAt.Should().BeAfter(body.ResetAt);
        body.NextEligibleAt.Should().BeCloseTo(body.ResetAt.AddMinutes(1440), TimeSpan.FromMinutes(1));
        body.Wallet.TotalBalance.Should().Be(100_000m);

        var resetCountAfter = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);
        resetCountAfter.Should().Be(resetCountBefore + 1);
    }

    [Fact]
    public async Task ResetPortfolio_FirstReset_SucceedsAndReturnsNextEligibleAt()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_first_{suffix}",
            $"reset_first_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        var resetCountBefore = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration!.UserId);
        resetCountBefore.Should().Be(0);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resetResponse.Content.ReadFromJsonAsync<PortfolioResetResponse>(PortfolioResetTestHelpers.JsonOptions);
        body.Should().NotBeNull();
        body!.ResetAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        body.NextEligibleAt.Should().BeAfter(body.ResetAt);
        body.NextEligibleAt.Should().BeCloseTo(body.ResetAt.AddMinutes(1440), TimeSpan.FromMinutes(1));

        var resetCountAfter = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);
        resetCountAfter.Should().Be(1);

        using var eligibilityResponse = await client.GetAsync("/api/portfolio/reset/eligibility");
        eligibilityResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var eligibility = await eligibilityResponse.Content.ReadFromJsonAsync<PortfolioResetEligibilityResponse>(PortfolioResetTestHelpers.JsonOptions);
        eligibility.Should().NotBeNull();
        eligibility!.IsEligible.Should().BeFalse();
        eligibility.NextEligibleAt.Should().BeCloseTo(body.NextEligibleAt, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ResetPortfolio_WhenCooldownActive_DoesNotMutateState()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_no_mutate_{suffix}",
            $"reset_no_mutate_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedPortfolioResetAsync(
            fixture,
            registration!.UserId,
            DateTimeOffset.UtcNow.AddHours(-2));

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(
            fixture,
            registration.UserId,
            symbol: "AAPL",
            quantity: 50,
            averagePrice: 150m,
            reservedQuantity: 4);

        var openOrderId = await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            registration.UserId,
            side: 0,
            type: 0,
            price: 200m,
            originalQuantity: 10,
            filledQuantity: 0,
            status: PortfolioResetTestHelpers.PendingStatus);

        var resetCountBefore = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var resetCountAfter = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);
        resetCountAfter.Should().Be(resetCountBefore);

        using var walletResponse = await client.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(PortfolioResetTestHelpers.JsonOptions);
        wallet!.TotalBalance.Should().Be(42_000m);
        wallet.ReservedBalance.Should().Be(5_000m);

        using var portfolioResponse = await client.GetAsync("/api/portfolio");
        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var portfolio = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>(PortfolioResetTestHelpers.JsonOptions);
        portfolio!.Holdings.Should().ContainSingle(holding => holding.Symbol == "AAPL" && holding.TotalQuantity == 50);

        using var openOrdersResponse = await client.GetAsync("/api/orders/open");
        openOrdersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var openOrders = await openOrdersResponse.Content.ReadFromJsonAsync<List<TradingSimulator.Contracts.Orders.OpenOrderDto>>(PortfolioResetTestHelpers.JsonOptions);
        openOrders.Should().ContainSingle(order => order.OrderId == openOrderId);
    }

    [Fact]
    public async Task ResetPortfolio_WhenCooldownActive_UsesLatestResetRow()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_latest_{suffix}",
            $"reset_latest_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedPortfolioResetAsync(fixture, registration!.UserId, DateTimeOffset.UtcNow.AddDays(-30));
        await PortfolioResetTestHelpers.SeedPortfolioResetAsync(fixture, registration.UserId, DateTimeOffset.UtcNow.AddHours(-2));

        var resetCountBefore = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var responseBody = await resetResponse.Content.ReadAsStringAsync();
        var problem = JsonSerializer.Deserialize<ApiProblemDetails>(responseBody, PortfolioResetTestHelpers.JsonOptions);
        problem!.Code.Should().Be("RESET_COOLDOWN_ACTIVE");

        var resetCountAfter = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);
        resetCountAfter.Should().Be(resetCountBefore);
    }
}
