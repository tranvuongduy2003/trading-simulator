using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.MatchingEngine;

public static class DependencyInjection
{
    public static IServiceCollection AddMatchingEngineServices(this IServiceCollection services)
    {
        services.AddSingleton<ICurrentUserAccessor, UnauthenticatedCurrentUserAccessor>();
        return services;
    }

    private sealed class UnauthenticatedCurrentUserAccessor : ICurrentUserAccessor
    {
        public UserId? UserId => null;

        public bool IsAuthenticated => false;
    }
}
