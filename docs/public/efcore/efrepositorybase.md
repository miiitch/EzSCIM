# EfScimRepositoryBase Reference

`EfScimRepositoryBase<TUser, TGroup, TContext>` is an abstract EF Core base repository
that implements `IUserGroupDataRepository<TUser, TGroup>` with zero boilerplate.

**Namespace**: `EzSCIM.EfCore`  
**Package**: `EzSCIM.EfCore`

---

## Signature

```csharp
public abstract class EfScimRepositoryBase<TUser, TGroup, TContext>
    : IUserGroupDataRepository<TUser, TGroup>
    where TUser    : class, IScimEntity
    where TGroup   : class, IScimEntity
    where TContext : DbContext
```

### Generic constraints

| Parameter | Constraint | Description |
|---|---|---|
| `TUser` | `class, IScimEntity` | Your EF user entity |
| `TGroup` | `class, IScimEntity` | Your EF group entity |
| `TContext` | `DbContext` | Your EF DbContext subclass |

---

## Minimum implementation

```csharp
using EzSCIM.EfCore;
using Microsoft.EntityFrameworkCore;

public class AppUserGroupRepository
    : EfScimRepositoryBase<AppUser, AppGroup, AppDbContext>
{
    public AppUserGroupRepository(AppDbContext context) : base(context) { }

    protected override DbSet<AppUser>  Users  => Context.Users;
    protected override DbSet<AppGroup> Groups => Context.Groups;
}
```

Two lines of non-trivial code. Everything else is inherited.

---

## Provided methods

### User operations

| Method | Behavior |
|---|---|
| `GetUserAsync(string id)` | `Users.FindAsync(id)` |
| `QueryUsers()` | `Users.AsQueryable()` — used by filter translator |
| `CreateUserAsync(TUser)` | Generates GUID if `Id` is empty, sets `CreatedAt`/`ModifiedAt`, calls `SaveChangesAsync` |
| `UpdateUserAsync(string id, TUser)` | Loads existing, calls `OnBeforeUpdateUserAsync`, saves |
| `DeleteUserAsync(string id)` | Loads and removes, returns `false` if not found |

### Group operations

| Method | Behavior |
|---|---|
| `GetGroupAsync(string id)` | `Groups.FindAsync(id)` |
| `QueryGroups()` | `Groups.AsQueryable()` |
| `CreateGroupAsync(TGroup)` | Same as CreateUser — Guid, timestamps, save |
| `UpdateGroupAsync(string id, TGroup)` | Loads existing, calls `OnBeforeUpdateGroupAsync`, saves |
| `DeleteGroupAsync(string id)` | Loads and removes, returns `false` if not found |

---

## Extension hooks

Override these methods when your entity has JSON columns or navigation properties
that require manual handling during updates.

### `OnBeforeUpdateUserAsync`

Called by `UpdateUserAsync` **before** `SaveChangesAsync`. Default behavior:
copies all scalar columns via `CurrentValues.SetValues(updated)`.

```csharp
protected override Task OnBeforeUpdateUserAsync(AppUser existing, AppUser updated)
{
    // Default: copies all scalar columns
    Context.Entry(existing).CurrentValues.SetValues(updated);

    // Example: handle JSON column manually
    existing.EmailsJson = updated.EmailsJson;
    existing.PhoneNumbersJson = updated.PhoneNumbersJson;

    return Task.CompletedTask;
}
```

### `OnBeforeUpdateGroupAsync`

Same pattern for groups.

```csharp
protected override Task OnBeforeUpdateGroupAsync(AppGroup existing, AppGroup updated)
{
    Context.Entry(existing).CurrentValues.SetValues(updated);
    existing.MembersJson = updated.MembersJson;
    return Task.CompletedTask;
}
```

---

## Unique constraint handling

`CreateUserAsync` catches `DbUpdateException` and wraps unique-key violations
as `InvalidOperationException`. This is translated to `409 Conflict` by the SCIM
controller layer.

Supported databases:
- **SQL Server**: error codes 2601 and 2627
- **PostgreSQL**: SqlState `23505`
- **SQLite**: `UNIQUE constraint failed` message

```csharp
try
{
    await _repository.CreateUserAsync(entity);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
{
    // Returns 409 Conflict automatically — no action needed
}
```

---

## Protected field

```csharp
/// <summary>The underlying DbContext instance. Available to all overrides.</summary>
protected readonly TContext Context;
```

---

## Full example with JSON columns

```csharp
public class DemoUserGroupRepository
    : EfScimRepositoryBase<DemoUserEntity, DemoGroupEntity, AppDbContext>
{
    public DemoUserGroupRepository(AppDbContext ctx) : base(ctx) { }

    protected override DbSet<DemoUserEntity>  Users  => Context.Users;
    protected override DbSet<DemoGroupEntity> Groups => Context.Groups;

    protected override Task OnBeforeUpdateUserAsync(
        DemoUserEntity existing, DemoUserEntity updated)
    {
        // Use CurrentValues for scalars, but handle JSON columns explicitly
        Context.Entry(existing).CurrentValues.SetValues(updated);
        existing.EmailsJson        = updated.EmailsJson;
        existing.PhoneNumbersJson  = updated.PhoneNumbersJson;
        existing.AddressesJson     = updated.AddressesJson;
        return Task.CompletedTask;
    }

    protected override Task OnBeforeUpdateGroupAsync(
        DemoGroupEntity existing, DemoGroupEntity updated)
    {
        Context.Entry(existing).CurrentValues.SetValues(updated);
        existing.MembersJson = updated.MembersJson;
        return Task.CompletedTask;
    }
}
```

---

**Next**: [Multi-provider: SQL Server / PostgreSQL →](./multi-provider.md)  
**Back**: [IScimEntity →](./iscimentity.md)

