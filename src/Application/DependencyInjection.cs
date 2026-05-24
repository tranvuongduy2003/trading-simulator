using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Services;
using TradingSimulator.Application.Behaviors;
using TradingSimulator.Application.Options;
using TradingSimulator.Application.Services;

namespace TradingSimulator.Application;

public static partial class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(AssemblyReference.Assembly));

        services.AddValidatorsFromAssembly(AssemblyReference.Assembly);
        services.AddDomainEventHandlers(AssemblyReference.Assembly);

        services.AddScoped<IPendingDomainEventsCollector, PendingDomainEventsCollector>();
        services.AddScoped<IPendingSessionCacheCollector, PendingSessionCacheCollector>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PostCommitSessionCacheBehavior<,>));

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddOptions<ConcurrencyOptions>()
            .BindConfiguration(ConcurrencyOptions.SectionName);

        services.AddOptions<ChannelPipelineOptions>()
            .BindConfiguration(ChannelPipelineOptions.SectionName);

        services.AddOptions<TradingOptions>()
            .BindConfiguration(TradingOptions.SectionName);

        services.AddOptions<TradingSessionOptions>()
            .BindConfiguration(TradingSessionOptions.SectionName);

        return services;
    }
}
