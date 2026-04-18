# Repository -> SCIM Mapping - Quick Overview

## What is it?

A complete system to connect your existing user repository to SCIM with automatic server-side filtering.

```text
Your SQL data -> IQueryable<TUser> -> SCIM filter -> optimized SQL -> SCIM response
```

---

## Quick start (3 steps)

### 1. Annotate your model

```csharp
public class MyUser
{
    [ScimProperty("userName", "string", Required = true)]
    public string Email { get; set; } = string.Empty;

    [ScimProperty("active", "boolean")]
    public bool IsActive { get; set; } = true;
}
```

### 2. Implement `IUserDataRepository`

```csharp
public class MyUserRepo : IUserDataRepository<MyUser>
{
    public IQueryable<MyUser> QueryUsers() => _context.Users;
    // + 4 other methods (Get/Create/Update/Delete)
}
```

### 3. Configure dependency injection

```csharp
services.AddScoped<IUserDataRepository<MyUser>, MyUserRepo>();
services.AddScoped<IScimFilterTranslator<MyUser>, GenericScimFilterTranslator<MyUser>>();
services.AddScoped<IScimUserOnlyRepository<ScimUser>>(sp =>
    new ScimUserRepositoryAdapter<MyUser>(
        sp.GetRequiredService<IUserDataRepository<MyUser>>(),
        sp.GetRequiredService<IScimFilterTranslator<MyUser>>()));
```

Done.

---

## Usage example

```http
GET /scim/Users?filter=active eq true and userName sw "john"
```

Automatically translated to:

```csharp
context.Users.Where(u => u.IsActive && u.Email.StartsWith("john"))
```

Executed as SQL:

```sql
SELECT * FROM Users WHERE IsActive = 1 AND Email LIKE 'john%'
```

---

## Provided components

| Component | File | Purpose |
|---|---|---|
| Repository interface | `IUserDataRepository.cs` | Contract for your data source |
| Generic translator | `GenericScimFilterTranslator.cs` | AST -> `IQueryable` (via attributes) |
| ScimUser translator | `ScimUserFilterTranslator.cs` | AST -> `IQueryable` (`ScimUser`) |
| Adapter | `ScimUserRepositoryAdapter.cs` | Repository <-> SCIM bridge |
| Example | `CustomUser.cs`, `CustomUserGroupRepository.cs` | Reference implementation |

---

## Test coverage

```text
26/26 tests passing
- ScimUserFilterTranslator: 13/13
- GenericScimFilterTranslator: 13/13
```

Covered operators:
- Comparison: `eq`, `ne`, `co`, `sw`, `ew`, `gt`, `lt`
- Logical: `and`, `or`, `not`
- Presence: `pr`
- Nested properties: `name.givenName`

---

## Full documentation

| Document | Description |
|---|---|
| `quick-start-repository.md` | 15-minute quick start |
| `repository-adapter-guide.md` | Full guide with examples |
| `repository-mapping-index.md` | Index and navigation |

---

## Benefits

- Performance: server-side SQL filtering, no full in-memory load
- Simplicity: 3 steps, around 15 minutes to integrate
- Flexibility: works with any annotated model
- Type safety: attribute-driven mapping and compile-time refactoring
- Compatibility: EF Core, Dapper, custom SQL repositories

---

## Architecture

```text
SCIM Client
    |
    | GET /scim/Users?filter=...
    v
UsersController
    | parse -> FilterExpression
    v
ScimUserRepositoryAdapter<TUser>
    |                    |
    |                    v
    v              IScimFilterTranslator<TUser>
IUserDataRepository<TUser>
```

---

## Next steps

1. Start with [`quick-start-repository.md`](./quick-start-repository.md)
2. Deep dive into [`repository-adapter-guide.md`](./repository-adapter-guide.md)
3. Review related design notes in [`interface-separation.md`](./interface-separation.md)
