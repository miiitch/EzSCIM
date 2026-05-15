# EF Core Integration Reference

Full guide: https://ezscim.miiitch.dev/efcore/getting-started/

---

## Step 1 — Install packages

```bash
dotnet add package EzSCIM
dotnet add package EzSCIM.EfCore
```

---

## Step 2 — Implement `IScimEntity` on entity classes

```csharp
using EzSCIM.EfCore;

public class AppUser : IScimEntity
{
    public string Id { get; set; } = string.Empty;       // auto-generated GUID if empty
    public DateTime CreatedAt { get; set; }               // set once on creation (meta.created)
    public DateTime ModifiedAt { get; set; }              // updated on every write (meta.lastModified)

    [ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
    public string UserName { get; set; } = string.Empty;

    [ScimProperty("displayName", "string")]
    public string DisplayName { get; set; } = string.Empty;

    [ScimProperty("givenName", "string")]
    public string? GivenName { get; set; }

    [ScimProperty("familyName", "string")]
    public string? FamilyName { get; set; }

    [ScimProperty("active", "boolean")]
    public bool Active { get; set; } = true;

    public string EmailsJson { get; set; } = "[]";        // multi-value — stored as JSON
}

public class AppGroup : IScimEntity
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    [ScimProperty("displayName", "string", Required = true)]
    public string DisplayName { get; set; } = string.Empty;

    public string MembersJson { get; set; } = "[]";       // multi-value — stored as JSON
}
```

`IScimEntity` reference: https://ezscim.miiitch.dev/efcore/iscimentity/

---

## Step 3 — Configure `DbContext`

### SQL Server

```csharp
modelBuilder.Entity<AppUser>(e =>
{
    e.HasKey(u => u.Id);
    e.HasIndex(u => u.UserName).IsUnique();
    e.Property(u => u.UserName).IsRequired().HasMaxLength(256);
    e.Property(u => u.Active).HasDefaultValue(true);
    e.Property(u => u.EmailsJson).HasColumnType("nvarchar(max)");
});
modelBuilder.Entity<AppGroup>(e =>
{
    e.HasKey(g => g.Id);
    e.HasIndex(g => g.DisplayName).IsUnique();
    e.Property(g => g.DisplayName).IsRequired().HasMaxLength(256);
    e.Property(g => g.MembersJson).HasColumnType("nvarchar(max)");
});
```

### PostgreSQL

Replace `nvarchar(max)` with `jsonb`:
```csharp
e.Property(u => u.EmailsJson).HasColumnType("jsonb");
e.Property(g => g.MembersJson).HasColumnType("jsonb");
```

Multi-provider setup (SQL Server + PostgreSQL): https://ezscim.miiitch.dev/efcore/multi-provider/

---

## Step 4 — Inherit `EfScimRepositoryBase`

```csharp
using EzSCIM.EfCore;
using Microsoft.EntityFrameworkCore;

public class AppRepository : EfScimRepositoryBase<AppUser, AppGroup, AppDbContext>
{
    public AppRepository(AppDbContext context) : base(context) { }

    protected override DbSet<AppUser>  Users  => Context.Users;
    protected override DbSet<AppGroup> Groups => Context.Groups;
}
```

This is all you need. `EfScimRepositoryBase` provides:
- `GetUserAsync`, `QueryUsers`, `CreateUserAsync` (auto-Id + timestamps), `UpdateUserAsync`, `DeleteUserAsync`
- Same for groups
- Unique constraint violations → `409 Conflict` automatically

### JSON column hook (if needed)

If your entity has JSON columns, override `OnBeforeUpdateUserAsync` to copy them manually
(EF Core's `SetValues` skips JSON columns):

```csharp
protected override Task OnBeforeUpdateUserAsync(AppUser existing, AppUser updated)
{
    Context.Entry(existing).CurrentValues.SetValues(updated);
    existing.EmailsJson = updated.EmailsJson; // must be assigned explicitly
    return Task.CompletedTask;
}

protected override Task OnBeforeUpdateGroupAsync(AppGroup existing, AppGroup updated)
{
    Context.Entry(existing).CurrentValues.SetValues(updated);
    existing.MembersJson = updated.MembersJson;
    return Task.CompletedTask;
}
```

`EfScimRepositoryBase` reference: https://ezscim.miiitch.dev/efcore/efrepositorybase/

---

## Step 5 — Register services in `Program.cs`

```csharp
using EzSCIM.Controllers;
using EzSCIM.DataRepositories;
using EzSCIM.Filtering;
using EzSCIM.Repositories;
using EzSCIM.Services;
using EzSCIM.Authentication;

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Data repository
builder.Services.AddScoped<IUserGroupDataRepository<AppUser, AppGroup>, AppRepository>();

// Filter translators (SCIM filter → LINQ)
builder.Services.AddScoped<IScimFilterTranslator<AppUser>, GenericScimFilterTranslator<AppUser>>();
builder.Services.AddScoped<IScimFilterTranslator<AppGroup>, GenericScimFilterTranslator<AppGroup>>();

// SCIM repository
builder.Services.AddScoped<IScimRepository, ScimRepository<AppUser, AppGroup>>();

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
