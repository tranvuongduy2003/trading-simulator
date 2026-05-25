using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TradingSimulator.Api.Common;
using TradingSimulator.Api.IntegrationTests.Integration;
using TradingSimulator.Api.IntegrationTests.Users.Fakes;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetMyWalletTests(IntegrationTestFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task GetMyWallet_WithoutSession_Returns401()
    {
        var client = fixture.Factory.CreateClient();

        using var walletResponse = await client.GetAsync("/api/wallet");

        walletResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyWallet_AfterRegister_Returns100kBalances()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"wallet_{suffix}",
            $"wallet_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        using var walletResponse = await client.GetAsync("/api/wallet");

        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>();
        wallet.Should().NotBeNull();
        wallet!.UserId.Should().Be(registration!.UserId);
        wallet.Username.Should().Be(registration.Username);
        wallet.Currency.Should().Be("USD");
        wallet.TotalBalance.Should().Be(100_000m);
        wallet.ReservedBalance.Should().Be(0m);
        wallet.AvailableBalance.Should().Be(100_000m);
        wallet.AvailableBalance.Should().Be(wallet.TotalBalance - wallet.ReservedBalance);
    }

    [Fact]
    public async Task GetMyWallet_WithSeededReserved_ReturnsCorrectBreakdown()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"wallet_reserved_{suffix}",
            $"wallet_reserved_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await SeedWalletBalancesAsync(
            registration!.UserId,
            totalBalance: 50_000m,
            reservedBalance: 10_000m);

        using var walletResponse = await client.GetAsync("/api/wallet");

        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>();
        wallet.Should().NotBeNull();
        wallet!.TotalBalance.Should().Be(50_000m);
        wallet.ReservedBalance.Should().Be(10_000m);
        wallet.AvailableBalance.Should().Be(40_000m);
        wallet.AvailableBalance.Should().Be(wallet.TotalBalance - wallet.ReservedBalance);
    }

    [Fact]
    public async Task GetMyWallet_WithSeeded5kReserved_ReturnsEc02Breakdown()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"wallet_ec02_{suffix}",
            $"wallet_ec02_{suffix}@example.com",
            "SecurePass1!");

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await SeedWalletBalancesAsync(
            registration!.UserId,
            totalBalance: 100_000m,
            reservedBalance: 5_000m);

        using var walletResponse = await client.GetAsync("/api/wallet");

        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>();
        wallet.Should().NotBeNull();
        wallet!.TotalBalance.Should().Be(100_000m);
        wallet.ReservedBalance.Should().Be(5_000m);
        wallet.AvailableBalance.Should().Be(95_000m);
        wallet.AvailableBalance.Should().Be(wallet.TotalBalance - wallet.ReservedBalance);
    }

    [Fact]
    public async Task GetMyWallet_WhenReadFails_Returns500_INTERNAL_ERROR()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"wallet_fail_{suffix}@example.com";
        const string password = "SecurePass1!";
        var username = $"wallet_fail_{suffix}";

        using var registerClient = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await registerClient.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest(username, email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var factory = fixture.CreateFactory(ConfigureThrowOnWalletReadRepository);
        var walletClient = factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var loginResponse = await walletClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, password));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var walletResponse = await walletClient.GetAsync("/api/wallet");

        walletResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var body = await walletResponse.Content.ReadAsStringAsync();
        body.Should().NotContain("availableBalance");

        var problem = JsonSerializer.Deserialize<ApiProblemDetails>(body, JsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
        problem.Code.Should().Be("INTERNAL_ERROR");
    }

    private static void ConfigureThrowOnWalletReadRepository(IServiceCollection services)
    {
        services.RemoveAll<IWalletReadRepository>();
        services.AddScoped<IWalletReadRepository, ThrowOnWalletReadRepository>();
    }

    private async Task SeedWalletBalancesAsync(
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

        var persisted = await databaseContext.Wallets
            .AsNoTracking()
            .SingleAsync(walletRecord => walletRecord.UserId == userId, cancellationToken);

        persisted.TotalBalance.Should().Be(totalBalance);
        persisted.ReservedBalance.Should().Be(reservedBalance);
    }
}
