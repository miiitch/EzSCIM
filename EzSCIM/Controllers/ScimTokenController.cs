using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EzSCIM.Models;
using EzSCIM.Services;

namespace EzSCIM.Controllers
{
    [ApiController]
    [Route("scim")]
    [Produces("application/scim+json")]
    [Authorize]
    public class ScimTokenController : ControllerBase
    {
        private readonly IJwtTokenService _tokenService;
        private readonly IWebHostEnvironment _environment;
        private readonly ITokenEndpointFeature _tokenEndpointFeature;

        public ScimTokenController(
            IJwtTokenService tokenService,
            IWebHostEnvironment environment,
            ITokenEndpointFeature tokenEndpointFeature)
        {
            _tokenService = tokenService;
            _environment = environment;
            _tokenEndpointFeature = tokenEndpointFeature;
        }

        /// <summary>
        /// Generate a JWT Bearer Token for SCIM API testing.
        /// Only available in development when enabled by service registration.
        /// </summary>
        /// <returns>JWT token and expiration information.</returns>
        [AllowAnonymous]
        [HttpGet("auth/token")]
        public IActionResult GenerateToken()
        {
            if (!_tokenEndpointFeature.IsEnabled)
            {
                return StatusCode(403, new ScimError
                {
                    Detail = "Token generation is disabled by service registration",
                    Status = 403
                });
            }

            if (!_environment.IsDevelopment())
            {
                return StatusCode(403, new ScimError
                {
                    Detail = "Token generation is only available in development environment",
                    Status = 403
                });
            }

            try
            {
                var token = _tokenService.GenerateToken(60);
                return Ok(new
                {
                    token = token,
                    expiresIn = "60 minutes",
                    tokenType = "Bearer"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ScimError
                {
                    Detail = $"Error generating token: {ex.Message}",
                    Status = 500
                });
            }
        }
    }
}
