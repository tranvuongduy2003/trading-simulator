using System.Reflection;

namespace TradingSimulator.MatchingEngine.UnitTests;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
