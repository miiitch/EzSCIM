# Architecture

## Multi-Provider Data Layer

The data layer uses a **provider-agnostic shared library** pattern to avoid
duplicating entities, extensions, and repositories between the Demo API and integration tests.

### Library structure

```
EzSCIM.Demo.Data/                      ← Shared library (no DB provider dependency)
├── ScimDbContextBase.cs               ← Abstract base DbContext (keys, indexes only)
├── DemoScimRepository.cs              ← IScimRepository implementation
├── DemoUserEntityExtensions.cs        ← DemoUserEntity ↔ ScimUser conversions
├── DemoGroupEntityExtensions.cs       ← DemoGroupEntity ↔ ScimGroup conversions
├── Entities/
│   ├── DemoUserEntity.cs              ← IScimEntity, no [Column] type attributes
│   ├── DemoGroupEntity.cs
│   └── MultiValuedAttributeHelper.cs  ← JSON serialization helpers
└── Repositories/
    └── DemoUserGroupRepository.cs     ← EfScimRepositoryBase<..., ScimDbContextBase>
```

### Provider-specific subclasses

```
ScimDbContextBase  (EzSCIM.Demo.Data — no column types)
    │
    ├── DemoScimDbContext              (EzSCIM.EntraID.Demo)
    │   └── nvarchar(max) for JSON    ← SQL Server / Azure SQL
    │
    └── PostgreSqlScimDbContext        (EzSCIM.IntegrationTests)
        └── jsonb for JSON             ← PostgreSQL (Testcontainers)
```

### DI registration pattern

`DemoUserGroupRepository` depends on `ScimDbContextBase` (not the concrete provider type).
DI forwards the base type to the concrete registered subclass:

```csharp
// SQL Server (Demo API / Program.cs)
builder.AddSqlServerDbContext<DemoScimDbContext>("scimdb");
builder.Services.AddScoped<ScimDbContextBase>(sp => sp.GetRequiredService<DemoScimDbContext>());

// PostgreSQL (IntegrationTests / ScimWebApplicationFactory.cs)
services.AddDbContext<PostgreSqlScimDbContext>(options => options.UseNpgsql(connStr));
services.AddScoped<ScimDbContextBase>(sp => sp.GetRequiredService<PostgreSqlScimDbContext>());
```

---

## Key design decisions

### Why `ScimDbContextBase` instead of a single context?

- Eliminates ~700 lines of duplicated code between Demo and IntegrationTests
- JSON column types differ: SQL Server uses `nvarchar(max)`, PostgreSQL uses `jsonb`
- Tests can use a real PostgreSQL via Testcontainers while the Demo uses SQL Server
- Both share the same entities, migrations schema (minus column types), and repository logic

### Why keep `DemoUserGroupRepository` typed to `ScimDbContextBase`?

- Makes the repository provider-agnostic
- DI resolves the correct concrete context at runtime
- No code duplication across providers

### Why separate `DemoScimRepository` from `DemoUserGroupRepository`?

- `DemoUserGroupRepository` handles EF CRUD (Create/Read/Update/Delete at entity level)
- `DemoScimRepository` handles SCIM-level operations (entity ↔ ScimModel conversion,
  filter translation, PATCH application)
- Clean separation of concerns — the EF layer has no SCIM model dependency

---

## EzSCIM core library (`EzSCIM`)

The core library provides:

| Component | Description |
|---|---|
| `UsersController` | Handles SCIM `/scim/Users` CRUD + PATCH |
| `GroupsController` | Handles SCIM `/scim/Groups` CRUD + PATCH |
| `SchemasController` | Handles `/scim/Schemas`, `/scim/ServiceProviderConfig` |
| `ScimPatchApplier` | Applies `PatchOp` operations to entities |
| `GenericScimFilterTranslator<T>` | Translates SCIM filter AST → LINQ `.Where()` |
| `JwtTokenService` | Generates and validates JWT tokens |
| `JwtBearerTokenAuthenticationHandler` | ASP.NET Core auth handler |
| `ScimSchemaGenerator` | Generates SCIM schemas from `[ScimProperty]` annotations |

### Controllers are registered via extension method

```csharp
// Registers all SCIM controllers with default routes
builder.Services.AddScimControllers();

// Optional: exposes GET /scim/auth/token (development only)
builder.Services.AddScimTokenGeneratorEndpoint();
```

---

## Operation Observability (`IScimOperationCallbacks`)

Optional, additive observability for host applications that want to know when SCIM operations
happen (last read, last update, etc.) and capture errors for their own logging/monitoring — without
replacing the standard `ILogger<T>` logging already performed inside the controllers.

| Component | Description |
|---|---|
| `Observability.IScimOperationCallbacks` | Interface a host implements: `OnOperationCompletedAsync` and `OnErrorAsync` |
| `Observability.ScimOperationCallbacksBase` | Convenience base class with virtual no-op methods |
| `Observability.ScimOperationContext` | Resource/operation kind, resource id, result, duration, timestamp |
| `Repositories.ObservableScimRepository` (internal) | Decorator around `IScimRepository` that invokes registered callbacks |
| `AddScimOperationCallback<T>()` / `AddScimOperationCallback(instance)` | DI registration, following the `AddJwtTokenService` extension-method pattern |

**100% opt-in.** `ObservableScimRepository` is only created if a host calls
`AddScimOperationCallback(...)` at least once, *after* registering `IScimRepository`. Hosts that never
call it get their own `IScimRepository` implementation untouched — no decorator, no overhead. Multiple
callbacks can be registered; each is notified independently, and an exception thrown by a callback is
logged and swallowed so it can never break the SCIM request pipeline. The decorator never alters the
wrapped repository's result or exception — on error it always rethrows the original exception unchanged
(so e.g. `ScimUsersController`'s `catch (InvalidOperationException ex) when (...)` on `CreateUser` still
behaves identically).

> **Not to be confused with** `EfScimRepositoryBase.OnBeforeUpdateUserAsync` / `OnBeforeUpdateGroupAsync`
> below — those are inheritance-based template-method hooks for customizing EF entity merging before
> `SaveChangesAsync()`, a completely different mechanism from this DI-registered observer pattern.

---

## EzSCIM.EfCore library

Thin abstraction layer on top of EF Core:

| Component | Description |
|---|---|
| `IScimEntity` | Marker interface requiring `Id`, `CreatedAt`, `ModifiedAt` |
| `EfScimRepositoryBase<TUser, TGroup, TContext>` | Abstract CRUD base with hooks |

No SCIM model dependency — depends only on `EzSCIM.DataRepositories` interfaces.

---

## Request flow (HTTP → DB)

```
HTTP Request: PATCH /scim/Users/{id}
    │
    ▼
UsersController.PatchUser(id, patchRequest)
    │
    ▼
IScimRepository.PatchUserAsync(id, patchRequest)      ← DemoScimRepository
    │
    ├── GetUserAsync(id)                               ← fetches entity via DemoUserGroupRepository
    │       └── Users.FindAsync(id)                    ← EF Core → DB
    │
    ├── ScimPatchApplier.Apply(entity, patchRequest)  ← applies operations to entity
    │
    └── UpdateUserAsync(id, entity)                   ← saves via DemoUserGroupRepository
            └── OnBeforeUpdateUserAsync(...)           ← copies JSON columns
                SaveChangesAsync()                     ← EF Core → DB
```

---

**See also**: [testing.md](./testing.md) | [development-setup.md](./development-setup.md)

