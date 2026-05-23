using System.Reflection;

namespace TradingSimulator.Application;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
