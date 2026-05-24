using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TradingSimulator.Api.IntegrationTests.Integration;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class RegisterUserTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient(
        new WebApplicationFactoryClientOptions { HandleCookies = true });

    [Fact]
    public async Task RegisterUser_Returns201_AndWalletShowsInitialCash()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterUserRequest(
            $"trader_{suffix}",
            $"trader_{suffix}@example.com",
            "SecurePass1!");

        using var registerResponse = await _client.PostAsJsonAsync("/api/users", request);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();
        registration!.Wallet.AvailableBalance.Should().Be(100_000m);
        registration.Wallet.Currency.Should().Be("USD");

        using var walletResponse = await _client.GetAsync("/api/wallet");

        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>();
        wallet.Should().NotBeNull();
        wallet!.UserId.Should().Be(registration.UserId);
        wallet.Username.Should().Be(registration.Username);
        wallet.Currency.Should().Be("USD");
        wallet.AvailableBalance.Should().Be(100_000m);
        wallet.TotalBalance.Should().Be(100_000m);
        wallet.ReservedBalance.Should().Be(0m);
    }
}
