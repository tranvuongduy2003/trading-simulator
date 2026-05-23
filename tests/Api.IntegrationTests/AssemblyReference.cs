using System.Reflection;

namespace TradingSimulator.Api.IntegrationTests;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
