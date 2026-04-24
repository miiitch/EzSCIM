# Repository Interfaces

EzSCIM uses a two-level interface hierarchy. Choose the level that fits your implementation.

---

## Interface hierarchy

```
IUserDataRepository<TUser>
    └── IUserGroupDataRepository<TUser, TGroup>

IScimUserOnlyRepository<TUser>
    └── IScimUserGroupRepository<TUser, TGroup>
           └── IScimRepository  (concrete: IScimUserGroupRepository<ScimUser, ScimGroup>)
```

---

## Data layer interfaces (your implementation)

These interfaces connect your storage to EzSCIM. You implement them.

### `IUserDataRepository<TUser>`

Namespace: `EzSCIM.DataRepositories`

Use when you support **users only**.

```csharp
public interface IUserDataRepository<TUser> where TUser : class
{
    Task<TUser?> GetUserAsync(string id);

    /// <summary>
    /// Returns a queryable source. EzSCIM applies SCIM filters as LINQ expressions.
    /// Must return IQueryable backed by a real query provider (EF Core, etc.)
    /// or an in-memory source for simple cases.
    /// </summary>
    IQueryable<TUser> QueryUsers();

    Task<TUser> CreateUserAsync(TUser user);
    Task<TUser?> UpdateUserAsync(string id, TUser user);
    Task<bool> DeleteUserAsync(string id);
}
```

### `IUserGroupDataRepository<TUser, TGroup>`

Namespace: `EzSCIM.DataRepositories`

Use when you support **users and groups**. Inherits all user methods.

```csharp
public interface IUserGroupDataRepository<TUser, TGroup>
    : IUserDataRepository<TUser>
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

---

## SCIM layer interfaces (registered in DI)

These interfaces are used by EzSCIM controllers. You register them in DI — the adapters
or your custom implementation bridges your data layer to these.

### `IScimUserOnlyRepository<TUser>`

Namespace: `EzSCIM.Repositories`

```csharp
public interface IScimUserOnlyRepository<TUser> where TUser : ScimUser
{
    Task<TUser?> GetUserAsync(string id);
    Task<TUser?> GetUserByUserNameAsync(string userName);
    Task<ScimListResponse<TUser>> GetUsersAsync(FilterExpression? filter, int startIndex, int count);
    Task<TUser> CreateUserAsync(TUser user);
    Task<TUser?> UpdateUserAsync(string id, TUser user);
    Task<TUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest);
    Task<bool> DeleteUserAsync(string id);
}
```

### `IScimRepository`

Namespace: `EzSCIM.Repositories`

The main interface — combines users and groups with concrete `ScimUser` / `ScimGroup` types.

```csharp
public interface IScimRepository : IScimUserGroupRepository<ScimUser, ScimGroup> { }
```

Register this in DI and the controllers resolve it automatically.

---

## DI registration patterns

### Pattern A — Users only

```csharp
builder.Services.AddScoped<IUserDataRepository<MyUser>, MyUserRepository>();
builder.Services.AddScoped<IScimFilterTranslator<MyUser>, GenericScimFilterTranslator<MyUser>>();
builder.Services.AddScoped<IScimUserOnlyRepository<ScimUser>>(sp =>
    new ScimUserRepositoryAdapter<MyUser>(
        sp.GetRequiredService<IUserDataRepository<MyUser>>(),
        sp.GetRequiredService<IScimFilterTranslator<MyUser>>()));
```

### Pattern B — Users + Groups (recommended)

```csharp
builder.Services.AddScoped<IUserGroupDataRepository<MyUser, MyGroup>, MyRepository>();
builder.Services.AddScoped<IScimFilterTranslator<MyUser>, GenericScimFilterTranslator<MyUser>>();
builder.Services.AddScoped<IScimFilterTranslator<MyGroup>, GenericScimFilterTranslator<MyGroup>>();
builder.Services.AddScoped<IScimRepository, MyScimRepository>();
```

### Pattern C — Direct IScimRepository implementation

If you prefer to implement `IScimRepository` directly instead of using adapters:

```csharp
public class MyScimRepository : IScimRepository
{
    public Task<ScimUser?> GetUserAsync(string id) { ... }
    public Task<ScimListResponse<ScimUser>> GetUsersAsync(FilterExpression? filter, int startIndex, int count) { ... }
    // ... all other methods
}

builder.Services.AddScoped<IScimRepository, MyScimRepository>();
```

---

## `IScimFilterTranslator<T>`

Namespace: `EzSCIM.Filtering`

Translates a SCIM `FilterExpression` AST into an `IQueryable<T>` with LINQ `.Where()`.

The built-in `GenericScimFilterTranslator<T>` uses `[ScimProperty]` annotations on your
entity class to resolve attribute name → C# property mappings automatically.

```csharp
// Works automatically when TUser properties are annotated with [ScimProperty]
builder.Services.AddScoped<IScimFilterTranslator<MyUser>, GenericScimFilterTranslator<MyUser>>();
```

Custom translator (advanced):

```csharp
public class MyUserFilterTranslator : IScimFilterTranslator<MyUser>
{
    public IQueryable<MyUser> ApplyFilter(IQueryable<MyUser> query, FilterExpression filter)
    {
        // custom LINQ translation
        return query;
    }
}
```

---

## Notes on `IQueryable<T>`

- The `QueryUsers()` / `QueryGroups()` methods **must** return a real `IQueryable<T>` source
  (EF Core `DbSet.AsQueryable()`, Cosmos `Container.GetItemLinqQueryable<T>()`, etc.)
- EzSCIM calls `.Where(filterExpression)` → `.Skip()` → `.Take()` → `.ToListAsync()` on it
- In-memory lists (`list.AsQueryable()`) work for simple cases but load all rows first
- If your data source does not support `IQueryable`, implement `IScimRepository` directly
  and apply filters manually

---

**Next**: [SCIM filter syntax →](./filtering.md) | [SCIM 2.0 attributes →](./scim-attributes.md)

