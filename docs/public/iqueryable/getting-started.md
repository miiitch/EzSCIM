# Getting Started — IQueryable Integration

This guide shows how to expose any existing data source as a SCIM 2.0 endpoint
using the `EzSCIM` package. No EF Core required.

**Time to complete**: ~15 minutes.

---

## 1. Install the package

```bash
dotnet add package EzSCIM
```

---

## 2. Annotate your entity

Add `[ScimProperty]` to the properties you want to expose via SCIM.
Only annotated properties are mapped.

```csharp
using EzSCIM.Attributes;

public class Employee
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

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
}
```

---

## 3. Implement the data repository

Implement `IUserGroupDataRepository<TUser, TGroup>` (or `IUserDataRepository<TUser>` for
users only). The key method is `QueryUsers()` which returns `IQueryable<T>` — EzSCIM
applies SCIM filters as LINQ expressions server-side.

```csharp
using EzSCIM.DataRepositories;

public class EmployeeRepository : IUserGroupDataRepository<Employee, Department>
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context) => _context = context;

    // IQueryable<T> is required — EzSCIM applies SCIM filters as LINQ
    public IQueryable<Employee> QueryUsers() => _context.Employees.AsQueryable();

    public async Task<Employee?> GetUserAsync(string id)
        => await _context.Employees.FindAsync(id);

    public async Task<Employee> CreateUserAsync(Employee user)
    {
        _context.Employees.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<Employee?> UpdateUserAsync(string id, Employee user)
    {
        var existing = await _context.Employees.FindAsync(id);
        if (existing is null) return null;
        _context.Entry(existing).CurrentValues.SetValues(user);
        existing.Id = id;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _context.Employees.FindAsync(id);
        if (user is null) return false;
        _context.Employees.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- Group operations (implement similarly) ---
    public IQueryable<Department> QueryGroups() => _context.Departments.AsQueryable();
    public async Task<Department?> GetGroupAsync(string id) => await _context.Departments.FindAsync(id);
    public async Task<Department> CreateGroupAsync(Department group) { /* ... */ return group; }
    public async Task<Department?> UpdateGroupAsync(string id, Department group) { /* ... */ return group; }
    public async Task<bool> DeleteGroupAsync(string id) { /* ... */ return true; }
}
```

---

## 4. Register services in Program.cs

```csharp
using EzSCIM.Controllers;
using EzSCIM.DataRepositories;
using EzSCIM.Filtering;
using EzSCIM.Repositories;
using EzSCIM.Services;
using EzSCIM.Authentication;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Your DbContext
builder.Services.AddDbContext<AppDbContext>(/* ... */);

// Data repository — provides IQueryable<T> sources
builder.Services.AddScoped<IUserGroupDataRepository<Employee, Department>, EmployeeRepository>();

// Filter translators — convert SCIM filter expressions to LINQ
builder.Services.AddScoped<IScimFilterTranslator<Employee>, GenericScimFilterTranslator<Employee>>();
builder.Services.AddScoped<IScimFilterTranslator<Department>, GenericScimFilterTranslator<Department>>();

// SCIM repository — bridges data repo + filter translators to SCIM operations
builder.Services.AddScoped<IScimRepository, ScimRepository<Employee, Department>>();

// Authentication
builder.Services.AddJwtTokenService();
builder.Services.AddAuthentication()
    .AddScheme<JwtBearerTokenAuthenticationOptions, JwtBearerTokenAuthenticationHandler>("Bearer", null);
builder.Services.AddAuthorization();

// SCIM controllers (registers /scim/Users, /scim/Groups, /scim/Schemas, etc.)
builder.Services.AddScimControllers();

// Development-only token endpoint
builder.Services.AddScimTokenGeneratorEndpoint();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## 5. Verify

```bash
# List users
curl -H "Authorization: Bearer $TOKEN" https://localhost:7001/scim/Users

# Filter users
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20true"

# Create user
curl -X POST https://localhost:7001/scim/Users \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/scim+json" \
  -d '{"schemas":["urn:ietf:params:scim:schemas:core:2.0:User"],"userName":"jane.doe@acme.com","active":true}'
```

---

## How SCIM filter translation works

```
SCIM request:  GET /scim/Users?filter=active eq true and userName co "@acme.com"
                                       │
                              IScimFilterTranslator<Employee>
                                       │
                              LINQ:    u => u.IsEnabled == true
                                           && u.Email.Contains("@acme.com")
                                       │
                              SQL:     WHERE IsEnabled = 1
                                       AND Email LIKE '%@acme.com%'
```

The translation uses `[ScimProperty]` annotations to map SCIM attribute names (`userName`, `active`)
to C# property names (`Email`, `IsEnabled`).

---

## Next steps

- [Repository interfaces reference →](./repository.md)
- [SCIM filter syntax →](./filtering.md)
- [SCIM 2.0 attribute reference →](./scim-attributes.md)
- [Schema extensions →](./schema-extensions.md)
- [Authentication setup →](../authentication.md)

