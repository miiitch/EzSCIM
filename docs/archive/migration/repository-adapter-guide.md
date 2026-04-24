# SCIM Integration Guide with a Custom Repository

## Overview

This guide explains how to integrate an existing user management system with SCIM using:

1. `IUserDataRepository<TUser>` - data access contract
2. `IScimFilterTranslator<TUser>` - AST -> `IQueryable` translation
3. `ScimUserRepositoryAdapter<TUser>` - SCIM adapter layer

## Architecture

```text
SCIM Controller
    |
    v
IScimUserOnlyRepository<ScimUser>
    |
    v
ScimUserRepositoryAdapter<TUser>
    |                          |
    v                          v
IUserDataRepository<TUser>     IScimFilterTranslator<TUser>
    |                          |
    v                          v
Your data source               AST -> Expression<Func<TUser, bool>>
(EF Core / SQL / Dapper)       (server-side filtering)
```

---

## Step 1 - Annotate your user model

Add `[ScimProperty]` attributes to the fields you want to expose.

```csharp
using ScimAPI.Attributes;

public class MyUser
{
    public string Id { get; set; } = string.Empty;

    [ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
    public string Username { get; set; } = string.Empty;

    [ScimProperty("email", "string")]
    public string EmailAddress { get; set; } = string.Empty;

    [ScimProperty("givenName", "string")]
    public string FirstName { get; set; } = string.Empty;

    [ScimProperty("familyName", "string")]
    public string LastName { get; set; } = string.Empty;

    [ScimProperty("displayName", "string")]
    public string DisplayName { get; set; } = string.Empty;

    [ScimProperty("active", "boolean")]
    public bool IsActive { get; set; } = true;

    [ScimProperty("title", "string")]
    public string JobTitle { get; set; } = string.Empty;

    // Internal fields (not exposed to SCIM)
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Common attributes

| SCIM attribute | Type | Notes |
|---|---|---|
| `userName` | `string` | Required unique identifier |
| `email` | `string` | Email address |
| `givenName` | `string` | First name |
| `familyName` | `string` | Last name |
| `displayName` | `string` | Display name |
| `active` | `boolean` | Active/inactive status |
| `title` | `string` | Job title |
| `externalId` | `string` | External source identifier |
| `name.givenName` | `string` | Nested attribute |
| `name.familyName` | `string` | Nested attribute |

---

## Step 2 - Implement `IUserDataRepository<TUser>`

Expose your data as `IQueryable` so filtering can run server-side.

```csharp
public class MyUserRepository : IUserDataRepository<MyUser>
{
    private readonly AppDbContext _db;

    public MyUserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<MyUser?> GetUserAsync(string id)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public IQueryable<MyUser> QueryUsers()
        => _db.Users.AsQueryable();

    public async Task<MyUser> CreateUserAsync(MyUser user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<MyUser?> UpdateUserAsync(string id, MyUser user)
    {
        var existing = await GetUserAsync(id);
        if (existing == null) return null;

        existing.Username = user.Username;
        existing.EmailAddress = user.EmailAddress;
        existing.FirstName = user.FirstName;
        existing.LastName = user.LastName;
        existing.DisplayName = user.DisplayName;
        existing.IsActive = user.IsActive;
        existing.JobTitle = user.JobTitle;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var existing = await GetUserAsync(id);
        if (existing == null) return false;

        _db.Users.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
```

---

## Step 3 - Register DI

```csharp
builder.Services.AddScoped<IUserDataRepository<MyUser>, MyUserRepository>();
builder.Services.AddScoped<IScimFilterTranslator<MyUser>, GenericScimFilterTranslator<MyUser>>();

builder.Services.AddScoped<IScimUserOnlyRepository<ScimUser>>(sp =>
    new ScimUserRepositoryAdapter<MyUser>(
        sp.GetRequiredService<IUserDataRepository<MyUser>>(),
        sp.GetRequiredService<IScimFilterTranslator<MyUser>>()));
```

If your app uses `IScimRepository`, add a wrapper registration that delegates to your user-only repository.

---

## How filtering works

1. The controller parses the filter string into a `FilterExpression`.
2. The adapter asks `IScimFilterTranslator<TUser>` to build a LINQ expression.
3. The expression is applied to `QueryUsers()`.
4. The provider (EF Core, SQL translator) executes the query server-side.

Example:

```text
SCIM: userName sw "john" and active eq true
LINQ: u => u.Username.StartsWith("john") && u.IsActive
SQL : WHERE Username LIKE 'john%' AND IsActive = 1
```

---

## Troubleshooting

### No results returned

- Verify `[ScimProperty]` names match the filter fields.
- Verify your translator supports the requested operators.
- Ensure `QueryUsers()` returns an `IQueryable` from your provider.

### Filters evaluated in memory

- Check that `QueryUsers()` does not call `ToList()` before filtering.
- Keep the query provider active until final projection.

### Nested attributes not mapped

- Use dot notation (`name.givenName`) consistently.
- Ensure mappings exist for nested paths in your translator.

---

## Validation checklist

- [ ] Model is annotated with `[ScimProperty]`
- [ ] `IUserDataRepository<TUser>` is implemented
- [ ] DI registrations are correct
- [ ] `GET /scim/Users` works
- [ ] Filtered queries return correct results
- [ ] `POST`, `PUT`, `PATCH`, and `DELETE` flows are validated

---

## Related documentation

- [`quick-start-repository.md`](./quick-start-repository.md)
- [`repository-mapping-overview.md`](./repository-mapping-overview.md)
- [`groups-and-constants-extension.md`](./groups-and-constants-extension.md)
- [`interface-separation.md`](./interface-separation.md)
