var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password");

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume()
    .WithPgAdmin()
    .WithEndpoint(port: 5432, targetPort: 5432);

var tradingDb = postgres.AddDatabase("trading");

var redis = builder.AddRedis("cache")
    .WithRedisCommander()
    .WithEndpoint(port: 6379, targetPort: 6379);

var matchingEngine = builder.AddProject<Projects.TradingSimulator_MatchingEngine>("matching-engine")
    .WithReference(tradingDb)
    .WithReference(redis);

var api = builder.AddProject<Projects.TradingSimulator_Api>("api")
    .WithReference(tradingDb)
    .WithReference(redis)
    .WithReference(matchingEngine)
    .WithHttpEndpoint(port: 8001, targetPort: 8001)
    .WithHttpsEndpoint(port: 8000, targetPort: 8000)
    .WithExternalHttpEndpoints();

#pragma warning disable ASPIRECERTIFICATES001 // WithHttpsDeveloperCertificate
builder.AddViteApp("web", "../../web")
    .WithReference(api)
    .WithHttpsEndpoint(port: 5000, targetPort: 5000)
    .WithHttpsDeveloperCertificate()
    .WithExternalHttpEndpoints()
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("https"));
#pragma warning restore ASPIRECERTIFICATES001

builder.Build().Run();
