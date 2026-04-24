# Plan: Refactoring Repository Interfaces (SCIM + Data) with Symmetric Naming

> ✅ **COMPLETED** — 2026-02-24. All 14 steps applied successfully. Solution builds with 0 errors.

Refactoring en 2 couches : **Data layer** (`IUserDataRepository` renommé symétriquement + `IUserGroupDataRepository` hérite) et **SCIM layer** (`IScimUserOnlyRepository` + `IScimUserGroupRepository` hérite). Les méthodes user dans `IUserDataRepository` sont renommées avec préfixe `User` (`GetUserAsync`, `QueryUsers`, etc.) pour la symétrie avec les méthodes `Group` dans `IUserGroupDataRepository`. Suppression des classes "group only". Fusion des implémentations EF et Demo.

## Steps

### Step 1 — Rename methods in `IUserDataRepository<TUser>`

**File:** `EzSCIM/DataRepositories/IUserDataRepository.cs`

Rename all methods with explicit `User` prefix for symmetry:

- `GetAsync(string id)` → `GetUserAsync(string id)`
- `Query()` → `QueryUsers()`
- `CreateAsync(TUser user)` → `CreateUserAsync(TUser user)`
- `UpdateAsync(string id, TUser user)` → `UpdateUserAsync(string id, TUser user)`
- `DeleteAsync(string id)` → `DeleteUserAsync(string id)`

### Step 2 — Replace `IGroupDataRepository<TGroup>` with `IUserGroupDataRepository<TUser, TGroup>`

**File:** `EzSCIM/DataRepositories/IGroupDataRepository.cs` → rename to `IUserGroupDataRepository.cs`

New interface inherits from `IUserDataRepository<TUser>` and adds group methods:

```csharp
public interface IUserGroupDataRepository<TUser, TGroup> : IUserDataRepository<TUser>
    where TUser : class
    where TGroup : class
{
    Task<TGroup?> GetGroupAsync(string id);
    IQueryable<TGroup> QueryGroups();
    Task<TGroup> CreateGroupAsync(TGroup group);
    Task<TGroup?> UpdateGroupAsync(string id, TGroup group);
    Task<bool> DeleteGroupAsync(string id);
}
```

Delete old `IGroupDataRepository.cs`.

### Step 3 — Transform SCIM interfaces in `IScimRepository.cs`

**File:** `EzSCIM/Repositories/IScimRepository.cs`

- Rename `IScimUserRepository<TUser>` → `IScimUserOnlyRepository<TUser>` (same methods, just rename)
- Rename `IScimGroupRepository<TGroup>` → `IScimUserGroupRepository<TUser, TGroup> : IScimUserOnlyRepository<TUser>` with constraints `where TUser : ScimUser where TGroup : ScimGroup`
- Update `IScimRepository` to inherit only from `IScimUserGroupRepository<ScimUser, ScimGroup>`

### Step 4 — Update `ScimUserRepositoryAdapter<TUser>`

**File:** `EzSCIM/Repositories/ScimUserRepositoryAdapter.cs`

- Implement `IScimUserOnlyRepository<ScimUser>` instead of `IScimUserRepository<ScimUser>`
- Update all internal calls to renamed data repo methods:
  - `_dataRepository.GetAsync(id)` → `_dataRepository.GetUserAsync(id)`
  - `_dataRepository.Query()` → `_dataRepository.QueryUsers()`
  - `_dataRepository.CreateAsync(...)` → `_dataRepository.CreateUserAsync(...)`
  - `_dataRepository.UpdateAsync(...)` → `_dataRepository.UpdateUserAsync(...)`
  - `_dataRepository.DeleteAsync(...)` → `_dataRepository.DeleteUserAsync(...)`

### Step 5 — Transform `ScimGroupRepositoryAdapter` → `ScimUserGroupRepositoryAdapter<TUser, TGroup>`

**File:** `EzSCIM/Repositories/ScimGroupRepositoryAdapter.cs` → rename to `ScimUserGroupRepositoryAdapter.cs`

- Implement `IScimUserGroupRepository<ScimUser, ScimGroup>`
- Inject `IUserGroupDataRepository<TUser, TGroup>` + both `IScimFilterTranslator<TUser>` and `IScimFilterTranslator<TGroup>`
- Contains both `UserMapper<TUser>` and `GroupMapper<TGroup>`
- User methods delegate to data repo user methods, group methods delegate to data repo group methods
- Move `GroupMapper<TGroup>` into this file (it was in `ScimGroupRepositoryAdapter.cs`)
- The `UserMapper<TUser>` class stays in `ScimUserRepositoryAdapter.cs` (shared via `internal` visibility within the assembly)

Delete old `ScimGroupRepositoryAdapter.cs`.

### Step 6 — Update `UsersOnlyRepository`

**File:** `EzSCIM/Repositories/UsersOnlyRepository.cs`

- Change `IScimUserRepository<ScimUser>` → `IScimUserOnlyRepository<ScimUser>`
- Update XML doc comments accordingly

### Step 7 — Delete `GroupsOnlyRepository`

**File:** `EzSCIM/Repositories/GroupsOnlyRepository.cs` → **DELETE**

A group-only provider has no meaning in SCIM (groups always reference users).

### Step 8 — `InMemoryScimRepository` (no changes)

**File:** `EzSCIM/Repositories/InMemoryScimRepository.cs`

No changes needed — it implements `IScimRepository` which now inherits from `IScimUserGroupRepository<ScimUser, ScimGroup>` which inherits from `IScimUserOnlyRepository<ScimUser>`. All method signatures at the SCIM level are unchanged.

### Step 9 — Merge EF Data Repositories into `EfUserGroupDataRepository`

**Files:**
- `EzSCIM.IntegrationTests/Data/Repositories/EfUserDataRepository.cs` → **DELETE**
- `EzSCIM.IntegrationTests/Data/Repositories/EfGroupDataRepository.cs` → **DELETE**
- Create `EzSCIM.IntegrationTests/Data/Repositories/EfUserGroupDataRepository.cs`

New class implements `IUserGroupDataRepository<UserEntity, GroupEntity>`:
- User methods (`GetUserAsync`, `QueryUsers`, `CreateUserAsync`, `UpdateUserAsync`, `DeleteUserAsync`) → delegate to `_context.Users`
- Group methods (`GetGroupAsync`, `QueryGroups`, `CreateGroupAsync`, `UpdateGroupAsync`, `DeleteGroupAsync`) → delegate to `_context.Groups`
- Constructor takes `ScimDbContext`

### Step 10 — Merge Demo Custom Repositories into `CustomUserGroupRepository`

**Files:**
- `EzSCIM.EntraID.Demo/Examples/CustomUserRepository.cs` → **DELETE**
- `EzSCIM.EntraID.Demo/Examples/CustomGroupRepository.cs` → **DELETE**
- Create `EzSCIM.EntraID.Demo/Examples/CustomUserGroupRepository.cs`

New class implements `IUserGroupDataRepository<CustomUser, CustomGroup>`:
- In-memory dictionaries for both users and groups
- User methods operate on `_users` dictionary
- Group methods operate on `_groups` dictionary

### Step 11 — Update `JsonUserRepositoryAdapter`

**File:** `EzSCIM.IntegrationTests/Data/Repositories/JsonUserRepositoryAdapter.cs`

- Change `IScimUserRepository<ScimUser>` → `IScimUserOnlyRepository<ScimUser>`
- Update internal data repo method calls:
  - `_dataRepo.GetAsync(id)` → `_dataRepo.GetUserAsync(id)`
  - `_dataRepo.Query()` → `_dataRepo.QueryUsers()`
  - `_dataRepo.CreateAsync(...)` → `_dataRepo.CreateUserAsync(...)`
  - `_dataRepo.UpdateAsync(...)` → `_dataRepo.UpdateUserAsync(...)`
  - `_dataRepo.DeleteAsync(...)` → `_dataRepo.DeleteUserAsync(...)`

### Step 12 — Update `ScimWebApplicationFactory`

**File:** `EzSCIM.IntegrationTests/ScimWebApplicationFactory.cs`

DI registration changes:

- Replace `services.AddScoped<IUserDataRepository<UserEntity>, EfUserDataRepository>()` + `services.AddScoped<IGroupDataRepository<GroupEntity>, EfGroupDataRepository>()` with single `services.AddScoped<IUserGroupDataRepository<UserEntity, GroupEntity>, EfUserGroupDataRepository>()`
- Also register `IUserDataRepository<UserEntity>` pointing to the same instance (for `JsonUserRepositoryAdapter` which only needs user data):
  ```csharp
  services.AddScoped<IUserDataRepository<UserEntity>>(sp =>
      sp.GetRequiredService<IUserGroupDataRepository<UserEntity, GroupEntity>>());
  ```
- Replace `IScimUserRepository<ScimUser>` → `IScimUserOnlyRepository<ScimUser>`
- Replace `IScimGroupRepository<ScimGroup>` → `IScimUserGroupRepository<ScimUser, ScimGroup>` (using `ScimGroupRepositoryAdapter` → `ScimUserGroupRepositoryAdapter`)
- Update `CompositeScimRepository`:
  - Change field types: `IScimUserRepository<ScimUser>` → `IScimUserOnlyRepository<ScimUser>`, `IScimGroupRepository<ScimGroup>` → `IScimUserGroupRepository<ScimUser, ScimGroup>`
  - Update constructor parameter types accordingly

### Step 13 — Update `RepositoryAdapterIntegrationTests`

**File:** `EzSCIM.UnitTests/Integration/RepositoryAdapterIntegrationTests.cs`

- Update all calls to `_dataRepository.CreateAsync(...)` → `_dataRepository.CreateUserAsync(...)`
- The `CustomUserRepository` type used here will now be `CustomUserGroupRepository` (or keep using `IUserDataRepository<CustomUser>` if the test only tests user operations — in that case `CustomUserGroupRepository` implements `IUserDataRepository<CustomUser>` via inheritance so it still works)

### Step 14 — Update Documentation

Update all markdown files that reference old interface/class names. **Skip `docs/archive/` files** (historical).

#### Files to update:

| File | Changes |
|------|---------|
| `docs/migration/interface-separation.md` | Rewrite interface hierarchy section: `IScimUserRepository` → `IScimUserOnlyRepository`, `IScimGroupRepository` → `IScimUserGroupRepository`, remove `GroupsOnlyRepository` references, explain new inheritance |
| `docs/migration/groups-and-constants-extension.md` | `IGroupDataRepository` → `IUserGroupDataRepository`, `IScimGroupRepository` → `IScimUserGroupRepository`, update code samples |
| `docs/migration/mapping-readme.md` | `IGroupDataRepository` → `IUserGroupDataRepository`, `IScimUserRepository` → `IScimUserOnlyRepository`, update DI registration samples |
| `docs/migration/README.md` | `IGroupDataRepository` → `IUserGroupDataRepository`, update checklist |
| `docs/migration/quick-start-repository.md` | `IScimUserRepository` → `IScimUserOnlyRepository`, `_dataRepository.Query()` → `_dataRepository.QueryUsers()`, update code samples |
| `docs/migration/repository-adapter-guide.md` | `IScimUserRepository` → `IScimUserOnlyRepository`, update adapter diagram |
| `docs/migration/repository-mapping-overview.md` | `IScimUserRepository` → `IScimUserOnlyRepository`, update DI sample |
| `docs/schema/extension-guide.md` | `IScimUserRepository` → `IScimUserOnlyRepository`, `IScimGroupRepository` → `IScimUserGroupRepository`, update code samples |
| `docs/status/implementation-status.md` | Update interface names, remove `GroupsOnlyRepository`, update ASCII diagram |
| `docs/guides/provider-modes.md` | Remove groups-only mode, `IScimGroupRepository` → `IScimUserGroupRepository`, `GroupsOnlyRepository` removed, update comparison table |
| `docs/guides/visual-separation.md` | Update ASCII diagrams with new interface names |
| `docs/status/session-summary.md` | `IGroupDataRepository` → `IUserGroupDataRepository` |
| `docs/guides/next-tasks-checklist.md` | `GroupsOnlyRepository` → removed |

## Execution Order

1. **Interfaces first** (Steps 1–3) — establish the new contracts
2. **Core implementations** (Steps 4–8) — update/create adapters, delete `GroupsOnlyRepository`
3. **Test infrastructure** (Steps 9–13) — merge EF repos, update DI, fix tests
4. **Documentation** (Step 14) — update all markdown references
5. **Build & verify** — compile after each major step

## Interface Hierarchy Summary (After Refactoring)

### Data Layer

```
IUserDataRepository<TUser>
    ├── GetUserAsync(string id)
    ├── QueryUsers()
    ├── CreateUserAsync(TUser)
    ├── UpdateUserAsync(string id, TUser)
    └── DeleteUserAsync(string id)
        │
        ▼
IUserGroupDataRepository<TUser, TGroup> : IUserDataRepository<TUser>
    ├── (inherits all User methods)
    ├── GetGroupAsync(string id)
    ├── QueryGroups()
    ├── CreateGroupAsync(TGroup)
    ├── UpdateGroupAsync(string id, TGroup)
    └── DeleteGroupAsync(string id)
```

### SCIM Layer

```
IScimUserOnlyRepository<TUser>           (was IScimUserRepository<TUser>)
    ├── GetUserAsync(string id)
    ├── GetUserByUserNameAsync(string userName)
    ├── GetUsersAsync(filter, startIndex, count)
    ├── CreateUserAsync(TUser)
    ├── UpdateUserAsync(string id, TUser)
    ├── PatchUserAsync(string id, ScimPatchRequest)
    └── DeleteUserAsync(string id)
        │
        ▼
IScimUserGroupRepository<TUser, TGroup> : IScimUserOnlyRepository<TUser>
    ├── (inherits all User methods)
    ├── GetGroupAsync(string id)
    ├── GetGroupByDisplayNameAsync(string displayName)
    ├── GetGroupsAsync(filter, startIndex, count)
    ├── CreateGroupAsync(TGroup)
    ├── UpdateGroupAsync(string id, TGroup)
    ├── PatchGroupAsync(string id, ScimPatchRequest)
    └── DeleteGroupAsync(string id)
        │
        ▼
IScimRepository : IScimUserGroupRepository<ScimUser, ScimGroup>
    (backward-compatible alias, no additional methods)
```

### Implementation Classes

```
UsersOnlyRepository : IScimUserOnlyRepository<ScimUser>
    (example for user-only providers)

InMemoryScimRepository : IScimRepository
    (reference implementation for dev/testing, handles both users and groups)

ScimUserRepositoryAdapter<TUser> : IScimUserOnlyRepository<ScimUser>
    (bridges IUserDataRepository<TUser> to SCIM user interface)

ScimUserGroupRepositoryAdapter<TUser, TGroup> : IScimUserGroupRepository<ScimUser, ScimGroup>
    (bridges IUserGroupDataRepository<TUser, TGroup> to SCIM user+group interface)

EfUserGroupDataRepository : IUserGroupDataRepository<UserEntity, GroupEntity>
    (EF Core implementation for integration tests)

CustomUserGroupRepository : IUserGroupDataRepository<CustomUser, CustomGroup>
    (in-memory example in Demo project)
```

### Deleted Classes

- `GroupsOnlyRepository` — a group without users has no meaning in SCIM
- `ScimGroupRepositoryAdapter<TGroup>` — replaced by `ScimUserGroupRepositoryAdapter<TUser, TGroup>`
- `EfUserDataRepository` — merged into `EfUserGroupDataRepository`
- `EfGroupDataRepository` — merged into `EfUserGroupDataRepository`
- `CustomUserRepository` — merged into `CustomUserGroupRepository`
- `CustomGroupRepository` — merged into `CustomUserGroupRepository`
- `IGroupDataRepository<TGroup>` — replaced by `IUserGroupDataRepository<TUser, TGroup>`

