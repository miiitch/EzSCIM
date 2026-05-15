# IQueryable Integration Reference

Full guide: https://ezscim.miiitch.dev/iqueryable/getting-started/

---

## Step 1 — Install package

```bash
dotnet add package EzSCIM
```

---

## Step 2 — Annotate entity with `[ScimProperty]`

```csharp
using EzSCIM.Attributes;

public class MyUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
    public string Email { get; set; } = string.Empty;

    [ScimProperty("givenName", "string")]
    public string FirstName { get; set; } = string.Empty;

    [ScimProperty("familyName", "string")]
    public string LastName { get; set; } = string.Empty;

    [ScimProperty("displayName", "string")]
    public string DisplayName { get; set; } = string.Empty;

    [ScimProperty("active", "boolean")]
    public bool IsActive { get; set; } = true;
}
```

Use standard SCIM attribute names (`userName`, `active`, `givenName`, `familyName`, `displayName`) so
Entra ID maps them automatically.

Full attribute reference: https://ezscim.miiitch.dev/iqueryable/scim-attributes/

Schema extensions (`[ScimProperty]` on custom attributes): https://ezscim.miiitch.dev/iqueryable/schema-extensions/

---

## Step 3 — Implement the data repository

### Users + Groups

```csharp
using EzSCIM.DataRepositories;

public class MyRepository : IUserGroupDataRepository<MyUser, MyGroup>
{
    private readonly AppDbContext _context;
    public MyRepository(AppDbContext context) => _context = context;

    // Must return IQueryable<T> — EzSCIM applies SCIM filters as LINQ
    public IQueryable<MyUser> QueryUsers() => _context.Users.AsQueryable();

    public Task<MyUser?> GetUserAsync(string id) => _context.Users.FindAsync(id).AsTask();
    public async Task<MyUser> CreateUserAsync(MyUser user) { _context.Users.Add(user); await _context.SaveChangesAsync(); return user; }
    public async Task<MyUser?> UpdateUserAsync(string id, MyUser user) { /* load, update, save */ return user; }
    public async Task<bool> DeleteUserAsync(string id) { /* load, remove, save */ return true; }

    public IQueryable<MyGroup> QueryGroups() => _context.Groups.AsQueryable();
    // ... GetGroupAsync, CreateGroupAsync, UpdateGroupAsync, DeleteGroupAsync
}
```

### Users only

Implement `IUserDataRepository<TUser>` and register `IScimUserOnlyRepository<ScimUser>` in DI instead.

Repository interfaces reference: https://ezscim.miiitch.dev/iqueryable/repository/

---

## Step 4 — Register services in `Program.cs`

### Users + Groups (recommended)

```csharp
using EzSCIM.Controllers;
using EzSCIM.DataRepositories;
using EzSCIM.Filtering;
using EzSCIM.Repositories;
using EzSCIM.Services;
using EzSCIM.Authentication;

// Data repository
builder.Services.AddScoped<IUserGroupDataRepository<MyUser, MyGroup>, MyRepository>();

// Filter translators (SCIM filter → LINQ)
builder.Services.AddScoped<IScimFilterTranslator<MyUser>, GenericScimFilterTranslator<MyUser>>();
builder.Services.AddScoped<IScimFilterTranslator<MyGroup>, GenericScimFilterTranslator<MyGroup>>();

// SCIM repository (bridges data repo + filter translators)
builder.Services.AddScoped<IScimRepository, ScimRepository<MyUser, MyGroup>>();

// Controllers
builder.Services.AddControllers().AddEzScimControllers();

// Authentication (if needed — see authentication.md)
builder.Services.AddJwtTokenService();
builder.Services.AddAuthentication()
    .AddScheme<JwtBearerTokenAuthenticationOptions, JwtBearerTokenAuthenticationHandler>("Bearer", null);
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

> ⚠️ Return `IQueryable<T>` from `QueryUsers()` / `QueryGroups()`, not `IEnumerable<T>`.
> Returning `list.AsQueryable()` works but loads all rows before filtering.

Filtering reference: https://ezscim.miiitch.dev/iqueryable/filtering/
