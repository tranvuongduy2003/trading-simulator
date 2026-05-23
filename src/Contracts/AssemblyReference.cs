using System.Reflection;

namespace TradingSimulator.Contracts;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
