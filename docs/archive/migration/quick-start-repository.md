# Quick Integration Example - Repository -> SCIM

## Scenario

You already have an existing user database and want to expose users through SCIM without rewriting your current business layer.

---

## 1) Existing model

```csharp
public class Employee
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
}
```

---

## 2) Add SCIM attributes (about 2 minutes)

```csharp
using ScimAPI.Attributes;

public class Employee
{
    public Guid Id { get; set; }

    [ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
    public string Email { get; set; } = string.Empty;

    [ScimProperty("givenName", "string")]
    public string FirstName { get; set; } = string.Empty;

    [ScimProperty("familyName", "string")]
    public string LastName { get; set; } = string.Empty;

    [ScimProperty("displayName", "string")]
    public string FullName { get; set; } = string.Empty;

    [ScimProperty("active", "boolean")]
    public bool IsEnabled { get; set; } = true;

    [ScimProperty("title", "string")]
    public string Position { get; set; } = string.Empty;

    // Non-SCIM field (not exposed)
    public DateTime HireDate { get; set; }
}
```

---

## 3) Create repository implementation (about 5 minutes)

```csharp
using ScimAPI.Repositories;
using Microsoft.EntityFrameworkCore;

public class EmployeeRepository : IUserDataRepository<Employee>
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetUserAsync(string id)
    {
        if (Guid.TryParse(id, out var guid))
            return await _context.Employees.FindAsync(guid);

        return null;
    }

    public IQueryable<Employee> QueryUsers()
    {
        return _context.Employees.AsQueryable();
    }

    public async Task<Employee> CreateUserAsync(Employee user)
    {
        _context.Employees.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<Employee?> UpdateUserAsync(string id, Employee user)
    {
        var existing = await GetUserAsync(id);
        if (existing == null) return null;

        existing.Email = user.Email;
        existing.FirstName = user.FirstName;
        existing.LastName = user.LastName;
        existing.FullName = user.FullName;
        existing.IsEnabled = user.IsEnabled;
        existing.Position = user.Position;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var existing = await GetUserAsync(id);
        if (existing == null) return false;

        _context.Employees.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
```

---

## 4) Register DI (about 3 minutes)

```csharp
using ScimAPI.Filtering;
using ScimAPI.Repositories;
using ScimAPI.Models;

builder.Services.AddScoped<IUserDataRepository<Employee>, EmployeeRepository>();
builder.Services.AddScoped<IScimFilterTranslator<Employee>, GenericScimFilterTranslator<Employee>>();

builder.Services.AddScoped<IScimUserOnlyRepository<ScimUser>>(sp =>
    new ScimUserRepositoryAdapter<Employee>(
        sp.GetRequiredService<IUserDataRepository<Employee>>(),
        sp.GetRequiredService<IScimFilterTranslator<Employee>>()));
```

If your application expects `IScimRepository`, you can bridge it:

```csharp
builder.Services.AddScoped<IScimRepository>(sp =>
    new ScimRepositoryAdapter(
        sp.GetRequiredService<IScimUserOnlyRepository<ScimUser>>()));
```

---

## 5) Validate with SCIM requests

### Create user

```http
POST /scim/Users
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:schemas:core:2.0:User"],
  "userName": "john.doe@company.com",
  "name": {
    "givenName": "John",
    "familyName": "Doe"
  },
  "displayName": "John Doe",
  "active": true,
  "title": "Developer"
}
```

### Filter users

```http
GET /scim/Users?filter=active eq true and userName co "@company.com"
```

### Filter translation behavior

```text
SCIM: active eq true and userName co "@company.com"
LINQ: u => u.IsEnabled == true && u.Email.Contains("@company.com")
SQL : WHERE IsEnabled = 1 AND Email LIKE '%@company.com%'
```

---

## SCIM attribute mapping reference

| SCIM attribute | Type | Employee property | Example |
|---|---|---|---|
| `userName` | `string` | `Email` | `john.doe@company.com` |
| `name.givenName` | `string` | `FirstName` | `John` |
| `name.familyName` | `string` | `LastName` | `Doe` |
| `displayName` | `string` | `FullName` | `John Doe` |
| `active` | `boolean` | `IsEnabled` | `true` |
| `title` | `string` | `Position` | `Developer` |

---

## Integration with Entra ID

Microsoft Entra ID can provision employees automatically once your SCIM endpoint is exposed.

```text
Entra ID -> SCIM API (/scim/Users) -> Repository Adapter -> SQL database
```

Common flows:
- User lookup by `userName`
- User creation (`POST /Users`)
- User update (`PATCH /Users/{id}`)
- User deactivation (`PATCH active=false`)

---

## Troubleshooting

### Filter does not return expected users

Check that:
1. Attributes are correctly annotated with `[ScimProperty]`
2. The requested SCIM attribute names match your mapping
3. `QueryUsers()` returns an `IQueryable` source

### User creation fails with conflict

If `userName` is unique in your system, ensure duplicate handling returns SCIM `409 Conflict`.

### Nested attributes not mapped

Use SCIM dot notation where needed (for example `name.givenName`) and ensure your translator handles nested paths.

---

## Result

You now expose your existing repository through SCIM with:
- Minimal code changes
- Type-safe mapping via attributes
- Server-side filter execution
- Compatibility with SCIM clients such as Entra ID

Continue with:
- [`repository-adapter-guide.md`](./repository-adapter-guide.md)
- [`repository-mapping-overview.md`](./repository-mapping-overview.md)
- [`groups-and-constants-extension.md`](./groups-and-constants-extension.md)
