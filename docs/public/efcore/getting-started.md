# Getting Started — EF Core Integration

This guide shows how to expose your EF Core data as a SCIM 2.0 endpoint
using the `EzSCIM` and `EzSCIM.EfCore` packages.

!!! info "Time to complete: ~10 minutes"
    If you already have an EF Core `DbContext` and entity classes, you only need
    to add `IScimEntity`, inherit `EfScimRepositoryBase`, and register the services.

---

## 1. Install the packages

```bash
dotnet add package EzSCIM
dotnet add package EzSCIM.EfCore
```

---

## 2. Implement `IScimEntity` on your entity classes

Your EF Core entity classes must implement `IScimEntity` to enable automatic
Id generation and timestamp management.

```csharp
using EzSCIM.EfCore;

public class AppUser : IScimEntity
{
    public string Id { get; set; } = string.Empty;     // (1)
    public DateTime CreatedAt { get; set; }             // (2)
    public DateTime ModifiedAt { get; set; }            // (3)

    // Your domain properties
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public bool Active { get; set; } = true;
    public string EmailsJson { get; set; } = "[]";     // (4)
}

public class AppGroup : IScimEntity
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    public string MembersJson { get; set; } = "[]";    // (4)
}
```

1. Auto-generated GUID if empty on creation.
2. Set once when the entity is first created. Maps to `meta.created`.
3. Updated on every write. Maps to `meta.lastModified`.
4. Multi-value attributes (emails, members) stored as JSON strings.

---

## 3. Create your DbContext

=== "SQL Server"

    ```csharp
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Users { get; set; } = null!;
        public DbSet<AppGroup> Groups { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
    }
    ```

=== "PostgreSQL"

    ```csharp
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Users { get; set; } = null!;
        public DbSet<AppGroup> Groups { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.UserName).IsUnique();
                e.Property(u => u.UserName).IsRequired().HasMaxLength(256);
                e.Property(u => u.Active).HasDefaultValue(true);
                e.Property(u => u.EmailsJson).HasColumnType("jsonb"); // Native JSON indexing
            });

            modelBuilder.Entity<AppGroup>(e =>
            {
                e.HasKey(g => g.Id);
                e.HasIndex(g => g.DisplayName).IsUnique();
                e.Property(g => g.DisplayName).IsRequired().HasMaxLength(256);
                e.Property(g => g.MembersJson).HasColumnType("jsonb");
            });
        }
    }
    ```

!!! tip "Multi-provider setup"
    If you need to support both SQL Server and PostgreSQL (e.g. production vs. tests),
    use a base context with provider-specific subclasses.
    See [Multi-provider setup →](./multi-provider.md)

---

## 4. Inherit `EfScimRepositoryBase`

```csharp
using EzSCIM.DataRepositories;
using EzSCIM.EfCore;
using Microsoft.EntityFrameworkCore;

public class AppUserGroupRepository
    : EfScimRepositoryBase<AppUser, AppGroup, AppDbContext> // (1)
{
    public AppUserGroupRepository(AppDbContext context) : base(context) { }

    protected override DbSet<AppUser>  Users  => Context.Users;  // (2)
    protected override DbSet<AppGroup> Groups => Context.Groups;
}
```

1. Three generic parameters: your user entity, group entity, and DbContext type.
2. Just point to your DbSets. All CRUD, Id generation, and timestamps are handled by the base class.

!!! note "What you get for free"
    `EfScimRepositoryBase` provides: `GetUserAsync`, `QueryUsers`, `CreateUserAsync`
    (with auto-Id + timestamps), `UpdateUserAsync`, `DeleteUserAsync` — and the same for groups.
    Unique constraint violations are automatically converted to `409 Conflict`.

---

## 5. Implement `IScimRepository`

You still need to bridge entity↔ScimModel conversions (entity JSON columns → SCIM `emails[]`).
Implement `IScimRepository` and delegate CRUD to `IUserGroupDataRepository`:

```csharp
using EzSCIM.DataRepositories;
using EzSCIM.Filtering;
using EzSCIM.Models;
using EzSCIM.Repositories;

public class AppScimRepository : IScimRepository
{
    private readonly IUserGroupDataRepository<AppUser, AppGroup> _data;
    private readonly IScimFilterTranslator<AppUser> _userFilter;
    private readonly IScimFilterTranslator<AppGroup> _groupFilter;

    public AppScimRepository(
        IUserGroupDataRepository<AppUser, AppGroup> data,
        IScimFilterTranslator<AppUser> userFilter,
        IScimFilterTranslator<AppGroup> groupFilter)
    {
        _data = data;
        _userFilter = userFilter;
        _groupFilter = groupFilter;
    }

    public async Task<ScimUser?> GetUserAsync(string id)
    {
        var entity = await _data.GetUserAsync(id);
        return entity?.ToScimUser();   // your extension method
    }

    public async Task<ScimListResponse<ScimUser>> GetUsersAsync(
        FilterExpression? filter, int startIndex, int count)
    {
        var query = _data.QueryUsers();
        if (filter != null)
            query = _userFilter.ApplyFilter(query, filter);

        var total = await query.CountAsync();
        var items = await query.Skip(startIndex - 1).Take(count).ToListAsync();
        return new ScimListResponse<ScimUser>
        {
            TotalResults = total,
            StartIndex = startIndex,
            Resources = items.Select(e => e.ToScimUser()).ToList()
        };
    }

    // ... implement remaining methods following the same pattern
}
```

---

## 6. Register services in Program.cs

```csharp
using EzSCIM.Controllers;
using EzSCIM.DataRepositories;
using EzSCIM.EfCore;
using EzSCIM.Filtering;
using EzSCIM.Repositories;
using EzSCIM.Services;
using EzSCIM.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// EF repository (CRUD)
builder.Services.AddScoped<IUserGroupDataRepository<AppUser, AppGroup>, AppUserGroupRepository>();

// Filter translators
builder.Services.AddScoped<IScimFilterTranslator<AppUser>, GenericScimFilterTranslator<AppUser>>();
builder.Services.AddScoped<IScimFilterTranslator<AppGroup>, GenericScimFilterTranslator<AppGroup>>();

// SCIM repository (entity ↔ ScimModel bridge)
builder.Services.AddScoped<IScimRepository, AppScimRepository>();

// Authentication
builder.Services.AddJwtTokenService();
builder.Services.AddAuthentication()
    .AddScheme<JwtBearerTokenAuthenticationOptions, JwtBearerTokenAuthenticationHandler>("Bearer", null);
builder.Services.AddAuthorization();

// SCIM controllers
builder.Services.AddScimControllers();
builder.Services.AddScimTokenGeneratorEndpoint(); // dev only

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## 7. Apply migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## Verify

=== "cURL"

    ```bash
    curl -H "Authorization: Bearer $TOKEN" https://localhost:7001/scim/Users
    curl -H "Authorization: Bearer $TOKEN" https://localhost:7001/scim/Schemas
    ```

=== "PowerShell"

    ```powershell
    $headers = @{ Authorization = "Bearer $TOKEN" }
    Invoke-RestMethod -Uri "https://localhost:7001/scim/Users" -Headers $headers
    Invoke-RestMethod -Uri "https://localhost:7001/scim/Schemas" -Headers $headers
    ```

---

## Next steps

- [IScimEntity interface →](./iscimentity.md)
- [EfScimRepositoryBase reference →](./efrepositorybase.md)
- [Multi-provider: SQL Server / PostgreSQL →](./multi-provider.md)
- [SCIM 2.0 attribute reference →](../iqueryable/scim-attributes.md)
- [Authentication setup →](../authentication.md)

