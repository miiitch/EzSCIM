var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ScimAPI>("scimapi")
    .WithExternalHttpEndpoints();

builder.Build().Run();
