using System.Reflection;

namespace TradingSimulator.AppHost;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
