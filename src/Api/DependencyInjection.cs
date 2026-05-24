using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TradingSimulator.Api.Auth;
using TradingSimulator.Api.Hubs;
using TradingSimulator.Api.Middleware;
using TradingSimulator.Api.Options;
using TradingSimulator.Api.Realtime;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Realtime;

namespace TradingSimulator.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddOptions<CorsOptions>()
                .BindConfiguration(CorsOptions.SectionName);
            services.AddCors();
        }

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        services.AddAuthentication(SessionAuthenticationDefaults.Scheme)
            .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(
                SessionAuthenticationDefaults.Scheme,
                null);

        services.AddAuthorization();

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

    public static IApplicationBuilder UseApiPipeline(
        this IApplicationBuilder application,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            var allowedOrigins = application.ApplicationServices
                .GetRequiredService<IOptions<CorsOptions>>()
                .Value
                .AllowedOrigins
                .Where(IsHttpsOrigin)
                .ToArray();

            if (allowedOrigins.Length > 0)
            {
                application.UseCors(corsPolicyBuilder => corsPolicyBuilder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            }
        }

        application.UseMiddleware<ExceptionHandlingMiddleware>();
        application.UseMiddleware<InvalidRequestMiddleware>();

        application.UseAuthentication();
        application.UseAuthorization();

        return application;
    }

    private static bool IsHttpsOrigin(string origin) =>
        !string.IsNullOrWhiteSpace(origin)
        && Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
}
