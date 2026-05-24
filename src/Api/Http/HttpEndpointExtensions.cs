using TradingSimulator.Api.Http.Filters;

namespace TradingSimulator.Api.Http;

internal static class HttpEndpointExtensions
{
    public static RouteHandlerBuilder RequireCompleteJsonBody<TBody>(this RouteHandlerBuilder builder)
        where TBody : class =>
        builder.AddEndpointFilter<CompleteJsonBodyEndpointFilter<TBody>>();
}
