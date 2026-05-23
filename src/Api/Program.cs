using Scalar.AspNetCore;
using TradingSimulator.Api;
using TradingSimulator.Api.Endpoints;
using TradingSimulator.Application;
using TradingSimulator.Infrastructure;
using TradingSimulator.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Environment);

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.MapDefaultEndpoints();
app.UseApiPipeline(app.Environment);

app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapEndpoints(TradingSimulator.Api.AssemblyReference.Assembly);
app.MapApiHubs();

app.Run();

public partial class Program;
