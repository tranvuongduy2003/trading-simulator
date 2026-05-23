using System.Reflection;

namespace TradingSimulator.Domain.UnitTests;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
