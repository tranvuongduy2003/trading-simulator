using TradingSimulator.Application;
using TradingSimulator.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
