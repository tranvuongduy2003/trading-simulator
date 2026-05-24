using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using TradingSimulator.Infrastructure.Persistence;
using Xunit;

namespace TradingSimulator.Testing.Common.Fixtures;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine")
        .Build();

    public IntegrationTestWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync());

        Factory = new IntegrationTestWebApplicationFactory(
            _postgresContainer.GetConnectionString(),
            _redisContainer.GetConnectionString());

        await ApplyMigrationsAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    private async Task ApplyMigrationsAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDatabaseContext>()
            .UseNpgsql(
                _postgresContainer.GetConnectionString(),
                npgsql => npgsql.MigrationsHistoryTable(
                    "__ef_migrations_history",
                    ApplicationDatabaseContext.SchemaName))
            .Options;

        await using var databaseContext = new ApplicationDatabaseContext(options);
        await databaseContext.Database.MigrateAsync();
    }
}
