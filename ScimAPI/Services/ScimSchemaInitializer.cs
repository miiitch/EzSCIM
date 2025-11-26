using ScimAPI.Models;
using ScimAPI.Repositories;
using System.Text.Json;

namespace ScimAPI.Services
{
    public class ScimSchemaInitializer : IHostedService
    {
        private readonly IScimRepository _repository;
        private readonly ILogger<ScimSchemaInitializer> _logger;
        private readonly IConfiguration _configuration;

        public ScimSchemaInitializer(IScimRepository repository, ILogger<ScimSchemaInitializer> logger, IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initialisation des schémas SCIM customs...");

            try
            {
                var customSchemas = _configuration.GetSection("CustomSchemas").Get<List<ScimSchema>>();

                if (customSchemas != null && customSchemas.Any())
                {
                    foreach (var schema in customSchemas)
                    {
                        await _repository.AddCustomSchemaAsync(schema);
                        _logger.LogInformation("Schéma custom chargé: {SchemaId}", schema.Id);
                    }
                }

                if (_configuration.GetValue("Scim:LoadTestData", false))
                {
                    await LoadTestDataAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'initialisation des schémas SCIM");
            }

            return;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task LoadTestDataAsync()
        {
            _logger.LogInformation("Chargement des données de test...");

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

            await _repository.CreateUserAsync(testUser);
            _logger.LogInformation("Utilisateur de test créé");

            var testGroup = new ScimGroup
            {
                DisplayName = "Test Group",
                ExternalId = "test-group-001"
            };

            await _repository.CreateGroupAsync(testGroup);
            _logger.LogInformation("Groupe de test créé");
        }
    }
}

