using Xunit;

namespace TradingSimulator.Testing.Common.Integration;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection
{
    public const string Name = "Integration";
}
