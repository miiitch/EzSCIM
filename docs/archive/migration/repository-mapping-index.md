# Index - Repository -> SCIM Mapping

Quick navigation guide for repository-to-SCIM integration documentation.

---

## Quick start

Want to integrate SCIM in about 15 minutes?

Start with [`quick-start-repository.md`](./quick-start-repository.md)

---

## Documentation by role

### Developer - first integration

1. [`quick-start-repository.md`](./quick-start-repository.md)  
   Step-by-step integration guide for your existing repository.

2. [`repository-adapter-guide.md`](./repository-adapter-guide.md)  
   Full guide with examples and advanced use cases.

### Architect - system understanding

1. [`mapping-readme.md`](./mapping-readme.md)  
   Architecture and component overview.

2. [`repository-mapping-overview.md`](./repository-mapping-overview.md)  
   Design-level summary and mapping model.

3. [`interface-separation.md`](./interface-separation.md)  
   Interface hierarchy and rationale.

### Project lead - executive summary

1. [`mapping-readme.md`](./mapping-readme.md)  
   High-level status and implementation value.

---

## Documentation by need

| Need | Document | Typical time |
|---|---|---|
| Integrate quickly | `quick-start-repository.md` | 15 min |
| Understand architecture | `repository-mapping-overview.md` | 10 min |
| Review implementation details | `repository-adapter-guide.md` | 30 min |
| Understand interfaces | `interface-separation.md` | 15 min |
| Add groups support | `groups-and-constants-extension.md` | 20 min |

---

## Key implementation components

### Source code

**Interfaces**
- `EzSCIM/Filtering/IScimFilterTranslator.cs` - AST to LINQ translation contract
- `EzSCIM/DataRepositories/IUserDataRepository.cs` - Generic user data contract
- `EzSCIM/DataRepositories/IUserGroupDataRepository.cs` - Combined user+group data contract

**Implementations**
- `EzSCIM/Filtering/ScimUserFilterTranslator.cs` - `ScimUser` translator
- `EzSCIM/Filtering/GenericScimFilterTranslator.cs` - Generic translator
- `EzSCIM/Filtering/ScimGroupFilterTranslator.cs` - Group translator
- `EzSCIM/Repositories/ScimUserRepositoryAdapter.cs` - User adapter
- `EzSCIM/Repositories/ScimUserGroupRepositoryAdapter.cs` - User+group adapter

**Examples**
- `EzSCIM.EntraID.Demo/Examples/CustomUser.cs` - Annotated custom user model
- `EzSCIM.EntraID.Demo/Examples/CustomGroup.cs` - Annotated custom group model
- `EzSCIM.EntraID.Demo/Examples/CustomUserGroupRepository.cs` - Combined repository example

---

## Suggested learning path

### Level 1 - Beginner (30 minutes)
1. Read `quick-start-repository.md` (15 min)
2. Copy/adapt the `CustomUser` example (5 min)
3. Configure DI as shown in the guide (5 min)
4. Run basic SCIM requests (5 min)

**Outcome:** A working SCIM user integration.

### Level 2 - Intermediate (1 hour)
1. Read `repository-adapter-guide.md` (20 min)
2. Review attribute mapping conventions (10 min)
3. Explore advanced scenarios (15 min)
4. Tailor mapping for your model (15 min)

**Outcome:** An optimized integration for your domain model.

### Level 3 - Advanced (2 hours)
1. Read `repository-mapping-overview.md` (20 min)
2. Study AST to LINQ translation flow (30 min)
3. Review integration tests in `EzSCIM.IntegrationTests` (40 min)
4. Implement a custom translator extension if needed (30 min)

**Outcome:** Full mastery of repository-to-SCIM customization.

---

## Keyword navigation

### AST (`FilterExpression`)
- `repository-adapter-guide.md`
- `repository-mapping-overview.md`

### `IQueryable`
- `quick-start-repository.md`
- `repository-adapter-guide.md`

### `[ScimProperty]`
- `quick-start-repository.md`
- `repository-adapter-guide.md`
- `groups-and-constants-extension.md`

### Performance
- `mapping-readme.md`
- `repository-mapping-overview.md`

### Groups support
- `groups-and-constants-extension.md`
- `interface-separation.md`

---

## Related docs

- [`README.md`](./README.md)
- [`repository-mapping-overview.md`](./repository-mapping-overview.md)
- [`repository-adapter-guide.md`](./repository-adapter-guide.md)
- [`quick-start-repository.md`](./quick-start-repository.md)
- [`groups-and-constants-extension.md`](./groups-and-constants-extension.md)

---

**Last Updated:** April 2026
