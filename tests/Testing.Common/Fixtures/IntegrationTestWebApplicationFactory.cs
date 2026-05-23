using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace TradingSimulator.Testing.Common.Fixtures;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _postgresConnectionString;
    private readonly string _redisConnectionString;

    public IntegrationTestWebApplicationFactory(
        string postgresConnectionString,
        string redisConnectionString)
    {
        _postgresConnectionString = postgresConnectionString;
        _redisConnectionString = redisConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.UseSetting("ConnectionStrings:trading", _postgresConnectionString);
        builder.UseSetting("ConnectionStrings:cache", _redisConnectionString);
    }
}
