using TradingSimulator.Application;
using TradingSimulator.Infrastructure;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.MatchingEngine;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMatchingEngineServices();

var host = builder.Build();

await host.ApplyMigrationsAsync();

host.Run();
