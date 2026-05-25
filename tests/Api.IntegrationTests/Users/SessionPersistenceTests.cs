using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TradingSimulator.Api.IntegrationTests.Integration;
using TradingSimulator.Api.IntegrationTests.Users.Fakes;
using TradingSimulator.Application.Abstractions.Cache;
using TradingSimulator.Application.Abstractions.Services;
using TradingSimulator.Application.Options;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class SessionPersistenceTests(IntegrationTestFixture fixture)
{
    private static readonly DateTimeOffset SessionLifetimeTestStart =
        new(2026, 5, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SessionPersistence_AfterLogin_SecondWalletRequest_Returns200()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"persist_{suffix}@example.com";
        const string password = "SecurePass1!";

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, password));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResponse.Headers.Should().ContainKey("Set-Cookie");

        using var firstWalletResponse = await client.GetAsync("/api/wallet");
        firstWalletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstWallet = await firstWalletResponse.Content.ReadFromJsonAsync<WalletResponse>();
        firstWallet.Should().NotBeNull();
        firstWallet!.UserId.Should().Be(registration!.UserId);

        using var secondWalletResponse = await client.GetAsync("/api/wallet");
        secondWalletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondWallet = await secondWalletResponse.Content.ReadFromJsonAsync<WalletResponse>();
        secondWallet.Should().NotBeNull();
        secondWallet!.UserId.Should().Be(registration.UserId);
    }

    [Fact]
    public async Task SessionPersistence_WithinLifetime_Returns200()
    {
        var clock = new TestClock { UtcNow = SessionLifetimeTestStart };

        await using var factory = fixture.CreateFactory(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(clock);
            services.AddSingleton(clock);
        });

        var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"lifetime_{suffix}@example.com";
        const string password = "SecurePass1!";

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, password));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var walletAtLoginResponse = await client.GetAsync("/api/wallet");
        walletAtLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        clock.UtcNow = SessionLifetimeTestStart.AddHours(1);

        using var walletAfterOneHourResponse = await client.GetAsync("/api/wallet");
        walletAfterOneHourResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await walletAfterOneHourResponse.Content.ReadFromJsonAsync<WalletResponse>();
        wallet.Should().NotBeNull();
        wallet!.UserId.Should().Be(registration!.UserId);
    }

    [Fact]
    public async Task SessionPersistence_ExpiredSession_Returns401()
    {
        var clock = new TestClock { UtcNow = SessionLifetimeTestStart };

        await using var factory = fixture.CreateFactory(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(clock);
            services.AddSingleton(clock);
        });

        var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"expired_{suffix}@example.com";
        const string password = "SecurePass1!";

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, password));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var walletBeforeExpiryResponse = await client.GetAsync("/api/wallet");
        walletBeforeExpiryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var expirationHours = factory.Services
            .GetRequiredService<IOptions<TradingSessionOptions>>()
            .Value.ExpirationHours;

        clock.UtcNow = SessionLifetimeTestStart
            .AddHours(expirationHours)
            .AddMinutes(1);

        using var walletAfterExpiryResponse = await client.GetAsync("/api/wallet");
        walletAfterExpiryResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SessionPersistence_RevokedSession_Returns401()
    {
        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"revoked_{suffix}@example.com";
        const string password = "SecurePass1!";

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, password));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var walletBeforeRevokeResponse = await client.GetAsync("/api/wallet");
        walletBeforeRevokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var sessionId = await GetLatestSessionIdForEmailAsync(fixture.Factory, email);
        sessionId.Should().NotBe(Guid.Empty);

        var revokedAt = DateTimeOffset.UtcNow;
        await RevokeSessionAsync(fixture.Factory, sessionId, revokedAt);

        using var walletAfterRevokeResponse = await client.GetAsync("/api/wallet");
        walletAfterRevokeResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SessionPersistence_RedisCacheWriteFails_StillAuthenticatesViaPostgres()
    {
        await using var factory = fixture.CreateFactory(ConfigureThrowingCacheService);
        var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"redis_{suffix}@example.com";
        const string password = "SecurePass1!";

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, password));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var walletResponse = await client.GetAsync("/api/wallet");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>();
        wallet.Should().NotBeNull();
        wallet!.UserId.Should().Be(registration!.UserId);
    }

    private static void ConfigureThrowingCacheService(IServiceCollection services)
    {
        services.RemoveAll<ICacheService>();
        services.AddSingleton<ICacheService, ThrowingCacheService>();
    }

    private static async Task<Guid> GetLatestSessionIdForEmailAsync(
        IntegrationTestWebApplicationFactory factory,
        string email)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var sessionId = await (
            from session in databaseContext.UserSessions.AsNoTracking()
            join user in databaseContext.Users.AsNoTracking() on session.UserId equals user.Id
            where user.Email == email
            orderby session.CreatedAt descending
            select session.Id).FirstOrDefaultAsync();

        return sessionId;
    }

    private static async Task RevokeSessionAsync(
        IntegrationTestWebApplicationFactory factory,
        Guid sessionId,
        DateTimeOffset revokedAt)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        await databaseContext.UserSessions
            .Where(session => session.Id == sessionId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(session => session.RevokedAt, revokedAt));
    }
}
