using TradingSimulator.Api;
using TradingSimulator.Api.Endpoints;
using TradingSimulator.Application;
using TradingSimulator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseApiPipeline();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapEndpoints(TradingSimulator.Api.AssemblyReference.Assembly);
app.MapApiHubs();

app.Run();

public partial class Program;
