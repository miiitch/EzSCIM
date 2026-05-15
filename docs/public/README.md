# EzSCIM

EzSCIM is a complete **SCIM 2.0** server implementation for ASP.NET Core.
Install one NuGet package, annotate your entity, implement one interface — and your API is provisioning-ready.

[![NuGet](https://img.shields.io/nuget/v/EzSCIM?label=EzSCIM&logo=nuget)](https://www.nuget.org/packages/EzSCIM)
[![NuGet](https://img.shields.io/nuget/v/EzSCIM.EfCore?label=EzSCIM.EfCore&logo=nuget)](https://www.nuget.org/packages/EzSCIM.EfCore)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/miiitch/EzSCIM/blob/master/LICENSE)

---

## Quick install

```bash
# Any data source (Dapper, Cosmos DB, custom…)
dotnet add package EzSCIM

# Entity Framework Core
dotnet add package EzSCIM
dotnet add package EzSCIM.EfCore
```

---

## Choose your integration model

<div class="grid cards" markdown>

-   :material-connection: **IQueryable model**

    ---

    Use this if you already have a repository or ORM that is **not** EF Core (Dapper, Cosmos DB, MongoDB, custom…).

    Annotate your entity with `[ScimProperty]`, implement `IUserGroupDataRepository<TUser, TGroup>` returning an `IQueryable<T>`, and EzSCIM translates SCIM filters to LINQ server-side.

    [:octicons-arrow-right-24: Get started with IQueryable](./iqueryable/getting-started.md)

-   :material-database: **EF Core model**

    ---

    Use this if you use **Entity Framework Core**. Inherit `EfScimRepositoryBase<TUser, TGroup, TContext>` and get automatic Id generation, timestamps, CRUD, filter translation, and unique-constraint detection — with zero boilerplate.

    [:octicons-arrow-right-24: Get started with EF Core](./efcore/getting-started.md)

</div>

---

## Prerequisites

!!! info "Requirements"
    - [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) or later
    - ASP.NET Core 8+
    - (EF Core model only) Entity Framework Core 8+

---

## :material-lock: Authentication

JWT Bearer token authentication — applies to **both** models.

[:octicons-arrow-right-24: Authentication setup](./authentication.md)

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

**Last Updated**: May 2026

