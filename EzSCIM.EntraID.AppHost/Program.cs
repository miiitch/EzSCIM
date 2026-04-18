using Aspire.Hosting.DevTunnels;


var builder = DistributedApplication.CreateBuilder(args);

// SQL Server container — spins up mcr.microsoft.com/mssql/server in dev
// WithDataVolume() persists data across restarts (flag 3 from plan)
// No data volume → ephemeral container: database is reset on every launch
var sql = builder.AddSqlServer("sql");

var scimDb = sql.AddDatabase("scimdb");

var api = builder.AddProject<Projects.EzSCIM_EntraID_Demo>("scimapi")
    .WithReference(scimDb)
    .WaitFor(scimDb)
    .WithExternalHttpEndpoints();

var tunnel = builder.AddDevTunnel("scim")
    .WithReference(api)
    .WithAnonymousAccess()
    .WithHttpsEndpoint();

builder.Build().Run();

