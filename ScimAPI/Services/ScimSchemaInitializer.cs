using ScimAPI.Models;
using ScimAPI.Repositories;
using System.Text.Json;

namespace ScimAPI.Services
{
    public class ScimSchemaInitializer(
        IScimRepository repository,
        ILogger<ScimSchemaInitializer> logger,
        IConfiguration configuration)
        : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Initialisation des schémas SCIM customs...");

            try
            {
                var customSchemas = configuration.GetSection("CustomSchemas").Get<List<ScimSchema>>();

                if (customSchemas != null && customSchemas.Any())
                {
                    foreach (var schema in customSchemas)
                    {
                        await repository.AddCustomSchemaAsync(schema);
                        logger.LogInformation("Schéma custom chargé: {SchemaId}", schema.Id);
                    }
                }

                if (configuration.GetValue("Scim:LoadTestData", false))
                {
                    await LoadTestDataAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de l'initialisation des schémas SCIM");
            }

            return;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task LoadTestDataAsync()
        {
            logger.LogInformation("Chargement des données de test...");

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
            logger.LogInformation("Utilisateur de test créé");

            var testGroup = new ScimGroup
            {
                DisplayName = "Test Group",
                ExternalId = "test-group-001"
            };

            await repository.CreateGroupAsync(testGroup);
            logger.LogInformation("Groupe de test créé");
        }
    }
}

