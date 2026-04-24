# Multi-Provider: SQL Server / PostgreSQL / SQLite

`EfScimRepositoryBase` is provider-agnostic. The only provider-specific configuration
is in `DbContext.OnModelCreating` — typically the column type for JSON fields.

---

## Pattern: provider-agnostic base + provider-specific subclass

Create a base `DbContext` that configures keys, indexes, and constraints without
specifying column types. Then create a provider-specific subclass that adds the
appropriate column type for JSON columns.

### Base context (no column types)

```csharp
using Microsoft.EntityFrameworkCore;

public class AppDbContextBase : DbContext
{
    // Accept non-generic DbContextOptions so subclass options resolve correctly
    public AppDbContextBase(DbContextOptions options) : base(options) { }

    public DbSet<AppUser>  Users  { get; set; } = null!;
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
            // No column type here — defined in provider subclass
        });

        modelBuilder.Entity<AppGroup>(e =>
        {
            e.HasKey(g => g.Id);
            e.HasIndex(g => g.DisplayName).IsUnique();
            e.Property(g => g.DisplayName).IsRequired().HasMaxLength(256);
        });
    }
}
```

### SQL Server subclass

```csharp
using Microsoft.EntityFrameworkCore;

public class SqlServerAppDbContext : AppDbContextBase
{
    public SqlServerAppDbContext(DbContextOptions<SqlServerAppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SQL Server uses nvarchar(max) for JSON stored as text
        modelBuilder.Entity<AppUser>(e =>
        {
            e.Property(u => u.EmailsJson).HasColumnType("nvarchar(max)");
            e.Property(u => u.PhoneNumbersJson).HasColumnType("nvarchar(max)");
            e.Property(u => u.AddressesJson).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<AppGroup>(e =>
        {
            e.Property(g => g.MembersJson).HasColumnType("nvarchar(max)");
        });
    }
}
```

### PostgreSQL subclass

```csharp
using Microsoft.EntityFrameworkCore;

public class PostgreSqlAppDbContext : AppDbContextBase
{
    public PostgreSqlAppDbContext(DbContextOptions<PostgreSqlAppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PostgreSQL uses jsonb for indexed, binary JSON
        modelBuilder.Entity<AppUser>(e =>
        {
            e.Property(u => u.EmailsJson).HasColumnType("jsonb");
            e.Property(u => u.PhoneNumbersJson).HasColumnType("jsonb");
            e.Property(u => u.AddressesJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<AppGroup>(e =>
        {
            e.Property(g => g.MembersJson).HasColumnType("jsonb");
        });
    }
}
```

### SQLite subclass (testing / development)

```csharp
using Microsoft.EntityFrameworkCore;

public class SqliteAppDbContext : AppDbContextBase
{
    public SqliteAppDbContext(DbContextOptions<SqliteAppDbContext> options)
        : base(options) { }

    // SQLite stores JSON as TEXT — no override needed,
    // EF Core maps string properties to TEXT by default.
}
```

---

## DI registration per provider

### SQL Server (production / Azure SQL)

```csharp
// Using Aspire SQL Server integration
builder.AddSqlServerDbContext<SqlServerAppDbContext>("scimdb");

// Forward base type so repositories that inject AppDbContextBase resolve correctly
builder.Services.AddScoped<AppDbContextBase>(sp =>
    sp.GetRequiredService<SqlServerAppDbContext>());

builder.Services.AddScoped<
    IUserGroupDataRepository<AppUser, AppGroup>,
    AppUserGroupRepository>();
```

### PostgreSQL (e.g. integration tests with Testcontainers)

```csharp
builder.Services.AddDbContext<PostgreSqlAppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<AppDbContextBase>(sp =>
    sp.GetRequiredService<PostgreSqlAppDbContext>());

builder.Services.AddScoped<
    IUserGroupDataRepository<AppUser, AppGroup>,
    AppUserGroupRepository>();
```

### SQLite (unit tests / local development)

```csharp
builder.Services.AddDbContext<SqliteAppDbContext>(options =>
    options.UseSqlite("Data Source=:memory:"));

builder.Services.AddScoped<AppDbContextBase>(sp =>
    sp.GetRequiredService<SqliteAppDbContext>());
```

---

## Update your repository to use the base type

If your repository is typed to the base context, it works with any provider:

```csharp
public class AppUserGroupRepository
    : EfScimRepositoryBase<AppUser, AppGroup, AppDbContextBase>
{
    public AppUserGroupRepository(AppDbContextBase ctx) : base(ctx) { }

    protected override DbSet<AppUser>  Users  => Context.Users;
    protected override DbSet<AppGroup> Groups => Context.Groups;
}
```

---

## Migrations

Migrations are per-provider-context. Run them from the Demo/API project.

```bash
# SQL Server migrations
dotnet ef migrations add InitialCreate \
  --context SqlServerAppDbContext \
  --project YourApiProject \
  --output-dir Migrations/SqlServer

dotnet ef database update --context SqlServerAppDbContext

# PostgreSQL migrations
dotnet ef migrations add InitialCreate \
  --context PostgreSqlAppDbContext \
  --project YourApiProject \
  --output-dir Migrations/PostgreSql
```

> **Tip**: For integration tests using Testcontainers + PostgreSQL, call
> `await context.Database.EnsureCreatedAsync()` instead of `MigrateAsync()` —
> it creates the schema directly from the model without migrations.

---

## Provider comparison

| | SQL Server / Azure SQL | PostgreSQL | SQLite |
|---|---|---|---|
| JSON column type | `nvarchar(max)` | `jsonb` | `TEXT` |
| JSON indexing | No native JSON index | Native `jsonb` index | No |
| Unique constraint support | ✅ | ✅ | ✅ |
| Use case | Production (Azure) | Production / tests | Unit tests |
| EF Core provider package | `Microsoft.EntityFrameworkCore.SqlServer` | `Npgsql.EntityFrameworkCore.PostgreSQL` | `Microsoft.EntityFrameworkCore.Sqlite` |

---

**Back**: [EfScimRepositoryBase →](./efrepositorybase.md)  
**Next**: [Authentication →](../authentication.md)

