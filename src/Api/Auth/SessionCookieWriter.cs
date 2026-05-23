using Microsoft.Extensions.Options;
using TradingSimulator.Application.Options;

namespace TradingSimulator.Api.Auth;

internal static class SessionCookieWriter
{
    public static void Append(
        HttpContext httpContext,
        Guid sessionId,
        DateTimeOffset expiresAt,
        TradingSessionOptions sessionOptions)
    {
        var maxAge = expiresAt - DateTimeOffset.UtcNow;
        if (maxAge <= TimeSpan.Zero)
        {
            maxAge = TimeSpan.FromHours(sessionOptions.ExpirationHours);
        }

        httpContext.Response.Cookies.Append(
            sessionOptions.CookieName,
            sessionId.ToString("D"),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                MaxAge = maxAge,
                Path = "/",
            });
    }
}
