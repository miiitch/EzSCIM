using ScimAPI.Repositories;
using ScimAPI.Services;
using ScimAPI.Authentication;
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
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Register schema initialization service
builder.Services.AddHostedService<ScimSchemaInitializer>();

// Configure JWT authentication
builder.Services.AddAuthentication()
    .AddScheme<JwtBearerTokenAuthenticationOptions, JwtBearerTokenAuthenticationHandler>(
        "Bearer", null);

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON for SCIM (camelCase by default)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

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

