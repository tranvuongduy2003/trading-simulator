using TradingSimulator.Application;
using TradingSimulator.Infrastructure;
using TradingSimulator.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();

await host.ApplyMigrationsAsync();

host.Run();
