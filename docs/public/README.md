# EzSCIM — NuGet Package Documentation

EzSCIM provides a complete **SCIM 2.0** server implementation for ASP.NET Core.
You expose your data through one of two integration models.

---

## Choose your integration model

### 🔌 Model 1 — IQueryable (any data source)

**NuGet package**: `EzSCIM`

Use this model if you:
- Already have an existing repository or ORM (Dapper, Cosmos DB, MongoDB, custom…)
- Want full control over data access
- Do **not** use Entity Framework Core

Your entity class gets annotated with `[ScimProperty]` attributes. You implement
`IUserGroupDataRepository<TUser, TGroup>` with an `IQueryable<T>` source, and EzSCIM
translates SCIM filters to LINQ server-side.

→ **[Get started with IQueryable](./iqueryable/getting-started.md)**

---

### 🗄️ Model 2 — EF Core / DbContext

**NuGet packages**: `EzSCIM` + `EzSCIM.EfCore`

Use this model if you use Entity Framework Core. Inherit `EfScimRepositoryBase<TUser, TGroup, TContext>`
and your entities get automatic Id generation, timestamps, CRUD, filter translation, and
unique-constraint detection — with zero boilerplate.

→ **[Get started with EF Core](./efcore/getting-started.md)**

---

## 🔐 Authentication

JWT Bearer token authentication — applies to **both** models.

→ **[Authentication setup](./authentication.md)**

---

## Reference

| Topic | Model |
|---|---|
| [Repository interfaces](./iqueryable/repository.md) | IQueryable |
| [SCIM filter syntax](./iqueryable/filtering.md) | IQueryable |
| [SCIM 2.0 attribute reference](./iqueryable/scim-attributes.md) | Both |
| [Microsoft Entra ID required SCIM fields](./iqueryable/scim-attributes.md#microsoft-entra-id-required-scim-fields) | Both |
| [Schema extensions `[ScimProperty]`](./iqueryable/schema-extensions.md) | Both |
| [IScimEntity interface](./efcore/iscimentity.md) | EF Core |
| [EfScimRepositoryBase reference](./efcore/efrepositorybase.md) | EF Core |
| [Multi-provider: SQL Server / PostgreSQL](./efcore/multi-provider.md) | EF Core |

---

**Last Updated**: April 24, 2026

