using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TradingSimulator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = configuration;

        // DbContext, repositories, Redis adapters, and channel wiring land here.
        return services;
    }
}
