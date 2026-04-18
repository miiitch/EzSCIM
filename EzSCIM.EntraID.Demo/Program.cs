using EzSCIM.DataRepositories;
using EzSCIM.EntraID.Demo.Data;
using EzSCIM.EntraID.Demo.Data.Entities;
using EzSCIM.EntraID.Demo.Data.Repositories;
using EzSCIM.Filtering;
using EzSCIM.Repositories;
using EzSCIM.Services;
using EzSCIM.Authentication;
using EzSCIM.Controllers;
using EzSCIM.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add custom SCIM schema configuration
builder.Configuration.AddJsonFile("appsettings.Scim.json", optional: true, reloadOnChange: true);

// Load Azure Key Vault in production
if (!builder.Environment.IsDevelopment())
{
    try
    {
        var keyVaultUrl = builder.Configuration["AzureKeyVault:VaultUri"];
        if (!string.IsNullOrEmpty(keyVaultUrl))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUrl),
                new DefaultAzureCredential());
        }
    }
    catch (Exception ex)
    {
        var logger = builder.Services.BuildServiceProvider().GetService<ILogger<Program>>();
        logger?.LogWarning(ex, "Unable to load Azure Key Vault");
    }
}

// Add services to the container

// Register SQL Server DbContext via Aspire integration (connection name = "scimdb")
// In dev: SQL Server container orchestrated by Aspire AppHost
// In prod: Azure SQL connection string injected via config/Key Vault
builder.AddSqlServerDbContext<DemoScimDbContext>("scimdb");

// Data repository (EF CRUD)
builder.Services.AddScoped<IUserGroupDataRepository<DemoUserEntity, DemoGroupEntity>, DemoUserGroupRepository>();

// Filter translators (server-side SCIM filter → LINQ)
builder.Services.AddScoped<IScimFilterTranslator<DemoUserEntity>, GenericScimFilterTranslator<DemoUserEntity>>();
builder.Services.AddScoped<IScimFilterTranslator<DemoGroupEntity>, GenericScimFilterTranslator<DemoGroupEntity>>();

// SCIM repository — delegates to EF + entity extensions for proper JSON multi-value handling
builder.Services.AddScoped<IScimRepository, DemoScimRepository>();

// Register JWT token service
builder.Services.AddJwtTokenService();

// Configure JWT authentication
builder.Services.AddAuthentication()
    .AddScheme<JwtBearerTokenAuthenticationOptions, JwtBearerTokenAuthenticationHandler>(
        "Bearer", null);

builder.Services.AddAuthorization();

// Add SCIM controllers with default configuration (scim/Users, scim/Groups)
builder.Services.AddScimControllers();

// Enable token generation endpoint in the demo app
builder.Services.AddScimTokenGeneratorEndpoint();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply EF migrations on startup (creates schema on first run, idempotent afterwards)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoScimDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations");
        throw;
    }
}

// Load test data if configured
if (app.Configuration.GetValue("Scim:LoadTestData", false))
{
    using var scope = app.Services.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<IScimRepository>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Loading test data...");
    
    var testUser = new ScimUser
    {
        UserName = "test.user@example.com",
        ExternalId = "test-001",
        Name = new ScimName
        {
            GivenName = "Test",
            FamilyName = "User",
            Formatted = "Test User"
        },
        DisplayName = "Test User",
        Active = true,
        Emails = new List<ScimEmail>
        {
            new() { Value = "test.user@example.com", Type = "work", Primary = true }
        }
    };
    
    await repository.CreateUserAsync(testUser);
    logger.LogInformation("Test user created successfully");
    
    var testGroup = new ScimGroup
    {
        DisplayName = "Test Group",
        ExternalId = "test-group-001"
    };
    
    await repository.CreateGroupAsync(testGroup);
    logger.LogInformation("Test group created successfully");
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

