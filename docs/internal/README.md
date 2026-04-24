# Internal Documentation

This section is for **contributors** to the EzSCIM repository.
For NuGet package usage documentation, see [../public/README.md](../public/README.md).

---

## Contents

| Document | Description |
|---|---|
| [architecture.md](./architecture.md) | Repository structure, multi-provider DbContext, EzSCIM.Demo.Data |
| [development-setup.md](./development-setup.md) | Local dev environment, Aspire, DevTunnel |
| [testing.md](./testing.md) | Integration tests, Testcontainers, xUnit collections |
| [scim-validator.md](./scim-validator.md) | Running the MS SCIM Validator, results, known issues |
| [issues/](./issues/) | Known bugs and workarounds |

---

## Project structure

```
scimwork/
├── EzSCIM/                      # Core library — NuGet package EzSCIM
│   ├── Controllers/             # UsersController, GroupsController, SchemasController
│   ├── Models/                  # ScimUser, ScimGroup, ScimPatchRequest, ...
│   ├── Repositories/            # IScimRepository, IScimUserGroupRepository
│   ├── DataRepositories/        # IUserGroupDataRepository, IScimFilterTranslator
│   ├── Filtering/               # GenericScimFilterTranslator, filter AST
│   ├── Services/                # JwtTokenService, ScimPatchApplier
│   └── Authentication/          # JwtBearerTokenAuthenticationHandler
│
├── EzSCIM.EfCore/               # EF Core abstractions — NuGet package EzSCIM.EfCore
│   ├── IScimEntity.cs
│   └── EfScimRepositoryBase.cs
│
├── EzSCIM.Demo.Data/            # Shared data library (used by Demo + IntegrationTests)
│   ├── ScimDbContextBase.cs     # Provider-agnostic base DbContext
│   ├── DemoScimRepository.cs    # IScimRepository implementation
│   ├── DemoUserEntityExtensions.cs
│   ├── DemoGroupEntityExtensions.cs
│   ├── Entities/                # DemoUserEntity, DemoGroupEntity, helpers
│   └── Repositories/            # DemoUserGroupRepository (EfScimRepositoryBase subclass)
│
├── EzSCIM.EntraID.Demo/         # Demo ASP.NET Core API (SQL Server via Aspire)
│   ├── Data/DemoScimDbContext.cs # SqlServer subclass of ScimDbContextBase
│   ├── Migrations/              # EF Core migrations for SQL Server
│   └── Program.cs
│
├── EzSCIM.EntraID.AppHost/      # Aspire orchestration (SQL Server container + DevTunnel)
├── EzSCIM.ServiceDefaults/      # Health checks, telemetry
│
├── EzSCIM.UnitTests/            # xUnit unit tests (in-memory, no DB)
├── EzSCIM.IntegrationTests/     # xUnit integration tests (PostgreSQL via Testcontainers)
│   ├── Data/PostgreSqlScimDbContext.cs  # PostgreSQL subclass of ScimDbContextBase
│   ├── Data/SeedData.cs
│   └── ScimWebApplicationFactory.cs
│
└── docs/
    ├── public/                  # NuGet package user documentation
    └── internal/                # This directory — contributor docs
```

---

## Contributing

1. All code, comments, and documentation must be in **English**
2. Follow Microsoft C# coding conventions
3. Add XML doc comments (`///`) to all public APIs
4. Run all tests before submitting a PR: `dotnet test`
5. For bug fixes: create a regression test first (see Testing section)

---

**Last Updated**: April 24, 2026

