# Fast Summary - SCIM Repository Mapping

**Version:** 1.1.0 (Users + Groups)  
**Date:** 2026-02-12

---

## In 30 seconds

Connect your existing repository (Users and Groups) to SCIM in **3 lines of code**:

```csharp
// 1) Annotate
[ScimProperty(ScimAttributeNames.User.UserName, "string", Required = true)]

// 2) Implement
public IQueryable<TUser> QueryUsers() => _context.Users;

// 3) Configure DI
services.AddScoped<IScimUserOnlyRepository<ScimUser>>(sp =>
    new ScimUserRepositoryAdapter<MyUser>(...));
```

**Result:** SCIM filters are translated to SQL automatically.

---

## What is included

### Users
- `IUserDataRepository<TUser>` interface
- AST -> `IQueryable` translator
- SCIM adapter layer
- Unit and integration test coverage

### Groups
- `IUserGroupDataRepository<TUser, TGroup>` interface
- AST -> `IQueryable` translator
- SCIM adapter layer
- Group flow test coverage

### Type-safe constants
- `ScimAttributeNames.User.*`
- `ScimAttributeNames.Group.*`
- `ScimAttributeNames.Common.*`
- IntelliSense and refactoring support

---

## Express guide

### Users (about 15 min)
1. Read [`quick-start-repository.md`](./quick-start-repository.md)
2. Copy and adapt the `CustomUser` example
3. Test with `GET /scim/Users?filter=active eq true`

### Groups (about 15 min)
1. Read [`groups-and-constants-extension.md`](./groups-and-constants-extension.md)
2. Copy and adapt the `CustomGroup` example
3. Test with `GET /scim/Groups?filter=displayName eq "Developers"`

---

## Complete example

```csharp
// Your model
public class Employee
{
    [ScimProperty(ScimAttributeNames.User.UserName, "string", Required = true)]
    public string Email { get; set; }

    [ScimProperty(ScimAttributeNames.User.Active, "boolean")]
    public bool IsActive { get; set; }
}

// Your repository
public class EmployeeRepo : IUserDataRepository<Employee>
{
    public IQueryable<Employee> QueryUsers() => _dbContext.Employees;
    // ... 4 additional methods
}

// Registration
services.AddScoped<IUserDataRepository<Employee>, EmployeeRepo>();
services.AddScoped<IScimFilterTranslator<Employee>, GenericScimFilterTranslator<Employee>>();
services.AddScoped<IScimUserOnlyRepository<ScimUser>>(sp =>
    new ScimUserRepositoryAdapter<Employee>(
        sp.GetRequiredService<IUserDataRepository<Employee>>(),
        sp.GetRequiredService<IScimFilterTranslator<Employee>>()));
```

Your SCIM API is now wired to your existing data model.

---

## Performance

```text
SCIM filter: active eq true and userName sw "john"
      ↓ automatic translation
SQL: WHERE IsActive = 1 AND Email LIKE 'john%'
```

This avoids loading all users into memory and keeps filtering at the data source level.

---

## Documentation map

| Need | Document | Time |
|---|---|---|
| Start quickly (Users) | `quick-start-repository.md` | 15 min |
| Start quickly (Groups) | `groups-and-constants-extension.md` | 15 min |
| Find everything | `repository-mapping-index.md` | 5 min |
| Deep dive | `repository-adapter-guide.md` | 30 min |

---

## Status

- Repository mapping and adapters are implemented
- User and group integration paths are available
- Test suites are in place and actively maintained

---

## Next step

Open [`quick-start-repository.md`](./quick-start-repository.md) and complete your first integration.
