﻿# Repository Integration & Migration

Documentation for integrating SCIM with your data repositories and migrating existing systems.

## 📖 Quick Navigation

### Getting Started
- **[Quick Start Repository Integration](./quick-start-repository.md)** - 15-minute integration guide
- **[Repository Mapping Overview](./repository-mapping-overview.md)** - System overview
- **[Repository Adapter Guide](./repository-adapter-guide.md)** - Complete implementation guide

### Reference
- **[Repository Mapping Index](./repository-mapping-index.md)** - Navigation index
- **[Interface Separation](./interface-separation.md)** - Design rationale
- **[Groups and Constants Extension](./groups-and-constants-extension.md)** - Groups implementation

---

## 🎯 Implementation Paths

### Quick Path (15 minutes)
1. Read [Quick Start Repository Integration](./quick-start-repository.md)
2. Annotate your model with `[ScimProperty]`
3. Implement `IUserDataRepository<T>` or `IUserGroupDataRepository<T, TGroup>`
4. Configure dependency injection

### Standard Path (1 hour)
1. Complete quick path
2. Review [Repository Mapping Overview](./repository-mapping-overview.md)
3. Study [Repository Adapter Guide](./repository-adapter-guide.md)
4. Implement error handling and pagination

### Advanced Path (2-3 hours)
1. Complete standard path
2. Read [Interface Separation](./interface-separation.md)
3. Implement custom translators
4. Integrate with existing database

---

## 📦 Components

### User Integration (v1.0)
- `IUserDataRepository<TUser>` - User repository interface
- `ScimUserFilterTranslator` - Filter to LINQ translator
- `GenericScimFilterTranslator<T>` - Generic translator
- `ScimUserRepositoryAdapter<T>` - SCIM adapter

### Group Integration (v1.1)
- `IUserGroupDataRepository<TUser, TGroup>` - Combined user+group repository interface (inherits from IUserDataRepository)
- `ScimGroupFilterTranslator` - Filter to LINQ translator
- `ScimUserGroupRepositoryAdapter<TUser, TGroup>` - SCIM adapter for users and groups

### Constants (v1.1)
- `ScimAttributeNames.User.*` - 30+ user attribute constants
- `ScimAttributeNames.Group.*` - 5+ group attribute constants
- `ScimAttributeNames.Common.*` - 4 common attribute constants
- `ScimAttributeNames.EnterpriseUser.*` - 12+ enterprise constants
- `ScimAttributeNames.Operators.*` - 12 operator constants

---

## 💡 Example Implementation

### Annotate Your Model

```csharp
public class MyUser
{
    [ScimProperty(ScimAttributeNames.User.UserName, "string", Required = true)]
    public string Email { get; set; }
    
    [ScimProperty(ScimAttributeNames.User.Active, "boolean")]
    public bool IsActive { get; set; }
    
    [ScimProperty(ScimAttributeNames.User.DisplayName, "string")]
    public string FullName { get; set; }
}
```

### Implement Repository

```csharp
public class MyUserRepository : IUserDataRepository<MyUser>
{
    private readonly IQueryable<MyUser> _users;
    
    public async Task<IEnumerable<MyUser>> GetAsync(
        FilterExpression? filter = null,
        int? startIndex = null,
        int? count = null)
    {
        var query = _users.AsQueryable();
        
        if (filter != null)
        {
            var translator = new ScimUserFilterTranslator();
            var predicate = translator.Translate(filter);
            query = query.Where(predicate);
        }
        
        return await query
            .Skip((startIndex ?? 1) - 1)
            .Take(count ?? 100)
            .ToListAsync();
    }
}
```

### Configure Dependency Injection

```csharp
services.AddScoped<IUserDataRepository<MyUser>, MyUserRepository>();
```

---

## 📊 Supported Databases

- Entity Framework Core
- SQL Server
- PostgreSQL
- MySQL
- SQLite
- Cosmos DB
- MongoDB
- Custom IQueryable implementations

---

## 📖 Documentation Structure

- **quick-start-repository.md** - 15-minute quick start
- **repository-mapping-overview.md** - Architecture overview
- **repository-adapter-guide.md** - Complete implementation guide
- **repository-mapping-index.md** - Navigation index
- **interface-separation.md** - Design patterns
- **mapping-readme.md** - Additional mapping details
- **groups-and-constants-extension.md** - Groups and constants

---

## 🧪 Testing

All components include comprehensive unit tests:

```bash
# Run repository tests
dotnet test --filter "RepositoryAdapter"

# Run filter translator tests
dotnet test --filter "FilterTranslator"

# Run integration tests
dotnet test --filter "IntegrationTests"
```

---

## ✅ Integration Checklist

- [ ] Model classes annotated with `[ScimProperty]`
- [ ] `IUserDataRepository<T>` implemented
- [ ] `IUserGroupDataRepository<TUser, TGroup>` implemented (if groups needed)
- [ ] Dependency injection configured
- [ ] Filter translations working
- [ ] Pagination implemented
- [ ] Error handling in place
- [ ] Unit tests passing
- [ ] Performance tested
- [ ] Documentation updated

---

## 🔗 Related Documentation

- [Quick Start Guide](../guides/quickstart.md)
- [Filter Documentation](../filters/overview.md)
- [Schema System](../schema/system-overview.md)
- [Testing Documentation](../tests/test-suite-update.md)

---

**Status**: ✅ Complete  
**Last Updated**: February 21, 2026  
**Version**: 1.1 (Users + Groups + Constants)

