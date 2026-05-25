using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TradingSimulator.Api.Common;
using TradingSimulator.Api.IntegrationTests.Integration;
using TradingSimulator.Api.IntegrationTests.Users.Fakes;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Contracts.Portfolio;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class LoginUserTransientFailureTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task LoginUser_WhenSessionCreateFails_Returns500_INTERNAL_ERROR()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_fail_{suffix}@example.com";
        const string password = "SecurePass1!";

        using var registerClient = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await registerClient.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        await using var factory = fixture.CreateFactory(ConfigureThrowOnCreateSessionStore);
        var loginClient = factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        await using var scope = factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var sessionsForUserBefore = await databaseContext.UserSessions.CountAsync(
            session => session.UserId == registration!.UserId);

        using var loginResponse = await loginClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, password));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        loginResponse.Headers.Contains("Set-Cookie").Should().BeFalse(
            "failed login must not issue a session cookie");

        var problem = await loginResponse.Content.ReadFromJsonAsync<ApiProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
        problem.Code.Should().Be("INTERNAL_ERROR");

        var sessionsForUserAfter = await databaseContext.UserSessions.CountAsync(
            session => session.UserId == registration.UserId);

        sessionsForUserAfter.Should().Be(sessionsForUserBefore);
    }

    [Fact]
    public async Task LoginUser_ParallelSameCredentials_AtMostOneEffectiveAuth()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_parallel_{suffix}@example.com";
        const string password = "SecurePass1!";

        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginRequest = new LoginUserRequest(email, password);

        var responses = await Task.WhenAll(
            client.PostAsJsonAsync("/api/auth/login", loginRequest),
            client.PostAsJsonAsync("/api/auth/login", loginRequest));

        try
        {
            responses.Count(response => response.StatusCode == HttpStatusCode.OK)
                .Should()
                .BeGreaterThanOrEqualTo(1);

            using var walletResponse = await client.GetAsync("/api/wallet");

            walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>();
            wallet.Should().NotBeNull();
            wallet!.AvailableBalance.Should().Be(100_000m);
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    [Fact]
    public async Task LoginUser_AfterSuccess_WalletProbeWorks()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_probe_{suffix}@example.com";
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

        using var walletResponse = await client.GetAsync("/api/wallet");

        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>();
        wallet.Should().NotBeNull();
        wallet!.UserId.Should().Be(registration!.UserId);
        wallet.Username.Should().Be(registration.Username);
    }

    private static void ConfigureThrowOnCreateSessionStore(IServiceCollection services)
    {
        services.RemoveAll<ISessionStore>();
        services.AddScoped<ISessionStore, ThrowOnCreateSessionStore>();
    }
}
