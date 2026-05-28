using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TradingSimulator.Api.Common;
using TradingSimulator.Api.Hubs;
using TradingSimulator.Api.IntegrationTests.Portfolios;
using TradingSimulator.Contracts.Realtime;
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

    public static Uri BuildSimulationHubUri(IntegrationTestWebApplicationFactory factory) =>
        new(factory.Server.BaseAddress!, "/hubs/simulation");

    public static async Task<HubConnection> ConnectSimulationHubAsync(
        IntegrationTestWebApplicationFactory factory,
        HttpMessageHandler handler,
        Action<OrderBookUpdatedMessage> onOrderBookUpdated)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(BuildSimulationHubUri(factory), options =>
            {
                options.HttpMessageHandlerFactory = _ => handler;
            })
            .Build();

        connection.On<OrderBookUpdatedMessage>(
            nameof(ISimulationHubClient.OrderBookUpdated),
            onOrderBookUpdated);

        await connection.StartAsync();
        await connection.InvokeAsync(nameof(SimulationHub.SubscribeToMarket), "AAPL");

        return connection;
    }

    public static async Task<OrderBookUpdatedMessage> WaitForOrderBookUpdateAsync(
        IReadOnlyCollection<OrderBookUpdatedMessage> messages,
        Func<OrderBookUpdatedMessage, bool> predicate,
        TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var match = messages.LastOrDefault(predicate);
            if (match is not null)
            {
                return match;
            }

            await Task.Delay(50);
        }

        throw new TimeoutException("Timed out waiting for OrderBookUpdated hub message.");
    }

    public static async Task CleanupAaplOrderBookAsync(IntegrationTestFixture fixture)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var multiplexer = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        await multiplexer.GetDatabase().KeyDeleteAsync("orderbook:AAPL:snapshot");
    }

    public static async Task ResetUserPortfolioAsync(HttpClient client)
    {
        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
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
