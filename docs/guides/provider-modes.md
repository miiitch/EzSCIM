## Quick Guide - Switching Between Provider Types

### Current Setup: Users + Groups

Your current implementation supports **both User and Group resources**.

> **Note:** A groups-only mode is not supported. In SCIM, groups always reference users, so group operations require user operations to be available.

---

## To Switch to Users Only

### Step 1: Update Program.cs
```csharp
// OLD
builder.Services.AddSingleton<IScimRepository, InMemoryScimRepository>();

// NEW
builder.Services.AddSingleton<IScimUserOnlyRepository<ScimUser>, UsersOnlyRepository>();
```

### Step 2: Update Controllers
```csharp
// OLD
public UsersController(IScimRepository repository) { ... }

// NEW
public UsersController(IScimUserOnlyRepository<ScimUser> repository) { ... }
```

### Step 3: Update ResourceTypes (in ScimConfigController.cs)
```csharp
// Only return User ResourceType
var resourceTypes = new List<ScimResourceType>
{
    new ScimResourceType
    {
        Id = "User",
        Name = "User",
        Endpoint = "/Users",
        Schema = "urn:ietf:params:scim:schemas:core:2.0:User"
    }
};
```

---

## To Keep Users + Groups (Current)

No changes needed! Keep using:

```csharp
// Program.cs
builder.Services.AddSingleton<IScimRepository, InMemoryScimRepository>();

// Controllers
public UsersController(IScimRepository repository) { ... }
public GroupsController(IScimRepository repository) { ... }

// ResourceTypes - Return both
var resourceTypes = new List<ScimResourceType>
{
    new ScimResourceType { Id = "User", ... },
    new ScimResourceType { Id = "Group", ... }
};
```

---

## Comparison Table

| Feature | Users Only | Users + Groups |
|---------|-----------|----------------|
| **Interface** | IScimUserOnlyRepository | IScimRepository |
| **Implementation** | UsersOnlyRepository | InMemoryScimRepository |
| **User Endpoints** | ✅ | ✅ |
| **Group Endpoints** | ❌ | ✅ |
| **ResourceTypes** | User only | User + Group |

---

## Environment-Specific Configuration

You can also use environment variables:

```csharp
// In Program.cs
var mode = builder.Configuration["Scim:ProviderMode"]; // "UsersOnly", "Both"

switch (mode)
{
    case "UsersOnly":
        builder.Services.AddSingleton<IScimUserOnlyRepository<ScimUser>, UsersOnlyRepository>();
        break;
    default: // "Both"
        builder.Services.AddSingleton<IScimRepository, InMemoryScimRepository>();
        break;
}
```

Then in appsettings.json:
```json
{
  "Scim": {
    "ProviderMode": "Both"  // or "UsersOnly"
  }
}
```

---

## Testing Each Mode

### Test Users Only
```powershell
# Should work
curl -H "Authorization: Bearer $token" https://localhost:7001/scim/Users

# Should return ResourceTypes with only User
curl -H "Authorization: Bearer $token" https://localhost:7001/scim/ResourceTypes
```

### Test Users + Groups
```powershell
# Both should work
curl -H "Authorization: Bearer $token" https://localhost:7001/scim/Users
curl -H "Authorization: Bearer $token" https://localhost:7001/scim/Groups

# Should return ResourceTypes with User and Group
curl -H "Authorization: Bearer $token" https://localhost:7001/scim/ResourceTypes
```

---

## Summary

✅ **Current Mode:** Users + Groups  
✅ **Easy to Switch:** Just change 2-3 lines  
✅ **Type Safe:** Proper interface injection  
✅ **Standards Compliant:** ResourceTypes declares support  
✅ **No Breaking Changes:** Backward compatible  

Choose your mode based on your SCIM requirements!
