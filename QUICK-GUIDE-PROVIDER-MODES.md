## 🚀 Quick Guide - Switching Between Provider Types

### Current Setup: Users + Groups + Schemas

Your current implementation supports **all three resources**.

---

## To Switch to Users Only

### Step 1: Update Program.cs
```csharp
// OLD
builder.Services.AddSingleton<IScimRepository, InMemoryScimRepository>();

// NEW
builder.Services.AddSingleton<IScimUserRepository, UsersOnlyRepository>();
```

### Step 2: Update Controllers
```csharp
// OLD
public UsersController(IScimRepository repository) { ... }

// NEW
public UsersController(IScimUserRepository repository) { ... }
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

## To Switch to Groups Only

### Step 1: Update Program.cs
```csharp
// OLD
builder.Services.AddSingleton<IScimRepository, InMemoryScimRepository>();

// NEW
builder.Services.AddSingleton<IScimGroupRepository, GroupsOnlyRepository>();
```

### Step 2: Update Controllers
```csharp
// OLD
public GroupsController(IScimRepository repository) { ... }

// NEW
public GroupsController(IScimGroupRepository repository) { ... }
```

### Step 3: Update ResourceTypes
```csharp
// Only return Group ResourceType
var resourceTypes = new List<ScimResourceType>
{
    new ScimResourceType
    {
        Id = "Group",
        Name = "Group",
        Endpoint = "/Groups",
        Schema = "urn:ietf:params:scim:schemas:core:2.0:Group"
    }
};
```

---

## To Switch to Users + Groups (Keep Current)

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

| Feature | Users Only | Groups Only | Users + Groups |
|---------|-----------|------------|----------------|
| **Interface** | IScimUserRepository | IScimGroupRepository | IScimRepository |
| **Implementation** | UsersOnlyRepository | GroupsOnlyRepository | InMemoryScimRepository |
| **User Endpoints** | ✅ | ❌ | ✅ |
| **Group Endpoints** | ❌ | ✅ | ✅ |
| **Schema Endpoints** | ❌ | ❌ | ✅ |
| **ResourceTypes** | User only | Group only | User + Group |

---

## Environment-Specific Configuration

You can also use environment variables:

```csharp
// In Program.cs
var mode = builder.Configuration["Scim:ProviderMode"]; // "UsersOnly", "GroupsOnly", "Both"

switch (mode)
{
    case "UsersOnly":
        builder.Services.AddSingleton<IScimUserRepository, UsersOnlyRepository>();
        break;
    case "GroupsOnly":
        builder.Services.AddSingleton<IScimGroupRepository, GroupsOnlyRepository>();
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
    "ProviderMode": "Both"  // or "UsersOnly" or "GroupsOnly"
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

### Test Groups Only
```powershell
# Should work
curl -H "Authorization: Bearer $token" https://localhost:7001/scim/Groups

# Should return ResourceTypes with only Group
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

✅ **Current Mode:** Users + Groups + Schemas  
✅ **Easy to Switch:** Just change 2-3 lines  
✅ **Type Safe:** Proper interface injection  
✅ **Standards Compliant:** ResourceTypes declares support  
✅ **No Breaking Changes:** Backward compatible  

Choose your mode based on your SCIM requirements!
