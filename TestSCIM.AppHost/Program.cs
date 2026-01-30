using Aspire.Hosting.DevTunnels;


var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.ScimAPI>("scimapi")
    .WithExternalHttpEndpoints();

var tunnel = builder.AddDevTunnel("scim")
    .WithReference(api)
    .WithAnonymousAccess()
    .WithHttpsEndpoint();

builder.Build().Run();

