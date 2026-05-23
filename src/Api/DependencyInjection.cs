using TradingSimulator.Api.Hubs;
using TradingSimulator.Api.Middleware;
using TradingSimulator.Api.Realtime;
using TradingSimulator.Application.Abstractions.Realtime;

namespace TradingSimulator.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<IRealtimeNotificationPublisher, SignalRRealtimeNotificationPublisher>();
        services.AddSingleton<IRealtimeHubConnectionRegistry, InMemoryRealtimeHubConnectionRegistry>();

        return services;
    }

    public static WebApplication MapApiHubs(this WebApplication application)
    {
        application.MapHub<SimulationHub>("/hubs/simulation");
        return application;
    }

    public static IApplicationBuilder UseApiPipeline(this IApplicationBuilder application)
    {
        application.UseMiddleware<ExceptionHandlingMiddleware>();
        return application;
    }
}
