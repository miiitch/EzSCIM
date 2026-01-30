﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScimAPI.Models;
using ScimAPI.Repositories;
using ScimAPI.Services;

namespace ScimAPI.Controllers
{
    [ApiController]
    [Route("scim")]
    [Produces("application/scim+json")]
    [Authorize]
    public class ScimConfigController(
        IScimRepository repository,
        ILogger<ScimConfigController> logger,
        IJwtTokenService jwtTokenService,
        IWebHostEnvironment environment)
        : ControllerBase
    {
        [HttpGet("ServiceProviderConfig")]
        public IActionResult GetServiceProviderConfig()
        {
            var config = new ScimServiceProviderConfig
            {
                DocumentationUri = "https://docs.microsoft.com/en-us/azure/active-directory/app-provisioning/use-scim-to-provision-users-and-groups",
                Patch = new ScimPatchConfig { Supported = true },
                Bulk = new ScimBulkConfig { Supported = false },
                Filter = new ScimFilterConfig { Supported = true, MaxResults = 200 },
                ChangePassword = new ScimChangePasswordConfig { Supported = false },
                Sort = new ScimSortConfig { Supported = false },
                Etag = new ScimEtagConfig { Supported = false },
                AuthenticationSchemes = new List<ScimAuthenticationScheme>
                {
                    new ScimAuthenticationScheme
                    {
                        Type = "oauthbearertoken",
                        Name = "OAuth Bearer Token",
                        Description = "Authentication via OAuth Bearer Token"
                    }
                }
            };

            return Ok(config);
        }

        [HttpGet("Schemas")]
        public async Task<IActionResult> GetSchemas()
        {
            var schemas = new List<ScimSchema> { GetUserSchema(), GetGroupSchema() };
            var customSchemas = await repository.GetCustomSchemasAsync();
            schemas.AddRange(customSchemas);
            return Ok(schemas);
        }

        [HttpGet("Schemas/{id}")]
        public async Task<IActionResult> GetSchema(string id)
        {
            ScimSchema? schema = id switch
            {
                "urn:ietf:params:scim:schemas:core:2.0:User" => GetUserSchema(),
                "urn:ietf:params:scim:schemas:core:2.0:Group" => GetGroupSchema(),
                _ => (await repository.GetCustomSchemasAsync()).FirstOrDefault(s => s.Id == id)
            };

            if (schema == null)
                return NotFound(new ScimError { Detail = $"Schema {id} non trouvé", Status = 404 });

            return Ok(schema);
        }

        [HttpPost("Schemas")]
        public async Task<IActionResult> AddCustomSchema([FromBody] ScimSchema schema)
        {
            try
            {
                if (string.IsNullOrEmpty(schema.Id) || !schema.Id.StartsWith("urn:"))
                    return BadRequest(new ScimError { Detail = "ID du schéma invalide", Status = 400 });

                await repository.AddCustomSchemaAsync(schema);
                return CreatedAtAction(nameof(GetSchema), new { id = schema.Id }, schema);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur AddCustomSchema");
                return StatusCode(500, new ScimError { Detail = "Erreur interne", Status = 500 });
            }
        }

        private ScimSchema GetUserSchema()
        {
            return new ScimSchema
            {
                Id = "urn:ietf:params:scim:schemas:core:2.0:User",
                Name = "User",
                Description = "User Account",
                Attributes = new List<ScimSchemaAttribute>
                {
                    new() { Name = "userName", Type = "string", Required = true, Uniqueness = "server" },
                    new() { Name = "name", Type = "complex", SubAttributes = new List<ScimSchemaAttribute>
                    {
                        new() { Name = "formatted", Type = "string" },
                        new() { Name = "familyName", Type = "string" },
                        new() { Name = "givenName", Type = "string" }
                    }},
                    new() { Name = "displayName", Type = "string" },
                    new() { Name = "active", Type = "boolean" },
                    new() { Name = "emails", Type = "complex", MultiValued = true },
                    new() { Name = "externalId", Type = "string" }
                }
            };
        }

        private ScimSchema GetGroupSchema()
        {
            return new ScimSchema
            {
                Id = "urn:ietf:params:scim:schemas:core:2.0:Group",
                Name = "Group",
                Description = "Group",
                Attributes = new List<ScimSchemaAttribute>
                {
                    new() { Name = "displayName", Type = "string", Required = true },
                    new() { Name = "members", Type = "complex", MultiValued = true },
                    new() { Name = "externalId", Type = "string" }
                }
            };
        }

        /// <summary>
        /// Endpoint de test pour générer un token JWT (développement uniquement)
        /// </summary>
        [HttpGet("auth/token")]
        [AllowAnonymous]
        public IActionResult GetAuthToken()
        {
            if (!environment.IsDevelopment())
            {
                return Forbid("Cet endpoint n'est accessible qu'en développement");
            }

            try
            {
                var token = jwtTokenService.GenerateToken();
                return Ok(new { token, expiresIn = "60 minutes" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la génération du token");
                return StatusCode(500, new ScimError { Detail = "Erreur lors de la génération du token", Status = 500 });
            }
        }
    }
}

