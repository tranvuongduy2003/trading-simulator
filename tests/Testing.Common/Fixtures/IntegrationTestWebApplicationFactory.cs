using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TradingSimulator.Testing.Common.Fixtures;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _postgresConnectionString;
    private readonly string _redisConnectionString;
    private readonly Action<IServiceCollection>? _configureTestServices;

    public IntegrationTestWebApplicationFactory(
        string postgresConnectionString,
        string redisConnectionString,
        Action<IServiceCollection>? configureTestServices = null)
    {
        _postgresConnectionString = postgresConnectionString;
        _redisConnectionString = redisConnectionString;
        _configureTestServices = configureTestServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.UseSetting("ConnectionStrings:Trading", _postgresConnectionString);
        builder.UseSetting("ConnectionStrings:Cache", _redisConnectionString);

        if (_configureTestServices is not null)
        {
            builder.ConfigureServices(_configureTestServices);
        }
    }
}
