using EzSCIM.Repositories;
using EzSCIM.Services;
using EzSCIM.Authentication;
using EzSCIM.Controllers;
using EzSCIM.Models;
using Microsoft.AspNetCore.Authentication;
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

// Register SCIM repository as Singleton for in-memory implementation
builder.Services.AddSingleton<IScimRepository, InMemoryScimRepository>();

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

