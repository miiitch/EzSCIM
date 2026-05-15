# Getting Started — IQueryable Integration

This guide shows how to expose any existing data source as a SCIM 2.0 endpoint
using the `EzSCIM` package. No EF Core required.

!!! info "Time to complete: ~15 minutes"
    Works with any data source that supports `IQueryable<T>` — EF Core, Dapper, Cosmos DB, MongoDB, or custom.

---

## 1. Install the package

```bash
dotnet add package EzSCIM
```

---

## 2. Annotate your entity

Add `[ScimProperty]` to the properties you want to expose via SCIM.
**Only annotated properties are mapped** — unannotated properties are never sent over the wire.

```csharp
using EzSCIM.Attributes;

public class Employee
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ScimProperty("userName", "string", Required = true, Uniqueness = "server")] // (1)
    public string Email { get; set; } = string.Empty;

    [ScimProperty("givenName", "string")]
    public string FirstName { get; set; } = string.Empty;

    [ScimProperty("familyName", "string")]
    public string LastName { get; set; } = string.Empty;

    [ScimProperty("displayName", "string")]
    public string FullName { get; set; } = string.Empty;

    [ScimProperty("active", "boolean")] // (2)
    public bool IsEnabled { get; set; } = true;

    [ScimProperty("title", "string")]
    public string Position { get; set; } = string.Empty;
}
```

1. `userName` is the SCIM attribute name. `Required = true` makes it mandatory in the schema.
2. `active` is used by Entra ID for enable/disable lifecycle operations.

!!! tip "SCIM attribute names"
    Use the standard SCIM names (`userName`, `active`, `givenName`…) so Entra ID
    and other provisioning clients can map their attributes automatically.
    See the [SCIM 2.0 attribute reference →](./scim-attributes.md)

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

    // IQueryable<T> is required — EzSCIM applies SCIM filters as LINQ // (1)
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

1. Returning `IQueryable<T>` (not `IEnumerable<T>`) is critical — filters and pagination are
   pushed down to the database. Returning `list.AsQueryable()` works but loads all rows first.

!!! note "Users only?"
    If you don't need group support, implement `IUserDataRepository<TUser>` instead and register
    `IScimUserOnlyRepository<ScimUser>` in DI. See the [repository interfaces reference →](./repository.md)

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

// Data repository — provides IQueryable<T> sources // (1)
builder.Services.AddScoped<IUserGroupDataRepository<Employee, Department>, EmployeeRepository>();

// Filter translators — convert SCIM filter expressions to LINQ // (2)
builder.Services.AddScoped<IScimFilterTranslator<Employee>, GenericScimFilterTranslator<Employee>>();
builder.Services.AddScoped<IScimFilterTranslator<Department>, GenericScimFilterTranslator<Department>>();

// SCIM repository — bridges data repo + filter translators to SCIM operations // (3)
builder.Services.AddScoped<IScimRepository, ScimRepository<Employee, Department>>();

// Authentication
builder.Services.AddJwtTokenService();
builder.Services.AddAuthentication()
    .AddScheme<JwtBearerTokenAuthenticationOptions, JwtBearerTokenAuthenticationHandler>("Bearer", null);
builder.Services.AddAuthorization();

// SCIM controllers (registers /scim/Users, /scim/Groups, /scim/Schemas, etc.) // (4)
builder.Services.AddScimControllers();

// Development-only token endpoint
builder.Services.AddScimTokenGeneratorEndpoint();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

1. Your repository implementation that returns `IQueryable<T>`.
2. `GenericScimFilterTranslator<T>` uses `[ScimProperty]` to map SCIM names to C# properties.
3. `ScimRepository<TUser, TGroup>` wires data + filter translators into SCIM operations.
4. Registers controllers for `/scim/Users`, `/scim/Groups`, `/scim/Schemas`, and `/scim/ServiceProviderConfig`.

---

## 5. Verify

=== "cURL"

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

=== "PowerShell"

    ```powershell
    $headers = @{ Authorization = "Bearer $TOKEN" }

    # List users
    Invoke-RestMethod -Uri "https://localhost:7001/scim/Users" -Headers $headers

    # Filter users
    $filter = 'active eq true'
    $uri = "https://localhost:7001/scim/Users?filter=$([uri]::EscapeDataString($filter))"
    Invoke-RestMethod -Uri $uri -Headers $headers
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

