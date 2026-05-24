using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TradingSimulator.Api.IntegrationTests.Users;

public class RegisterUserSessionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RegisterUserSessionTests(WebApplicationFactory<Program> factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetWallet_WithoutSession_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/wallet");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
