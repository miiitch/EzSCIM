using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ScimAPI.Services;

namespace ScimAPI.Authentication
{
    /// <summary>
    /// Custom authentication handler for JWT Bearer Token
    /// </summary>
    public class JwtBearerTokenAuthenticationHandler(
        IOptionsMonitor<JwtBearerTokenAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IJwtTokenService jwtTokenService)
        : AuthenticationHandler<JwtBearerTokenAuthenticationOptions>(options, logger, encoder)
    {
        /// <summary>
        /// Handles JWT Bearer Token authentication
        /// </summary>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if Authorization header exists
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authorization header missing"));
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            
            // Verify Bearer token format
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header format"));
            }

            // Extract the token
            var token = authHeader.Substring("Bearer ".Length).Trim();

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Empty token"));
            }

            // Validate the JWT token
            var principal = jwtTokenService.ValidateToken(token);
            if (principal == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid or expired JWT token"));
            }

            // Create authentication ticket
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    /// <summary>
    /// Options for JWT Bearer Token authentication
    /// </summary>
    public class JwtBearerTokenAuthenticationOptions : AuthenticationSchemeOptions
    {
    }
}
