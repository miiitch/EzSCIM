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

