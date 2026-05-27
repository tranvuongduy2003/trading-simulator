using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Contracts.Orders;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Infrastructure.Persistence.Entities;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Orders;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetMyOrderHistoryTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GetMyOrderHistory_AfterReset_ReturnsEmpty()
    {
        var registeredUser = await RegisterUserAsync("order_history_reset");
        await SeedTerminalOrderAsync(registeredUser.UserId, DateTimeOffset.UtcNow.AddMinutes(-5));

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(registeredUser.Email, registeredUser.Password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var historyResponse = await client.GetAsync("/api/orders/history");
        historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await historyResponse.Content.ReadFromJsonAsync<OrderHistoryResponse>();
        history.Should().NotBeNull();
        history!.PageNumber.Should().Be(1);
        history.PageSize.Should().Be(25);
        history.TotalCount.Should().Be(0);
        history.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyOrderHistory_AfterOtherUserReset_RemainsVisibleForCurrentUser()
    {
        var userA = await RegisterUserAsync("order_history_user_a");
        var userB = await RegisterUserAsync("order_history_user_b");

        await SeedTerminalOrderAsync(userA.UserId, DateTimeOffset.UtcNow.AddMinutes(-6));
        await SeedTerminalOrderAsync(userB.UserId, DateTimeOffset.UtcNow.AddMinutes(-5));

        var userAClient = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });
        using var userALoginResponse = await userAClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(userA.Email, userA.Password));
        userALoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var userAResetResponse = await userAClient.PostAsync("/api/portfolio/reset", null);
        userAResetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var userBClient = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });
        using var userBLoginResponse = await userBClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(userB.Email, userB.Password));
        userBLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var userBHistoryResponse = await userBClient.GetAsync("/api/orders/history");
        userBHistoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var userBHistory = await userBHistoryResponse.Content.ReadFromJsonAsync<OrderHistoryResponse>();
        userBHistory.Should().NotBeNull();
        userBHistory!.TotalCount.Should().Be(1);
        userBHistory.Items.Should().ContainSingle();
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

    private async Task SeedTerminalOrderAsync(Guid userId, DateTimeOffset createdAt)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var terminatedAt = createdAt.AddMinutes(1);
        await databaseContext.Orders.AddAsync(
            new OrderRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Symbol = "AAPL",
                Side = 0,
                Type = 0,
                Price = 155m,
                OriginalQuantity = 8,
                FilledQuantity = 8,
                Status = 2,
                IsSimulated = false,
                CreatedAt = createdAt,
                UpdatedAt = terminatedAt,
                TerminatedAt = terminatedAt,
            });

        await databaseContext.SaveChangesAsync();
    }
}
