using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TradingSimulator.Api.Common;
using TradingSimulator.Api.IntegrationTests.Portfolios;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Market;

internal static class MarketTestHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<(Guid UserId, HttpClient Client)> RegisterAndLoginAsync(
        IntegrationTestFixture fixture,
        string usernamePrefix)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"{usernamePrefix}_{suffix}@example.com";
        const string password = "SecurePass1!";
        var username = $"{usernamePrefix}_{suffix}";

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest(username, email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(JsonOptions);
        registration.Should().NotBeNull();

        using var logoutResponse = await client.PostAsync("/api/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, password));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        return (registration!.UserId, client);
    }

    public static Task AssertUnauthorizedAsync(HttpResponseMessage response) =>
        PortfolioResetTestHelpers.AssertUnauthorizedAsync(response);

    public static Task<Guid> SeedOpenBidAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        decimal price,
        long quantity,
        CancellationToken cancellationToken = default) =>
        PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            userId,
            side: 0,
            type: 0,
            price,
            originalQuantity: quantity,
            filledQuantity: 0,
            status: PortfolioResetTestHelpers.PendingStatus,
            cancellationToken: cancellationToken);

    public static Task<Guid> SeedOpenAskAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        decimal price,
        long quantity,
        CancellationToken cancellationToken = default) =>
        PortfolioResetTestHelpers.SeedOpenOrderAsync(
            fixture,
            userId,
            side: 1,
            type: 0,
            price,
            originalQuantity: quantity,
            filledQuantity: 0,
            status: PortfolioResetTestHelpers.PendingStatus,
            cancellationToken: cancellationToken);

    public static async Task AssertInvalidSymbolAsync(HttpResponseMessage response)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest,
            $"expected 400 but got {(int)response.StatusCode} with body: {responseBody}");

        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = JsonSerializer.Deserialize<ApiProblemDetails>(responseBody, JsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(400);
        problem.Code.Should().Be("INVALID_SYMBOL");
    }
}
