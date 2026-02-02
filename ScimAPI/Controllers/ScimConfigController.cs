using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScimAPI.Models;
using ScimAPI.Helpers;
using ScimAPI.Services;

namespace ScimAPI.Controllers
{
    [ApiController]
    [Route("scim")]
    [Produces("application/scim+json")]
    [Authorize]
    public class ScimConfigController(
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
        public IActionResult GetSchemas()
        {
            var schemas = new List<ScimSchema> 
            { 
                ScimSchemaGenerator.UserSchema, 
                ScimSchemaGenerator.GroupSchema 
            };
            return Ok(schemas);
        }

        [HttpGet("Schemas/{id}")]
        public IActionResult GetSchema(string id)
        {
            ScimSchema? schema = id switch
            {
                "urn:ietf:params:scim:schemas:core:2.0:User" => ScimSchemaGenerator.UserSchema,
                "urn:ietf:params:scim:schemas:core:2.0:Group" => ScimSchemaGenerator.GroupSchema,
                _ => null
            };

            if (schema == null)
                return NotFound(new ScimError { Detail = $"Schema {id} non trouvé", Status = 404 });

            return Ok(schema);
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

