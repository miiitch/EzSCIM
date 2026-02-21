# 🎯 Resolution Guide - JWT Service Dependency Injection

## Problem Statement

```
Bearer was not authenticated. Failure message: Authorization header missing
System.InvalidOperationException: Unable to resolve service for type 
'EzSCIM.Services.IJwtTokenService' while attempting to activate 
'EzSCIM.Controllers.ScimConfigController'.
```

## Root Cause Analysis

The JWT Token Service interface (`IJwtTokenService`) is not registered in the dependency injection container of your ASP.NET Core application.

## ✅ Solution

### Step 1: Add the Extension (Recommended)

Update `Program.cs` to include:

```csharp
using EzSCIM.Services;  // Add this using statement

var builder = WebApplication.CreateBuilder(args);

// ... other service registrations ...

// Add this line ↓
builder.Services.AddJwtTokenService();

// ... rest of configuration ...
```

### Step 2: Verify Configuration

Ensure `appsettings.json` contains:

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-minimum-32-characters-long"
  }
}
```

### Step 3: Rebuild and Test

```powershell
# Clean build
dotnet clean
dotnet build

# Run the application
dotnet run
```

### Step 4: Verify Resolution

Test the endpoint:

```powershell
# Should return 401 (no token provided) instead of 500 error
Invoke-WebRequest -Uri "https://localhost:7091/scim/ServiceProviderConfig" `
  -SkipCertificateCheck
```

## 🔄 Alternative Approaches

### If You Want Manual Registration

```csharp
// Instead of AddJwtTokenService()
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
```

### If You Want Custom Lifetime

```csharp
// Scoped lifetime (once per HTTP request)
builder.Services.AddJwtTokenService(ServiceLifetime.Scoped);

// Transient lifetime (new instance each time)
builder.Services.AddJwtTokenService(ServiceLifetime.Transient);
```

## 📊 Comparison Table

| Method | Use Case | Efficiency | Thread Safety |
|--------|----------|-----------|---------------|
| `AddJwtTokenService()` | Default, recommended | ⭐⭐⭐ Highest | ✅ Safe |
| `AddJwtTokenService(Scoped)` | Per-request isolation | ⭐⭐ Medium | ✅ Safe |
| `AddJwtTokenService(Transient)` | Testing, custom | ⭐ Lowest | ✅ Safe |
| Manual registration | Legacy code | ⭐⭐⭐ Highest | ✅ Safe |

## 🧪 Verification Checklist

After applying the fix:

- [ ] Project builds without errors
- [ ] `AddJwtTokenService()` is called in `Program.cs`
- [ ] `appsettings.json` has `Jwt:SecretKey`
- [ ] Application starts without errors
- [ ] `/scim/ServiceProviderConfig` returns 401 (expected)
- [ ] Token generation works
- [ ] Authorized requests succeed

## 🔍 Debugging Steps

If the issue persists:

### Step 1: Verify Service Registration

Add logging to see registered services:

```csharp
// In Program.cs, after building services
using (var scope = app.Services.CreateScope())
{
    var jwtService = scope.ServiceProvider.GetService<IJwtTokenService>();
    if (jwtService == null)
        Console.WriteLine("ERROR: IJwtTokenService not registered!");
    else
        Console.WriteLine("SUCCESS: IJwtTokenService registered");
}
```

### Step 2: Check Configuration Loading

```csharp
var secretKey = builder.Configuration["Jwt:SecretKey"];
Console.WriteLine($"Secret Key configured: {!string.IsNullOrEmpty(secretKey)}");
```

### Step 3: Verify Assembly Loading

Ensure `EzSCIM` library is referenced:

```xml
<!-- In your .csproj file -->
<ItemGroup>
  <ProjectReference Include="..\EzSCIM\EzSCIM.csproj" />
</ItemGroup>
```

## 📝 Complete Working Example

Here's a minimal `Program.cs` that works:

```csharp
using EzSCIM.Services;
using EzSCIM.Authentication;
using EzSCIM.Controllers;
using EzSCIM.Repositories;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register SCIM repository
builder.Services.AddSingleton<IScimRepository, InMemoryScimRepository>();

// ✅ Register JWT service using extension
builder.Services.AddJwtTokenService();

// ✅ Configure authentication
builder.Services.AddAuthentication()
    .AddScheme<JwtBearerTokenAuthenticationOptions, JwtBearerTokenAuthenticationHandler>(
        "Bearer", null);

builder.Services.AddAuthorization();

// ✅ Add SCIM controllers
builder.Services.AddScimControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 🎓 Learning Resources

- **Extension Methods**: `EzSCIM/Services/JWT-SERVICE-EXTENSION-GUIDE.md`
- **Quick Fix**: `QUICK-FIX-JWT-SERVICE.md`
- **Full Guide**: `EzSCIM.EntraID.AppHost/ASPIRE-ENTRAID-SCIM-GUIDE.md`
- **Code Standards**: `.github/copilot-instructions.md`

## ⚡ Quick Checklist

```
[ ] Using statement: `using EzSCIM.Services;`
[ ] Extension call: `builder.Services.AddJwtTokenService();`
[ ] Config file: `appsettings.json` has `Jwt:SecretKey`
[ ] Project reference: EzSCIM library is referenced
[ ] Build clean: `dotnet clean && dotnet build`
[ ] Error gone: Application starts without DI error
```

## 🚨 If Still Having Issues

### Symptom: Still getting "Unable to resolve"

**Solution**:
1. Check you're using `builder.Services` (not `services` from different scope)
2. Ensure `AddJwtTokenService()` is called BEFORE building the app
3. Verify no other service registration is removing it

### Symptom: Configuration error

**Solution**:
1. Check `Jwt:SecretKey` exists in `appsettings.json`
2. Minimum 32 characters required
3. No special characters in key

### Symptom: Build errors

**Solution**:
1. Run `dotnet clean`
2. Run `dotnet restore`
3. Run `dotnet build`
4. If still failing, check that all project references are correct

## 📞 Support

1. **Quick questions**: Check this document
2. **Code examples**: See `EzSCIM.EntraID.Demo/Program.cs`
3. **Detailed help**: Read `JWT-SERVICE-EXTENSION-GUIDE.md`
4. **Complete setup**: Follow `ASPIRE-ENTRAID-SCIM-GUIDE.md`

---

**Summary**: The solution is simple - add `builder.Services.AddJwtTokenService();` to your `Program.cs` and ensure the JWT configuration is in `appsettings.json`. This registers the JWT Token Service in the dependency injection container, resolving the error.

**Version**: 1.0  
**Last Updated**: February 20, 2026

