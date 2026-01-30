using ScimAPI.Repositories;
using ScimAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Ajouter la configuration des schémas SCIM customs
builder.Configuration.AddJsonFile("appsettings.Scim.json", optional: true, reloadOnChange: true);

// Add services to the container.

// Enregistrer le repository SCIM en tant que Singleton pour l'implémentation en mémoire
builder.Services.AddSingleton<IScimRepository, InMemoryScimRepository>();

// Enregistrer le service d'initialisation des schémas
builder.Services.AddHostedService<ScimSchemaInitializer>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configuration JSON pour SCIM (camelCase par défaut)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();