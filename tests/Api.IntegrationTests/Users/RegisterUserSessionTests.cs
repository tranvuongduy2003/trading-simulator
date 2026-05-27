using System.Net;
using FluentAssertions;
using TradingSimulator.Api.IntegrationTests.Integration;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class RegisterUserSessionTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GetWallet_WithoutSession_ReturnsUnauthorized()
    {
        var client = fixture.Factory.CreateClient();
        var response = await client.GetAsync("/api/wallet");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
