using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TradingSimulator.Api.IntegrationTests.Portfolios.Fakes;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Contracts.Portfolio;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Portfolios;

[Collection(IntegrationTestCollection.Name)]
public sealed class ResetPortfolioWalletTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ResetPortfolio_WithSession_Returns200_WithContractShape()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_portfolio_{suffix}",
            $"reset_portfolio_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(
            fixture,
            registration!.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resetResponse.Content.ReadFromJsonAsync<PortfolioResetResponse>(PortfolioResetTestHelpers.JsonOptions);
        body.Should().NotBeNull();
        body!.ResetAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        body.NextEligibleAt.Should().BeAfter(body.ResetAt);
        body.NextEligibleAt.Should().BeCloseTo(body.ResetAt.AddMinutes(1440), TimeSpan.FromMinutes(1));

        body.Wallet.Currency.Should().Be("USD");
        body.Wallet.TotalBalance.Should().Be(100_000m);
        body.Wallet.ReservedBalance.Should().Be(0m);
        body.Wallet.AvailableBalance.Should().Be(100_000m);
        body.Wallet.AvailableBalance.Should().Be(body.Wallet.TotalBalance - body.Wallet.ReservedBalance);

        using var walletResponse = await client.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(PortfolioResetTestHelpers.JsonOptions);
        wallet.Should().NotBeNull();
        wallet!.TotalBalance.Should().Be(100_000m);
        wallet.ReservedBalance.Should().Be(0m);
        wallet.AvailableBalance.Should().Be(100_000m);
    }

    [Fact]
    public async Task ResetPortfolio_AfterDepletedWallet_Returns100k_OnGetWallet()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_wallet_{suffix}",
            $"reset_wallet_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(fixture, registration!.UserId, 42_000m, 5_000m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(fixture, registration.UserId, "AAPL", 50, 150m);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resetBody = await resetResponse.Content.ReadFromJsonAsync<PortfolioResetResponse>(PortfolioResetTestHelpers.JsonOptions);
        resetBody.Should().NotBeNull();
        resetBody!.Wallet.TotalBalance.Should().Be(100_000m);
        resetBody.Wallet.ReservedBalance.Should().Be(0m);
        resetBody.Wallet.AvailableBalance.Should().Be(100_000m);

        using var walletResponse = await client.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var walletBody = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(PortfolioResetTestHelpers.JsonOptions);
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

        var client = fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(fixture, registration!.UserId, 42_000m, 5_000m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(fixture, registration.UserId, "AAPL", 50, 150m);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var portfolioResponse = await client.GetAsync("/api/portfolio");
        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolioBody = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>(PortfolioResetTestHelpers.JsonOptions);
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

        var client = fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(fixture, registration!.UserId, 42_000m, 5_000m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(fixture, registration.UserId, "AAPL", 50, 150m);

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

        var sessionClient = fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await sessionClient.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(fixture, registration!.UserId, 42_000m, 5_000m);
        await PortfolioResetTestHelpers.SeedHoldingAsync(fixture, registration.UserId, "AAPL", 50, 150m);

        var beforeCount = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, registration.UserId);

        await using var throwFactory = fixture.CreateFactory(ConfigureThrowOnPortfolioResetWriteRepository);
        var failingClient = throwFactory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var loginResponse = await failingClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(request.Email, request.Password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var resetResponse = await failingClient.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using var walletResponse = await sessionClient.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var walletBody = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(PortfolioResetTestHelpers.JsonOptions);
        walletBody.Should().NotBeNull();
        walletBody!.TotalBalance.Should().Be(42_000m);
        walletBody.ReservedBalance.Should().Be(5_000m);
        walletBody.AvailableBalance.Should().Be(37_000m);

        using var portfolioResponse = await sessionClient.GetAsync("/api/portfolio");
        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var portfolioBody = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>(PortfolioResetTestHelpers.JsonOptions);
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

        var client = fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(fixture, registration!.UserId, 500m, 0m);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resetBody = await resetResponse.Content.ReadFromJsonAsync<PortfolioResetResponse>(PortfolioResetTestHelpers.JsonOptions);
        resetBody.Should().NotBeNull();
        resetBody!.Wallet.TotalBalance.Should().Be(100_000m);
        resetBody.Wallet.ReservedBalance.Should().Be(0m);
        resetBody.Wallet.AvailableBalance.Should().Be(100_000m);

        using var portfolioResponse = await client.GetAsync("/api/portfolio");
        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var portfolioBody = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>(PortfolioResetTestHelpers.JsonOptions);
        portfolioBody.Should().NotBeNull();
        portfolioBody!.Holdings.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPortfolio_ReleasesReservationsBeforeWalletReset()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_release_{suffix}",
            $"reset_release_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(fixture, registration!.UserId, 80_000m, 2_000m);
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
            status: PortfolioResetTestHelpers.PendingStatus);
        await PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            registration.UserId,
            side: 1,
            type: 0,
            price: 150m,
            originalQuantity: 10,
            filledQuantity: 0,
            status: PortfolioResetTestHelpers.PendingStatus);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var walletResponse = await client.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(PortfolioResetTestHelpers.JsonOptions);
        wallet.Should().NotBeNull();
        wallet!.TotalBalance.Should().Be(100_000m);
        wallet.ReservedBalance.Should().Be(0m);
        wallet.AvailableBalance.Should().Be(100_000m);

        using var portfolioResponse = await client.GetAsync("/api/portfolio");
        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var portfolio = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>(PortfolioResetTestHelpers.JsonOptions);
        portfolio.Should().NotBeNull();
        portfolio!.Holdings.Should().BeEmpty();
    }

    private static void ConfigureThrowOnPortfolioResetWriteRepository(IServiceCollection services)
    {
        services.RemoveAll<IPortfolioResetWriteRepository>();
        services.AddScoped<IPortfolioResetWriteRepository, ThrowOnPortfolioResetWriteRepository>();
    }
}
