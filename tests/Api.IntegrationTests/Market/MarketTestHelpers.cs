using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Api.Common;
using TradingSimulator.Api.IntegrationTests.Portfolios;
using TradingSimulator.Application.Abstractions.Cache;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;
using ClientHubConnection = Microsoft.AspNetCore.SignalR.Client.HubConnection;

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

    public static async Task<ClientHubConnection> CreateConnectedSimulationHubAsync(
        IntegrationTestWebApplicationFactory factory,
        HttpClient authenticatedClient,
        CancellationToken cancellationToken = default)
    {
        var hubUri = new Uri(authenticatedClient.BaseAddress!, "/hubs/simulation");
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUri, httpConnectionOptions =>
            {
                httpConnectionOptions.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                var cookieContainer = ExtractCookieContainer(authenticatedClient);
                if (cookieContainer is not null)
                {
                    httpConnectionOptions.Cookies = cookieContainer;
                }
            })
            .Build();

        await connection.StartAsync(cancellationToken);
        return connection;
    }

    public static async Task ClearOrderBookSnapshotCacheAsync(
        IntegrationTestFixture fixture,
        CancellationToken cancellationToken = default)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        await cacheService.DeleteAsync("orderbook:AAPL:snapshot", cancellationToken);
    }

    public static async Task ClearUserMarketStateAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        await databaseContext.Orders
            .Where(orderRecord => orderRecord.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await ClearOrderBookSnapshotCacheAsync(fixture, cancellationToken);
    }

    public static async Task WaitUntilAsync(
        Func<bool> condition,
        TimeSpan timeout,
        TimeSpan pollInterval)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(pollInterval);
        }

        throw new TimeoutException("Condition was not met before the timeout elapsed.");
    }

    private static CookieContainer? ExtractCookieContainer(HttpClient httpClient)
    {
        if (!TryGetHandler(httpClient, out var handler))
        {
            return null;
        }

        return FindCookieContainer(handler);
    }

    private static bool TryGetHandler(HttpClient httpClient, out HttpMessageHandler handler)
    {
        var field = typeof(HttpMessageInvoker).GetField(
            "_handler",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (field?.GetValue(httpClient) is HttpMessageHandler resolvedHandler)
        {
            handler = resolvedHandler;
            return true;
        }

        handler = null!;
        return false;
    }

    private static CookieContainer? FindCookieContainer(HttpMessageHandler? handler) =>
        handler switch
        {
            HttpClientHandler httpClientHandler => httpClientHandler.CookieContainer,
            SocketsHttpHandler socketsHttpHandler => socketsHttpHandler.CookieContainer,
            DelegatingHandler delegatingHandler => FindCookieContainer(delegatingHandler.InnerHandler),
            _ => null,
        };
}
