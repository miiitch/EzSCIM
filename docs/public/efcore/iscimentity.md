# IScimEntity Interface

`IScimEntity` is a marker interface required by `EfScimRepositoryBase<TUser, TGroup, TContext>`.
Implement it on your EF Core entity classes to enable automatic Id generation and timestamp management.

**Namespace**: `EzSCIM.EfCore`  
**Package**: `EzSCIM.EfCore`

---

## Interface definition

```csharp
namespace EzSCIM.EfCore;

public interface IScimEntity
{
    /// <summary>
    /// Unique identifier. Auto-generated (GUID) by EfScimRepositoryBase if empty on creation.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// UTC timestamp set once when the entity is first created. Never updated afterwards.
    /// Mapped to SCIM meta.created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp updated on every write operation (create and update).
    /// Mapped to SCIM meta.lastModified.
    /// </summary>
    DateTime ModifiedAt { get; set; }
}
```

---

## Implement on your entity

```csharp
using EzSCIM.EfCore;

public class AppUser : IScimEntity
{
    // Required by IScimEntity
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    // Your domain properties
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}
```

---

## What `EfScimRepositoryBase` does with these fields

| Operation | `Id` | `CreatedAt` | `ModifiedAt` |
|---|---|---|---|
| `CreateUserAsync` / `CreateGroupAsync` | Auto-generates GUID if empty | Set to `DateTime.UtcNow` | Set to `DateTime.UtcNow` |
| `UpdateUserAsync` / `UpdateGroupAsync` | Preserved from DB | Preserved from DB | Set to `DateTime.UtcNow` |
| `DeleteUserAsync` / `DeleteGroupAsync` | Used to locate entity | — | — |

Auto-Id generation logic:

```csharp
if (string.IsNullOrEmpty(user.Id))
    user.Id = Guid.NewGuid().ToString();
```

---

## EF Core model configuration

Configure `Id` as the primary key. EF Core infers this by convention, but explicit
configuration is recommended:

```csharp
modelBuilder.Entity<AppUser>(e =>
{
    e.HasKey(u => u.Id);
    // Optional: store as uniqueidentifier (SQL Server) or uuid (PostgreSQL)
    // e.Property(u => u.Id).HasColumnType("uniqueidentifier");
});
```

---

## Notes

!!! info "Design decisions"
    - `Id` is a `string` (not `Guid`) to be compatible with SCIM's string ID requirement
      and to allow any format (GUID, int-as-string, opaque string, etc.)
    - `CreatedAt` and `ModifiedAt` use `DateTime` (UTC). Use `DateTimeOffset` if you need
      timezone offset storage.
    - The interface has no behavior — it is purely a compile-time constraint used by
      `EfScimRepositoryBase<TUser, TGroup, TContext>` generic constraints.

---

**Next**: [EfScimRepositoryBase reference →](./efrepositorybase.md)


