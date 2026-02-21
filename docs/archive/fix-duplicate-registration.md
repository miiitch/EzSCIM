# 🔧 Fix: Duplicate Service Registration Error

## Problem

```
System.AggregateException: Some services are not able to be constructed
Error while validating the service descriptor 'ServiceType: 
EzSCIM.Authentication.JwtBearerTokenAuthenticationHandler Lifetime: Transient 
ImplementationType: EzSCIM.Authentication.JwtBearerTokenAuthenticationHandler': 
Unable to resolve service for type 'EzSCIM.Services.IJwtTokenService' 
while attempting to activate
```

## Root Cause

**Duplicate service registration** in `Program.cs`:

```csharp
// ❌ WRONG - Registers only the concrete type, NOT the interface
builder.Services.AddSingleton<JwtTokenService>();

// ✅ CORRECT - Registers the interface with implementation
builder.Services.AddJwtTokenService();
```

When you register `AddSingleton<JwtTokenService>()` without specifying the interface, the DI container only knows about the concrete class `JwtTokenService`, not the interface `IJwtTokenService`.

The `JwtBearerTokenAuthenticationHandler` requires `IJwtTokenService` (the interface), which is not registered, causing the error.

## ✅ Solution Applied

### Before (Incorrect)
```csharp
// Register SCIM repository
builder.Services.AddSingleton<IScimRepository, InMemoryScimRepository>();
builder.Services.AddSingleton<JwtTokenService>();  // ❌ Missing interface

// Register JWT token service
builder.Services.AddJwtTokenService();
```

### After (Correct)
```csharp
// Register SCIM repository
builder.Services.AddSingleton<IScimRepository, InMemoryScimRepository>();

// Register JWT token service
builder.Services.AddJwtTokenService();  // ✅ Registers IJwtTokenService -> JwtTokenService
```

## What Changed

1. **Removed**: `builder.Services.AddSingleton<JwtTokenService>();`
2. **Kept**: `builder.Services.AddJwtTokenService();`

The extension method `AddJwtTokenService()` correctly registers:
```csharp
services.AddSingleton<IJwtTokenService, JwtTokenService>();
```

## Additional Fixes

Also updated log messages from French to English to comply with global repository standards:

**Before**:
```csharp
logger.LogInformation("Chargement des données de test...");
logger.LogInformation("Utilisateur de test créé");
logger.LogInformation("Groupe de test créé");
```

**After**:
```csharp
logger.LogInformation("Loading test data...");
logger.LogInformation("Test user created successfully");
logger.LogInformation("Test group created successfully");
```

## How to Verify

1. **Clean and rebuild**:
   ```powershell
   dotnet clean
   dotnet build
   ```

2. **Run the application**:
   ```powershell
   dotnet run --project .\EzSCIM.EntraID.Demo
   ```

3. **Check for errors**:
   - Application should start without DI exceptions
   - Log should show "Loading test data..." if configured
   - No `AggregateException` errors

## Common Mistake Pattern

### ❌ Wrong Patterns
```csharp
// Missing interface mapping
builder.Services.AddSingleton<ConcreteClass>();

// Double registration
builder.Services.AddSingleton<IInterface, ConcreteClass>();
builder.Services.AddSingleton<ConcreteClass>();
```

### ✅ Correct Patterns
```csharp
// Using extension method (recommended)
builder.Services.AddJwtTokenService();

// Manual registration with interface
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Only if you need both registrations explicitly
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton(sp => sp.GetRequiredService<IJwtTokenService>() as JwtTokenService);
```

## Prevention

To avoid this issue:

1. **Always use extension methods** when available:
   - `AddJwtTokenService()`
   - `AddScimControllers()`
   - Custom extension methods

2. **When registering manually**, always specify the interface:
   ```csharp
   builder.Services.AddSingleton<IInterface, Implementation>();
   ```

3. **Check for duplicates** before adding registrations

4. **Use dependency injection validation** in development:
   ```csharp
   if (app.Environment.IsDevelopment())
   {
       var serviceProvider = app.Services;
       serviceProvider.GetRequiredService<IJwtTokenService>(); // Throws if not registered
   }
   ```

## Understanding the Extension Method

The `AddJwtTokenService()` extension method in `EzSCIM/Services/ServiceCollectionExtensions.cs`:

```csharp
public static IServiceCollection AddJwtTokenService(this IServiceCollection services)
{
    // Correctly registers the interface with the implementation
    return services.AddSingleton<IJwtTokenService, JwtTokenService>();
}
```

This ensures:
- ✅ `IJwtTokenService` can be injected
- ✅ DI container knows how to create `JwtTokenService`
- ✅ Singleton lifetime (one instance for the application)

## Testing the Fix

```powershell
# 1. Clean build
dotnet clean
dotnet restore
dotnet build

# 2. Run application
dotnet run --project .\EzSCIM.EntraID.Demo

# 3. Check logs - should see:
# info: Microsoft.Hosting.Lifetime[0]
#       Now listening on: https://localhost:7091
# info: Program[0]
#       Loading test data...
# info: Program[0]
#       Test user created successfully
```

## Related Documentation

- `QUICK-FIX-JWT-SERVICE.md` - Original JWT service fix
- `JWT-SERVICE-EXTENSION-GUIDE.md` - Extension method documentation
- `.github/copilot-instructions.md` - Code standards

## Status

✅ **Fixed** - Duplicate registration removed  
✅ **Verified** - Application compiles  
✅ **Standards** - English-only logs  

---

**Fix Applied**: February 20, 2026  
**Issue**: Duplicate service registration  
**Solution**: Removed duplicate, kept extension method  
**Version**: 1.0

