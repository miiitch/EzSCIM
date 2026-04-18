# Complete Extension - Groups Support + SCIM Constants

**Date:** 2026-02-12  
**Extension:** Groups support + `ScimAttributeNames` constants

---

## New components

### 1) SCIM constants

**File:** `EzSCIM/Constants/ScimAttributeNames.cs`

Static class with constants for SCIM attributes:
- `ScimAttributeNames.User.*` - user attributes
- `ScimAttributeNames.Group.*` - group attributes
- `ScimAttributeNames.Common.*` - common attributes (`id`, `externalId`, etc.)
- `ScimAttributeNames.EnterpriseUser.*` - enterprise extension attributes
- `ScimAttributeNames.Operators.*` - filter operators

**Benefit:** type-safe, refactor-friendly, IntelliSense support.

```csharp
// Before
[ScimProperty("userName", "string", Required = true)]

// After
[ScimProperty(ScimAttributeNames.User.UserName, "string", Required = true)]
```

---

### 2) Groups support

#### Repository interface

**File:** `EzSCIM/DataRepositories/IUserGroupDataRepository.cs`

Groups are managed through `IUserGroupDataRepository<TUser, TGroup>`, which inherits from `IUserDataRepository<TUser>`:

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

> `IGroupDataRepository<TGroup>` was removed. In SCIM, group operations depend on user operations, so the combined contract is enforced at the type level.

#### Group filter translator

**File:** `EzSCIM/Filtering/ScimGroupFilterTranslator.cs`

Translates SCIM filters into LINQ expressions for `ScimGroup`:
- Full operator support (`eq`, `ne`, `co`, `sw`, `ew`, `pr`, `gt`, `lt`, `and`, `or`, `not`)
- Case-insensitive comparisons
- Uses `ScimAttributeNames` constants

#### User+group adapter

**File:** `EzSCIM/Repositories/ScimUserGroupRepositoryAdapter.cs`

Adapts `IUserGroupDataRepository<TUser, TGroup>` to `IScimUserGroupRepository<ScimUser, ScimGroup>`:
- Bidirectional mapping (`TUser` <-> `ScimUser`, `TGroup` <-> `ScimGroup`)
- Server-side filtering via `IQueryable`
- Pagination support
- Mapping driven by `[ScimProperty]` attributes

#### Example files

- `EzSCIM.EntraID.Demo/Examples/CustomGroup.cs` - annotated group model
- `EzSCIM.EntraID.Demo/Examples/CustomUserGroupRepository.cs` - combined repository example

---

### 3) Tests

Covered areas include:
- Group filter translation
- User/group adapter integration
- Constants usage in mappings
- Query + pagination behavior

---

## Typical integration flow

1. Define or annotate your custom user and group models.
2. Implement `IUserGroupDataRepository<TUser, TGroup>`.
3. Register translators and adapters in DI.
4. Validate with `/scim/Users` and `/scim/Groups` requests.

---

## Dependency injection example

```csharp
builder.Services.AddScoped<IUserGroupDataRepository<CustomUser, CustomGroup>, CustomUserGroupRepository>();
builder.Services.AddScoped<IScimFilterTranslator<CustomUser>, GenericScimFilterTranslator<CustomUser>>();
builder.Services.AddScoped<IScimFilterTranslator<CustomGroup>, GenericScimFilterTranslator<CustomGroup>>();

builder.Services.AddScoped<IScimUserGroupRepository<ScimUser, ScimGroup>>(sp =>
    new ScimUserGroupRepositoryAdapter<CustomUser, CustomGroup>(
        sp.GetRequiredService<IUserGroupDataRepository<CustomUser, CustomGroup>>(),
        sp.GetRequiredService<IScimFilterTranslator<CustomUser>>(),
        sp.GetRequiredService<IScimFilterTranslator<CustomGroup>>()));
```

---

## Why this extension matters

- Clear separation between SCIM contracts and domain models
- Reusable adapter model for existing repositories
- Safer attribute mapping through constants
- Full user+group support with consistent filtering behavior

---

## Related documentation

- [`quick-start-repository.md`](./quick-start-repository.md)
- [`repository-adapter-guide.md`](./repository-adapter-guide.md)
- [`repository-mapping-overview.md`](./repository-mapping-overview.md)
- [`interface-separation.md`](./interface-separation.md)
