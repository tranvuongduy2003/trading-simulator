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
using TradingSimulator.Application.Abstractions.Cache;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class RegisterUserTransientFailureTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient(
        new WebApplicationFactoryClientOptions { HandleCookies = true });

    [Fact]
    public async Task RegisterUser_RetrySameCredentials_Returns422_NotSecondWallet()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var username = $"retry_{suffix}";
        var email = $"retry_{suffix}@example.com";
        var request = new RegisterUserRequest(username, email, "SecurePass1!");

        var walletCountBefore = await CountWalletsAsync();

        using (var first = await _client.PostAsJsonAsync("/api/users", request))
        {
            first.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        var walletCountAfterFirst = await CountWalletsAsync();
        walletCountAfterFirst.Should().Be(walletCountBefore + 1);

        using var retry = await _client.PostAsJsonAsync("/api/users", request);

        retry.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var problem = await retry.Content.ReadFromJsonAsync<ApiProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Code.Should().Be("USERNAME_TAKEN");

        var walletCountAfterRetry = await CountWalletsAsync();
        walletCountAfterRetry.Should().Be(walletCountAfterFirst);
    }

    [Fact]
    public async Task RegisterUser_WhenPersistenceFails_Returns500_INTERNAL_ERROR()
    {
        await using var factory = fixture.CreateFactory(ConfigureThrowOnAddUserRepository);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"fail_{suffix}",
            $"fail_{suffix}@example.com",
            "SecurePass1!");

        var countsBefore = await CountPersistenceRowsAsync(factory);

        using var response = await client.PostAsJsonAsync("/api/users", request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
        problem.Code.Should().Be("INTERNAL_ERROR");

        var countsAfter = await CountPersistenceRowsAsync(factory);
        countsAfter.Users.Should().Be(countsBefore.Users);
        countsAfter.Wallets.Should().Be(countsBefore.Wallets);
        countsAfter.Portfolios.Should().Be(countsBefore.Portfolios);
    }

    [Fact]
    public async Task RegisterUser_WhenRedisCacheWriteFails_StillReturns201_AndWalletWorks()
    {
        await using var factory = fixture.CreateFactory(ConfigureThrowingCacheService);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"redis_{suffix}",
            $"redis_{suffix}@example.com",
            "SecurePass1!");

        using var registerResponse = await client.PostAsJsonAsync("/api/users", request);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();
        registration!.Wallet.AvailableBalance.Should().Be(100_000m);

        using var walletResponse = await client.GetAsync("/api/wallet");

        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>();
        wallet.Should().NotBeNull();
        wallet!.UserId.Should().Be(registration.UserId);
        wallet.AvailableBalance.Should().Be(100_000m);
    }

    [Fact]
    public async Task RegisterUser_ParallelSameUsername_AtMostOneWallet()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var username = $"parallel_{suffix}";
        var request = new RegisterUserRequest(
            username,
            $"parallel_{suffix}@example.com",
            "SecurePass1!");

        var walletCountBefore = await CountWalletsAsync();

        using var clientOne = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });
        using var clientTwo = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { HandleCookies = true });

        var responses = await Task.WhenAll(
            clientOne.PostAsJsonAsync("/api/users", request),
            clientTwo.PostAsJsonAsync("/api/users", request));

        try
        {
            var createdCount = responses.Count(response => response.StatusCode == HttpStatusCode.Created);
            createdCount.Should().Be(1);

            var walletCountAfter = await CountWalletsAsync();
            walletCountAfter.Should().Be(walletCountBefore + 1);

            var nonCreated = responses.Where(response => response.StatusCode != HttpStatusCode.Created).ToArray();
            nonCreated.Should().ContainSingle();

            var secondStatus = nonCreated[0].StatusCode;
            secondStatus.Should().BeOneOf(
                HttpStatusCode.UnprocessableEntity,
                HttpStatusCode.InternalServerError);
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    private static void ConfigureThrowOnAddUserRepository(IServiceCollection services)
    {
        services.RemoveAll<IUserRepository>();
        services.AddScoped<IUserRepository, ThrowOnAddUserRepository>();
    }

    private static void ConfigureThrowingCacheService(IServiceCollection services)
    {
        services.RemoveAll<ICacheService>();
        services.AddSingleton<ICacheService, ThrowingCacheService>();
    }

    private async Task<int> CountWalletsAsync()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        return await databaseContext.Wallets.CountAsync();
    }

    private static async Task<(int Users, int Wallets, int Portfolios)> CountPersistenceRowsAsync(
        IntegrationTestWebApplicationFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        return (
            await databaseContext.Users.CountAsync(),
            await databaseContext.Wallets.CountAsync(),
            await databaseContext.Portfolios.CountAsync());
    }
}
