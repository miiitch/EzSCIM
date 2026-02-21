# JWT Token Service Extension Guide

This guide explains the `AddJwtTokenService` extension method for registering JWT authentication in the dependency injection container.

## Overview

The `ServiceCollectionExtensions` class provides convenient extension methods for registering the JWT Token Service in your ASP.NET Core application.

## Location

```
EzSCIM/Services/ServiceCollectionExtensions.cs
```

## Extension Methods

### `AddJwtTokenService()`

Registers the JWT Token Service with default lifetime (Singleton).

```csharp
public static IServiceCollection AddJwtTokenService(this IServiceCollection services)
```

#### Parameters

- `services`: The `IServiceCollection` to register services into

#### Returns

- `IServiceCollection` for method chaining

#### Example

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register JWT Token Service with Singleton lifetime
builder.Services.AddJwtTokenService();

var app = builder.Build();
app.Run();
```

### `AddJwtTokenService(ServiceLifetime)`

Registers the JWT Token Service with a custom lifetime.

```csharp
public static IServiceCollection AddJwtTokenService(
    this IServiceCollection services, 
    ServiceLifetime lifetime)
```

#### Parameters

- `services`: The `IServiceCollection` to register services into
- `lifetime`: The service lifetime (Singleton, Scoped, or Transient)

#### Returns

- `IServiceCollection` for method chaining

#### Lifetime Options

| Lifetime | Behavior | Use Case |
|----------|----------|----------|
| **Singleton** | Created once, shared across all requests | Default - most efficient |
| **Scoped** | Created once per HTTP request | Thread-safe token generation per request |
| **Transient** | Created every time requested | Testing, custom token expiration |

#### Example: Scoped Lifetime

```csharp
builder.Services.AddJwtTokenService(ServiceLifetime.Scoped);
```

#### Example: Transient Lifetime

```csharp
builder.Services.AddJwtTokenService(ServiceLifetime.Transient);
```

## Service Interface

```csharp
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
```

## Usage in Controllers

Once registered, inject `IJwtTokenService` into your controllers:

```csharp
[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _tokenService;

    public AuthController(IJwtTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpGet("token")]
    public IActionResult GetToken()
    {
        // Generate a token valid for 2 hours
        var token = _tokenService.GenerateToken(expirationMinutes: 120);
        return Ok(new { token });
    }

    [HttpPost("validate")]
    public IActionResult ValidateToken([FromBody] string token)
    {
        var principal = _tokenService.ValidateToken(token);
        if (principal == null)
            return Unauthorized(new { message = "Invalid token" });

        return Ok(new { claims = principal.Claims });
    }
}
```

## Configuration

The JWT Token Service requires configuration in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-that-should-be-at-least-32-characters-long"
  }
}
```

### Secret Key Requirements

- **Minimum Length**: 32 characters (for HS256)
- **Security**: Use strong, random values
- **Production**: Store in Azure Key Vault

### Example Configuration

```json
{
  "Jwt": {
    "SecretKey": "bTrHxYz7K8vQwPmL2nJ5sD9fG4cR6vB1A8tUxWzZ",
    "ExpirationMinutes": 60
  },
  "AzureKeyVault": {
    "VaultUri": "https://your-keyvault.vault.azure.net/"
  }
}
```

## Complete Setup Example

Here's a complete setup in `Program.cs`:

```csharp
using EzSCIM.Services;
using EzSCIM.Authentication;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddJwtTokenService();

// Configure authentication
builder.Services.AddAuthentication()
    .AddScheme<JwtBearerTokenAuthenticationOptions, JwtBearerTokenAuthenticationHandler>(
        "Bearer", null);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Token Generation Example

```csharp
public class TokenService
{
    private readonly IJwtTokenService _jwtService;

    public TokenService(IJwtTokenService jwtService)
    {
        _jwtService = jwtService;
    }

    public string GenerateAccessToken()
    {
        // Default: 60 minutes expiration
        return _jwtService.GenerateToken();
    }

    public string GenerateRefreshToken()
    {
        // 7 days expiration
        return _jwtService.GenerateToken(expirationMinutes: 7 * 24 * 60);
    }

    public string GenerateShortLivedToken()
    {
        // 5 minutes expiration
        return _jwtService.GenerateToken(expirationMinutes: 5);
    }
}
```

## Token Validation Example

```csharp
public class AuthorizationService
{
    private readonly IJwtTokenService _jwtService;

    public AuthorizationService(IJwtTokenService jwtService)
    {
        _jwtService = jwtService;
    }

    public bool ValidateUserToken(string token)
    {
        var principal = _jwtService.ValidateToken(token);
        return principal != null;
    }

    public string? GetUserIdFromToken(string token)
    {
        var principal = _jwtService.ValidateToken(token);
        return principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }
}
```

## Testing

### Unit Test Example

```csharp
using Xunit;
using Microsoft.Extensions.Configuration;
using EzSCIM.Services;

public class JwtTokenServiceTests
{
    private IJwtTokenService _tokenService;

    public JwtTokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "super-secret-key-min-32-chars-long!" }
            })
            .Build();

        var logger = new Mock<ILogger<JwtTokenService>>().Object;
        _tokenService = new JwtTokenService(config, logger);
    }

    [Fact]
    public void GenerateToken_ReturnsValidToken()
    {
        // Act
        var token = _tokenService.GenerateToken(60);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void ValidateToken_AcceptsValidToken()
    {
        // Arrange
        var token = _tokenService.GenerateToken(60);

        // Act
        var principal = _tokenService.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
    }

    [Fact]
    public void ValidateToken_RejectsInvalidToken()
    {
        // Act
        var principal = _tokenService.ValidateToken("invalid.token.here");

        // Assert
        Assert.Null(principal);
    }
}
```

## Dependency Injection Container Registration

When you call `AddJwtTokenService()`, the following occurs:

```csharp
// Extension method does this:
services.Add(new ServiceDescriptor(
    typeof(IJwtTokenService),
    typeof(JwtTokenService),
    ServiceLifetime.Singleton));

// This allows:
var service = serviceProvider.GetRequiredService<IJwtTokenService>();
```

## Best Practices

1. **Always use interface injection**: Depend on `IJwtTokenService`, not the concrete class
2. **Use appropriate lifetime**: Singleton for stateless token generation
3. **Protect the secret key**: Never hardcode in source, use configuration
4. **Regenerate tokens**: Implement token refresh mechanism
5. **Log token operations**: Track validation failures for security
6. **Handle expiration**: Implement token refresh logic
7. **Use HTTPS**: Always use HTTPS in production
8. **Validate on every request**: Don't trust client claims

## Common Issues

### Issue: "Unable to resolve service for type 'IJwtTokenService'"

**Solution**: Ensure you called `AddJwtTokenService()` before building the app:

```csharp
builder.Services.AddJwtTokenService(); // ✅ Call this first
var app = builder.Build();
```

### Issue: "JWT secret key is too short"

**Solution**: Use at least 32 characters for the secret:

```json
{
  "Jwt": {
    "SecretKey": "use-at-least-32-characters-for-hs256"
  }
}
```

### Issue: Token validation always fails

**Solution**: Ensure the same secret is used for generation and validation:

- Check `appsettings.json` has correct secret
- Ensure no configuration overrides in `appsettings.Development.json`
- Restart application after changing secret

---

## See Also

- [JwtTokenService Implementation](./JwtTokenService.cs)
- [JWT Bearer Authentication Handler](../Authentication/)
- [SCIM Configuration](../Controllers/ScimConfigController.cs)

---

**Last Updated**: February 20, 2026  
**Version**: 1.0

