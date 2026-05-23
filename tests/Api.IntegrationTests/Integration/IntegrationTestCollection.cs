using TradingSimulator.Testing.Common.Fixtures;
using Xunit;

namespace TradingSimulator.Api.IntegrationTests.Integration;

[CollectionDefinition(IntegrationTestCollection.Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Integration";
}
