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
public sealed class GetMyOpenOrdersTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GetMyOpenOrders_WithoutSession_Returns401_UNAUTHORIZED()
    {
        var client = fixture.Factory.CreateClient();

        using var response = await client.GetAsync("/api/orders/open");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyOpenOrders_WithSession_ReturnsOnlyCurrentUser()
    {
        var userA = await RegisterUserAsync("open_orders_user_a");
        var userB = await RegisterUserAsync("open_orders_user_b");

        await SeedOpenOrderAsync(userA.UserId, 0, 10, DateTimeOffset.UtcNow.AddMinutes(-2));
        await SeedOpenOrderAsync(userB.UserId, 1, 11, DateTimeOffset.UtcNow.AddMinutes(-1));

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(userA.Email, userA.Password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var response = await client.GetAsync("/api/orders/open");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var openOrders = await response.Content.ReadFromJsonAsync<List<OpenOrderDto>>();
        openOrders.Should().NotBeNull();
        openOrders!.Should().HaveCount(1);
        openOrders[0].OriginalQuantity.Should().Be(10);
        openOrders[0].Side.Should().Be("Buy");
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

    private async Task SeedOpenOrderAsync(
        Guid userId,
        short side,
        long quantity,
        DateTimeOffset createdAt)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        await databaseContext.Orders.AddAsync(
            new OrderRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Symbol = "AAPL",
                Side = side,
                Type = 0,
                Price = 150m,
                OriginalQuantity = quantity,
                FilledQuantity = 0,
                Status = 0,
                IsSimulated = false,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            });

        await databaseContext.SaveChangesAsync();
    }
}
