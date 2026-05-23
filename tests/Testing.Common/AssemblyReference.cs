using System.Reflection;

namespace TradingSimulator.Testing.Common;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
