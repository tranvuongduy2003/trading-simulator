using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Api.Common;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Infrastructure.Persistence.Entities;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Portfolios;

internal static class PortfolioResetTestHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public const short PendingStatus = 0;
    public const short PartiallyFilledStatus = 1;
    public const short CancelledStatus = 3;

    public static async Task SeedWalletBalancesAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        decimal totalBalance,
        decimal reservedBalance,
        CancellationToken cancellationToken = default)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var rowsUpdated = await databaseContext.Wallets
            .Where(walletRecord => walletRecord.UserId == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(walletRecord => walletRecord.TotalBalance, totalBalance)
                    .SetProperty(walletRecord => walletRecord.ReservedBalance, reservedBalance),
                cancellationToken);

        rowsUpdated.Should().Be(1);
    }

    public static async Task SeedHoldingAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        string symbol,
        long quantity,
        decimal averagePrice,
        long reservedQuantity = 0,
        CancellationToken cancellationToken = default)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var portfolioId = await databaseContext.Portfolios
            .AsNoTracking()
            .Where(portfolioRecord => portfolioRecord.UserId == userId)
            .Select(portfolioRecord => portfolioRecord.Id)
            .SingleAsync(cancellationToken);

        await databaseContext.Holdings.AddAsync(
            new HoldingRecord
            {
                PortfolioId = portfolioId,
                Symbol = symbol,
                TotalQuantity = quantity,
                ReservedQuantity = reservedQuantity,
                AveragePrice = averagePrice,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            cancellationToken);

        await databaseContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task<int> CountPortfolioResetsAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        return await databaseContext.PortfolioResets
            .CountAsync(portfolioReset => portfolioReset.UserId == userId, cancellationToken);
    }

    public static async Task SeedPortfolioResetAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        DateTimeOffset resetAt,
        CancellationToken cancellationToken = default)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        await databaseContext.PortfolioResets.AddAsync(
            new PortfolioResetRecord
            {
                UserId = userId,
                ResetAt = resetAt,
            },
            cancellationToken);

        await databaseContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task<Guid> SeedOpenOrderAsync(
        IntegrationTestFixture fixture,
        Guid userId,
        short side,
        short type,
        decimal? price,
        long originalQuantity,
        long filledQuantity,
        short status,
        string symbol = "AAPL",
        CancellationToken cancellationToken = default)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var now = DateTimeOffset.UtcNow;
        var orderId = Guid.NewGuid();

        await databaseContext.Orders.AddAsync(
            new OrderRecord
            {
                Id = orderId,
                UserId = userId,
                Symbol = symbol,
                Side = side,
                Type = type,
                Price = price,
                OriginalQuantity = originalQuantity,
                FilledQuantity = filledQuantity,
                Status = status,
                IsSimulated = false,
                CreatedAt = now,
                UpdatedAt = now,
            },
            cancellationToken);

        await databaseContext.SaveChangesAsync(cancellationToken);
        return orderId;
    }

    public static async Task<Guid> RegisterAndGetUserIdAsync(IntegrationTestFixture fixture, string usernamePrefix)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"{usernamePrefix}_{suffix}",
            $"{usernamePrefix}_{suffix}@example.com",
            "SecurePass1!");

        var sessionClient = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await sessionClient.PostAsJsonAsync("/api/users", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>(JsonOptions);
        registration.Should().NotBeNull();
        return registration!.UserId;
    }

    public static async Task AssertUnauthorizedAsync(HttpResponseMessage response)
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

    public static DateTimeOffset ParseNextEligibleAt(ApiProblemDetails problem)
    {
        problem.Extensions.Should().ContainKey("nextEligibleAt");
        var nextEligibleAtRaw = problem.Extensions["nextEligibleAt"]?.ToString();
        nextEligibleAtRaw.Should().NotBeNullOrWhiteSpace();
        return DateTimeOffset.Parse(nextEligibleAtRaw!, CultureInfo.InvariantCulture);
    }
}
