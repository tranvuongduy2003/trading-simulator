using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TradingSimulator.Api.IntegrationTests.Integration;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class RegisterUserSessionTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient(
        new WebApplicationFactoryClientOptions { HandleCookies = true });

    [Fact]
    public async Task GetWallet_WithoutSession_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/wallet");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
