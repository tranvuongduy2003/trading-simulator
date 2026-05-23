using System.Reflection;

namespace TradingSimulator.Infrastructure;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
