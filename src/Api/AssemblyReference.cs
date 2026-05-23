using System.Reflection;

namespace TradingSimulator.Api;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
