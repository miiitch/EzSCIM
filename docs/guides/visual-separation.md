## Interface Hierarchy - Visual Overview

### BEFORE: Flat Separated Interfaces

```
┌─────────────────────────┐  ┌─────────────────────────┐
│ IScimUserRepository     │  │ IScimGroupRepository    │
├─────────────────────────┤  ├─────────────────────────┤
│ • GetUserAsync()        │  │ • GetGroupAsync()       │
│ • GetUsersAsync()       │  │ • GetGroupsAsync()      │
│ • CreateUserAsync()     │  │ • CreateGroupAsync()    │
│ • UpdateUserAsync()     │  │ • UpdateGroupAsync()    │
│ • PatchUserAsync()      │  │ • PatchGroupAsync()     │
│ • DeleteUserAsync()     │  │ • DeleteGroupAsync()    │
└─────────────────────────┘  └─────────────────────────┘

❌ Groups without users has no meaning in SCIM
❌ Duplicated responsibility possible
❌ GroupsOnlyRepository was misleading
```

---

### AFTER: User-First Hierarchy

```
┌──────────────────────────────────────┐
│  IScimUserOnlyRepository<TUser>      │
├──────────────────────────────────────┤
│ • GetUserAsync()                     │
│ • GetUserByUserNameAsync()           │
│ • GetUsersAsync()                    │
│ • CreateUserAsync()                  │
│ • UpdateUserAsync()                  │
│ • PatchUserAsync()                   │
│ • DeleteUserAsync()                  │
└──────────────────┬───────────────────┘
                   │ inherits
                   ▼
┌──────────────────────────────────────┐
│  IScimUserGroupRepository<TUser,     │
│                           TGroup>    │
├──────────────────────────────────────┤
│ (all User methods inherited)         │
│ • GetGroupAsync()                    │
│ • GetGroupByDisplayNameAsync()       │
│ • GetGroupsAsync()                   │
│ • CreateGroupAsync()                 │
│ • UpdateGroupAsync()                 │
│ • PatchGroupAsync()                  │
│ • DeleteGroupAsync()                 │
└──────────────────┬───────────────────┘
                   │ inherits
                   ▼
┌──────────────────────────────────────┐
│  IScimRepository                     │
│  (backward-compatible alias)         │
│  = IScimUserGroupRepository          │
│    <ScimUser, ScimGroup>             │
└──────────────────────────────────────┘

✅ Groups always have users (SCIM spec)
✅ No duplication
✅ Clear hierarchy
✅ Backward compatible
```

---

### Data Layer Hierarchy

```
┌──────────────────────────────────────┐
│  IUserDataRepository<TUser>          │
├──────────────────────────────────────┤
│ • GetUserAsync(id)                   │
│ • QueryUsers()                       │
│ • CreateUserAsync(user)              │
│ • UpdateUserAsync(id, user)          │
│ • DeleteUserAsync(id)                │
└──────────────────┬───────────────────┘
                   │ inherits
                   ▼
┌──────────────────────────────────────┐
│  IUserGroupDataRepository            │
│    <TUser, TGroup>                   │
├──────────────────────────────────────┤
│ (all User methods inherited)         │
│ • GetGroupAsync(id)                  │
│ • QueryGroups()                      │
│ • CreateGroupAsync(group)            │
│ • UpdateGroupAsync(id, group)        │
│ • DeleteGroupAsync(id)               │
└──────────────────────────────────────┘
```

---

## Implementation Classes

```
┌──────────────────────────┐
│ UsersOnlyRepository      │
│ : IScimUserOnlyRepository│
├──────────────────────────┤
│ User operations only     │
│ (example implementation) │
└──────────────────────────┘

┌──────────────────────────┐
│ InMemoryScimRepository   │
│ : IScimRepository        │
├──────────────────────────┤
│ User + Group operations  │
│ (reference impl for dev) │
└──────────────────────────┘

┌────────────────────────────────────────────┐
│ ScimUserRepositoryAdapter<TUser>           │
│ : IScimUserOnlyRepository<ScimUser>        │
├────────────────────────────────────────────┤
│ Bridges IUserDataRepository to SCIM        │
│ Uses [ScimProperty] attribute mapping      │
└────────────────────────────────────────────┘

┌────────────────────────────────────────────┐
│ ScimUserGroupRepositoryAdapter<TUser,TGrp> │
│ : IScimUserGroupRepository<ScimUser,       │
│                             ScimGroup>     │
├────────────────────────────────────────────┤
│ Bridges IUserGroupDataRepository to SCIM   │
│ Uses [ScimProperty] attribute mapping      │
│ for both users and groups                  │
└────────────────────────────────────────────┘
```

---

## Deployment Scenarios

### Scenario 1: Users Only
```
Application
    │
    ├─> IScimUserOnlyRepository<ScimUser> (injected)
    │       │
    │       └─> UsersOnlyRepository (implementation)
    │               │
    │               └─ User endpoints only
    │
    └─> ResourceTypes
            └─> Only "User" type
```

### Scenario 2: Users + Groups (Current)
```
Application
    │
    ├─> IScimRepository (injected)
    │       │
    │       └─> InMemoryScimRepository (implementation)
    │               │
    │               ├─ User endpoints
    │               └─ Group endpoints
    │
    └─> ResourceTypes
            ├─> "User" type
            └─> "Group" type
```

---

## Switching Costs

| Operation | Cost | Effort |
|-----------|------|--------|
| **Keep Current (Users+Groups)** | 0 lines | None |
| **Switch to Users Only** | 3 lines | 5 minutes |

---

## Checklist

- ✅ User-first interface hierarchy implemented
- ✅ Example implementations provided
- ✅ Backward compatibility maintained
- ✅ SCIM standards compliance (groups reference users)
- ✅ Type safety achieved
- ✅ Documentation complete
- ✅ Ready for production

**Status:** 🟢 Production Ready
