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
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Contracts.Portfolio;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Infrastructure.Persistence.Entities;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Portfolios;

[Collection(IntegrationTestCollection.Name)]
public sealed class ResetPortfolioTests(IntegrationTestFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task ResetPortfolio_WithoutSession_Returns401_NoResetRow()
    {
        var rowsBefore = await CountPortfolioResetRowsAsync();
        var client = fixture.Factory.CreateClient();

        using var response = await client.PostAsync("/api/portfolio/reset", null);

        await AssertUnauthorizedAsync(response);
        var rowsAfter = await CountPortfolioResetRowsAsync();
        rowsAfter.Should().Be(rowsBefore);
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
    }

    [Fact]
    public async Task ResetPortfolio_WhenInFlight_SecondRequestReturns409()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_inflight_{suffix}",
            $"reset_inflight_{suffix}@example.com",
            "SecurePass1!");

        await using var factory = fixture.CreateFactory(ConfigureDelayingWalletReadRepository);
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
    public async Task ResetPortfolio_AfterSuccess_GetWalletReturnsResetSnapshot()
    {
        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });
        var user = await RegisterUserAsync(client, "reset_depleted");

        await SeedWalletAndHoldingAsync(
            user.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m,
            holdingQuantity: 0);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resetBody = await resetResponse.Content.ReadFromJsonAsync<PortfolioResetResponse>(JsonOptions);
        resetBody.Should().NotBeNull();
        resetBody!.Wallet.TotalBalance.Should().Be(100_000m);
        resetBody.Wallet.ReservedBalance.Should().Be(0m);
        resetBody.Wallet.AvailableBalance.Should().Be(100_000m);

        using var walletResponse = await client.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>(JsonOptions);
        wallet.Should().NotBeNull();
        wallet!.TotalBalance.Should().Be(100_000m);
        wallet.ReservedBalance.Should().Be(0m);
        wallet.AvailableBalance.Should().Be(100_000m);
        wallet.Currency.Should().Be("USD");

        wallet.TotalBalance.Should().Be(resetBody.Wallet.TotalBalance);
        wallet.ReservedBalance.Should().Be(resetBody.Wallet.ReservedBalance);
        wallet.AvailableBalance.Should().Be(resetBody.Wallet.AvailableBalance);
        wallet.Currency.Should().Be(resetBody.Wallet.Currency);
    }

    [Fact]
    public async Task ResetPortfolio_AfterSuccess_GetPortfolioReturnsEmptyHoldings()
    {
        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });
        var user = await RegisterUserAsync(client, "reset_holdings");

        await SeedWalletAndHoldingAsync(
            user.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m,
            holdingQuantity: 50);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var portfolioResponse = await client.GetAsync("/api/portfolio");
        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolio = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>(JsonOptions);
        portfolio.Should().NotBeNull();
        portfolio!.Holdings.Should().BeEmpty();
        portfolio.UserId.Should().Be(user.UserId);
    }

    [Fact]
    public async Task ResetPortfolio_WhenMutationFails_RollsBackWalletAndHoldings()
    {
        await using var factory = fixture.CreateFactory(ConfigureFailingPortfolioRepository);
        var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });
        var user = await RegisterUserAsync(client, "reset_rollback");

        await SeedWalletAndHoldingAsync(
            user.UserId,
            totalBalance: 42_000m,
            reservedBalance: 5_000m,
            holdingQuantity: 50);

        var walletBefore = await client.GetFromJsonAsync<WalletResponse>("/api/wallet", JsonOptions);
        var portfolioBefore = await client.GetFromJsonAsync<PortfolioResponse>("/api/portfolio", JsonOptions);
        walletBefore.Should().NotBeNull();
        portfolioBefore.Should().NotBeNull();
        portfolioBefore!.Holdings.Should().HaveCount(1);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var problem = await resetResponse.Content.ReadFromJsonAsync<ApiProblemDetails>(JsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
        problem.Code.Should().Be("INTERNAL_ERROR");

        var walletAfter = await client.GetFromJsonAsync<WalletResponse>("/api/wallet", JsonOptions);
        var portfolioAfter = await client.GetFromJsonAsync<PortfolioResponse>("/api/portfolio", JsonOptions);
        walletAfter.Should().NotBeNull();
        portfolioAfter.Should().NotBeNull();

        walletAfter!.TotalBalance.Should().Be(walletBefore!.TotalBalance);
        walletAfter.ReservedBalance.Should().Be(walletBefore.ReservedBalance);
        walletAfter.AvailableBalance.Should().Be(walletBefore.AvailableBalance);
        walletAfter.Currency.Should().Be(walletBefore.Currency);

        portfolioAfter!.UserId.Should().Be(portfolioBefore.UserId);
        portfolioAfter.Holdings.Should().HaveCount(portfolioBefore.Holdings.Count);
        portfolioAfter.Holdings[0].Symbol.Should().Be(portfolioBefore.Holdings[0].Symbol);
        portfolioAfter.Holdings[0].TotalQuantity.Should().Be(portfolioBefore.Holdings[0].TotalQuantity);
        portfolioAfter.Holdings[0].ReservedQuantity.Should().Be(portfolioBefore.Holdings[0].ReservedQuantity);
        portfolioAfter.Holdings[0].AveragePrice.Should().Be(portfolioBefore.Holdings[0].AveragePrice);
    }

    private static void ConfigureDelayingWalletReadRepository(IServiceCollection services)
    {
        services.RemoveAll<IWalletReadRepository>();
        services.AddScoped<IWalletReadRepository, DelayingWalletReadRepository>();
    }

    private static void ConfigureFailingPortfolioRepository(IServiceCollection services)
    {
        services.RemoveAll<IPortfolioRepository>();
        services.AddScoped<IPortfolioRepository, FailingPortfolioRepository>();
    }

    private async Task<UserRegistrationResponse> RegisterUserAsync(HttpClient client, string usernamePrefix)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"{usernamePrefix}_{suffix}",
            $"{usernamePrefix}_{suffix}@example.com",
            "SecurePass1!");

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(JsonOptions);
        body.Should().NotBeNull();
        return body!;
    }

    private async Task SeedWalletAndHoldingAsync(
        Guid userIdentifier,
        decimal totalBalance,
        decimal reservedBalance,
        long holdingQuantity)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var wallet = await databaseContext.Wallets.SingleAsync(record => record.UserId == userIdentifier);
        wallet.TotalBalance = totalBalance;
        wallet.ReservedBalance = reservedBalance;
        wallet.UpdatedAt = DateTimeOffset.UtcNow;

        var portfolio = await databaseContext.Portfolios.SingleAsync(record => record.UserId == userIdentifier);

        var existingHolding = await databaseContext.Holdings.FindAsync(portfolio.Id, "AAPL");
        if (existingHolding is not null)
        {
            databaseContext.Holdings.Remove(existingHolding);
        }

        if (holdingQuantity > 0)
        {
            databaseContext.Holdings.Add(
                new HoldingRecord
                {
                    PortfolioId = portfolio.Id,
                    Symbol = "AAPL",
                    TotalQuantity = holdingQuantity,
                    ReservedQuantity = 0,
                    AveragePrice = 150m,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
        }

        await databaseContext.SaveChangesAsync();
    }

    private async Task<int> CountPortfolioResetRowsAsync()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        return await databaseContext.PortfolioResets.CountAsync();
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
