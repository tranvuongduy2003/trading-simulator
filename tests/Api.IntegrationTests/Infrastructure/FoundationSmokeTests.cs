using System.Net;
using FluentAssertions;
using TradingSimulator.Api.IntegrationTests.Integration;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollection.Name)]
public sealed class FoundationSmokeTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var client = fixture.Factory.CreateClient();
        var response = await client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
