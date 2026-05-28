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
using TradingSimulator.Application.Abstractions.Realtime;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Portfolios;

[Collection(IntegrationTestCollection.Name)]
public sealed class ResetPortfolioNotificationsTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ResetPortfolio_WhenInFlight_SecondRequestReturns409()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"reset_inflight_{suffix}",
            $"reset_inflight_{suffix}@example.com",
            "SecurePass1!");

        await using var factory = fixture.CreateFactory(ConfigureDelayingPortfolioResetWriteRepository);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

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

        var problem = JsonSerializer.Deserialize<ApiProblemDetails>(responseBody, PortfolioResetTestHelpers.JsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(409);
        problem.Code.Should().Be("RESET_IN_PROGRESS");
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
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(PortfolioResetTestHelpers.JsonOptions);
        registration.Should().NotBeNull();

        await PortfolioResetTestHelpers.SeedWalletBalancesAsync(fixture, registration!.UserId, 100_000m, 2_000m);
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
            price: 201m,
            originalQuantity: 7,
            filledQuantity: 2,
            status: PortfolioResetTestHelpers.PartiallyFilledStatus);

        using var resetResponse = await client.PostAsync("/api/portfolio/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var capturingPublisher = factory.Services.GetRequiredService<CapturingRealtimeNotificationPublisher>();
        capturingPublisher.CancellationNotifications.Should().HaveCount(2);
        capturingPublisher.CancellationNotifications
            .Should()
            .OnlyContain(notification => notification.UserIdentifier == registration.UserId);
        capturingPublisher.OrderBookUpdates.Should().ContainSingle();
        capturingPublisher.OrderBookUpdates[0].Symbol.Should().Be("AAPL");
        capturingPublisher.OrderBookUpdates[0].Message.UpdatedAt.Should().NotBe(default);
        capturingPublisher.BalanceUpdates.Should().ContainSingle();
        capturingPublisher.BalanceUpdates[0].Message.AvailableCash.Should().Be(100_000m);
    }

    private static void ConfigureDelayingPortfolioResetWriteRepository(IServiceCollection services)
    {
        services.RemoveAll<IPortfolioResetWriteRepository>();
        services.AddScoped<IPortfolioResetWriteRepository, DelayingPortfolioResetWriteRepository>();
    }

    private static void ConfigureCapturingRealtimeNotificationPublisher(IServiceCollection services)
    {
        services.RemoveAll<IRealtimeNotificationPublisher>();
        services.AddSingleton<CapturingRealtimeNotificationPublisher>();
        services.AddSingleton<IRealtimeNotificationPublisher>(serviceProvider =>
            serviceProvider.GetRequiredService<CapturingRealtimeNotificationPublisher>());
    }
}
