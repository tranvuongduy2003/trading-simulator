using System.Reflection;

namespace TradingSimulator.ServiceDefaults;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
