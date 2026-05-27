using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TradingSimulator.Api.IntegrationTests.Integration;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollection.Name)]
public sealed class FoundationSmokeTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient(
        new WebApplicationFactoryClientOptions { HandleCookies = true });

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
