<div align="center">
  <img src="EzSCIM.png" alt="EzSCIM logo" width="120" />

  # EzSCIM

  **SCIM 2.0 server implementation for ASP.NET Core**

  [![CI](https://github.com/miiitch/EzSCIM/actions/workflows/ci-release.yml/badge.svg)](https://github.com/miiitch/EzSCIM/actions/workflows/ci-release.yml)
  [![NuGet EzSCIM](https://img.shields.io/nuget/v/EzSCIM?label=EzSCIM)](https://www.nuget.org/packages/EzSCIM)
  [![NuGet EzSCIM.EfCore](https://img.shields.io/nuget/v/EzSCIM.EfCore?label=EzSCIM.EfCore)](https://www.nuget.org/packages/EzSCIM.EfCore)
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
</div>

---

EzSCIM lets you add a **SCIM 2.0 provisioning endpoint** to any ASP.NET Core application in minutes.
It is compatible with **Microsoft Entra ID** (Azure AD) automatic user provisioning.

## Features

- ✅ Full SCIM 2.0 Users and Groups endpoints (GET, POST, PUT, PATCH, DELETE)
- ✅ SCIM filter translation to LINQ — works with any data source
- ✅ Schema extensions via `[ScimProperty]` attribute
- ✅ JWT Bearer authentication out of the box
- ✅ EF Core base repository with zero-boilerplate CRUD
- ✅ Multi-provider support (SQL Server, PostgreSQL, …)
- ✅ Microsoft Entra ID provisioning verified

## Choose your integration model

| | `EzSCIM` | `EzSCIM` + `EzSCIM.EfCore` |
|---|---|---|
| **Data source** | Any (IQueryable) | Entity Framework Core |
| **Setup** | Implement `IUserGroupDataRepository<TUser, TGroup>` | Inherit `EfScimRepositoryBase<TUser, TGroup, TContext>` |
| **Best for** | Dapper, Cosmos DB, MongoDB, custom repositories | EF Core / DbContext |

## Quick start (EF Core model)

```bash
dotnet add package EzSCIM
dotnet add package EzSCIM.EfCore
```

```csharp
// Program.cs
builder.Services.AddEzScim<MyUser, MyGroup>();
builder.Services.AddScoped<IScimRepository, MyRepository>();
builder.Services.AddJwtTokenService(builder.Configuration);

app.MapScimEndpoints();
```

```csharp
// MyRepository.cs
public class MyRepository : EfScimRepositoryBase<MyUser, MyGroup, MyDbContext>
{
    public MyRepository(MyDbContext context) : base(context) { }
}
```

→ **[Full documentation](https://miiitch.github.io/EzSCIM)**

## Documentation

| Topic | Link |
|---|---|
| Getting started (IQueryable) | [docs/public/iqueryable/getting-started.md](docs/public/iqueryable/getting-started.md) |
| Getting started (EF Core) | [docs/public/efcore/getting-started.md](docs/public/efcore/getting-started.md) |
| Authentication | [docs/public/authentication.md](docs/public/authentication.md) |
| SCIM 2.0 attribute reference | [docs/public/iqueryable/scim-attributes.md](docs/public/iqueryable/scim-attributes.md) |
| Architecture (contributors) | [docs/internal/architecture.md](docs/internal/architecture.md) |
| Changelog | [CHANGELOG.md](CHANGELOG.md) |

## Copilot Skill

This repository ships an **Agent Skill** that guides you through integrating EzSCIM step-by-step:
choose your integration model, answer four quick questions, and get ready-to-use boilerplate for your entity, repository, DI registration, and JWT authentication.

### Install via `gh`

```bash
gh skill install miiitch/EzSCIM ezscim
```

### Usage

After installing, open GitHub Copilot Chat and ask it to set up EzSCIM, describing your stack:

> *"Set up EzSCIM in my ASP.NET Core project using EF Core and SQL Server"*

or simply:

> *"Add SCIM provisioning to my app"*

The skill will ask four questions before generating any code:

| # | Question | Options |
|---|----------|---------|
| 1 | Integration model? | **IQueryable** (Dapper, Cosmos DB, custom…) or **EF Core** |
| 2 | Group support needed? | Yes / No |
| 3 | Database / ORM in use? | Free text — e.g. "EF Core + PostgreSQL" |
| 4 | Add JWT Bearer authentication? | Yes / No |

Based on your answers it generates:
- Entity class annotated with `[ScimProperty]` (IQueryable) or implementing `IScimEntity` (EF Core)
- Repository class (`IUserGroupDataRepository` or `EfScimRepositoryBase`)
- `Program.cs` DI registration
- `appsettings.json` JWT configuration (if auth requested)

### Install — per repository (manual)

Copy `skills/ezscim/` into your project:

```
<your-repo>/
  .github/
    copilot/
      skills/
        ezscim/
```

### Install — per user (all workspaces, manual)

| OS | Path |
|----|------|
| Windows | `%USERPROFILE%\.agents\skills\ezscim\` |
| macOS / Linux | `~/.agents/skills/ezscim/` |

### Publish a new release (maintainers)

Use `gh skill publish` — it validates the skill against the spec, adds the `agent-skills` repo topic, creates the tag and the GitHub Release, then triggers the CI which publishes NuGet packages and attaches the skill zip to the release.

```bash
# Validate only (no publish)
gh skill publish --dry-run

# Publish a new version
gh skill publish --tag v1.2.3
```

---

## Contributing

Contributions are welcome! Please read the [contributor guide](docs/internal/README.md) before submitting a PR.

All code, comments, and documentation must be in **English**.

## License

MIT — see [LICENSE](LICENSE).
