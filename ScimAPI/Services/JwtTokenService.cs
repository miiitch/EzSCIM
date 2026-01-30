using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ScimAPI.Services
{
    /// <summary>
    /// Service for JWT token validation and generation for SCIM authentication
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Validates a JWT token and returns the claims if valid
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Generates a new JWT token with expiration duration
        /// </summary>
        string GenerateToken(int expirationMinutes = 60);
    }

    /// <summary>
    /// Implementation of JWT token service
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtTokenService> _logger;
        private readonly byte[] _secretKeyBytes;

        public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT:SecretKey configuration is missing");
            }

            // Ensure secret key is at least 32 characters for HS256
            if (secretKey.Length < 32)
            {
                _logger.LogWarning("JWT secret key is too short ({Length} characters). Minimum 32 required for HS256", secretKey.Length);
            }

            _secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
        }

        /// <summary>
        /// Validates a JWT token using HS256 signature verification
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(_secretKeyBytes);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating JWT token");
                return null;
            }
        }

        /// <summary>
        /// Generates a new JWT token with minimal claims (sub, jti, exp)
        /// </summary>
        public string GenerateToken(int expirationMinutes = 60)
        {
            var key = new SymmetricSecurityKey(_secretKeyBytes);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Minimal claims: subject and JWT ID
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "scim-client"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}
