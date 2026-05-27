using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TradingSimulator.Api.Common;
using TradingSimulator.Api.IntegrationTests.Portfolios.Fakes;
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

    [Fact]
    public async Task ResetPortfolio_WithoutSession_Returns401_UNAUTHORIZED()
    {
        var client = fixture.Factory.CreateClient();

        using var response = await client.PostAsync("/api/portfolio/reset", null);

        await AssertUnauthorizedAsync(response);
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

    private static void ConfigureDelayingWalletReadRepository(IServiceCollection services)
    {
        services.RemoveAll<IWalletReadRepository>();
        services.AddScoped<IWalletReadRepository, DelayingWalletReadRepository>();
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
