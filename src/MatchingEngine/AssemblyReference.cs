using System.Reflection;

namespace TradingSimulator.MatchingEngine;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
