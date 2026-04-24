# Quick Fix: Unable to Resolve IJwtTokenService

If you're getting this error:

```
System.InvalidOperationException: Unable to resolve service for type 'EzSCIM.Services.IJwtTokenService' 
while attempting to activate 'EzSCIM.Controllers.ScimConfigController'.
```

## Solution

The JWT Token Service needs to be registered in your dependency injection container.

### Option 1: Use the Extension Method (Recommended)

In `Program.cs`:

```csharp
using EzSCIM.Services;

var builder = WebApplication.CreateBuilder(args);

// Add this line to register the JWT Token Service
builder.Services.AddJwtTokenService();

var app = builder.Build();
app.Run();
```

### Option 2: Register Manually

If you prefer to register manually:

```csharp
using EzSCIM.Services;

builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
```

### Option 3: With Custom Lifetime

```csharp
// Use Scoped lifetime instead of Singleton
builder.Services.AddJwtTokenService(ServiceLifetime.Scoped);
```

## Required Configuration

Ensure your `appsettings.json` has the JWT configuration:

```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-that-should-be-at-least-32-characters-long"
  }
}
```

## Verification

After making changes:

1. Rebuild the solution: `dotnet build`
2. Run the application: `dotnet run`
3. Test the endpoint: `http://localhost:5000/scim/ServiceProviderConfig`
   - Should return 401 Unauthorized (expected - no token provided)

## Complete Example

See `EzSCIM.EntraID.Demo/Program.cs` for a working example.

## For More Information

- Read: `EzSCIM/Services/JWT-SERVICE-EXTENSION-GUIDE.md`
- See: `EzSCIM.EntraID.AppHost/ASPIRE-ENTRAID-SCIM-GUIDE.md`

---

**Version**: 1.0  
**Last Updated**: February 20, 2026

