using Aspire.Hosting.DevTunnels;


var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.EzSCIM_EntraID_Demo>("scimapi")
    .WithExternalHttpEndpoints();

var tunnel = builder.AddDevTunnel("scim")
    .WithReference(api)
    .WithAnonymousAccess()
    .WithHttpsEndpoint();

builder.Build().Run();

