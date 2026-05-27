using System.Net;
using FluentAssertions;
using TradingSimulator.Testing.Common.Fixtures;
using TradingSimulator.Testing.Common.Integration;

namespace TradingSimulator.Api.IntegrationTests.Portfolios;

[Collection(IntegrationTestCollection.Name)]
public sealed class ResetPortfolioAuthTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ResetPortfolio_WithoutSession_Returns401_UNAUTHORIZED()
    {
        var client = fixture.Factory.CreateClient();
        var userId = await PortfolioResetTestHelpers.RegisterAndGetUserIdAsync(fixture, "reset_unauth");
        var beforeCount = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, userId);

        using var response = await client.PostAsync("/api/portfolio/reset", null);

        await PortfolioResetTestHelpers.AssertUnauthorizedAsync(response);

        var afterCount = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, userId);
        afterCount.Should().Be(beforeCount);
    }

    [Fact]
    public async Task GetResetEligibility_RequiresAuthentication()
    {
        var client = fixture.Factory.CreateClient();
        var userId = await PortfolioResetTestHelpers.RegisterAndGetUserIdAsync(fixture, "reset_elig_unauth");

        using var response = await client.GetAsync("/api/portfolio/reset/eligibility");

        await PortfolioResetTestHelpers.AssertUnauthorizedAsync(response);

        var resetCount = await PortfolioResetTestHelpers.CountPortfolioResetsAsync(fixture, userId);
        resetCount.Should().Be(0);
    }
}
