# EzSCIM Documentation

---

## 📦 Using the NuGet packages?

→ **[docs/public/](./public/README.md)**

| | |
|---|---|
| 🔌 [IQueryable integration](./public/iqueryable/getting-started.md) | Any data source — annotate your entity, implement `IQueryable<T>` |
| 🗄️ [EF Core integration](./public/efcore/getting-started.md) | Entity Framework Core — inherit `EfScimRepositoryBase` |
| 🔐 [Authentication](./public/authentication.md) | JWT Bearer token setup, Entra ID configuration |
| 📋 [SCIM 2.0 attribute reference](./public/iqueryable/scim-attributes.md) | All standard User / Group attributes |
| 🧭 [Entra ID required SCIM fields](./public/iqueryable/scim-attributes.md#microsoft-entra-id-required-scim-fields) | Minimum attributes, model example, validator links |
| 🤖 [Copilot Skill](./public/README.md#-copilot-skill) | Install the Agent Skill to get guided setup via Copilot Chat |
| 🧩 [Schema extensions](./public/iqueryable/schema-extensions.md) | `[ScimProperty]`, custom schemas |

---

## 🔧 Contributing to this repo?

→ **[docs/internal/](./internal/README.md)**

| | |
|---|---|
| [Architecture](./internal/architecture.md) | Multi-provider DbContext, EzSCIM.Demo.Data, request flow |
| [Development setup](./internal/development-setup.md) | Aspire, DevTunnel, migrations |
| [Testing](./internal/testing.md) | Testcontainers, xUnit collections, seed data |
| [SCIM Validator](./internal/scim-validator.md) | Running the validator, storing results, known issue |
| [Issues](./internal/issues/) | Known bugs |

---

## 📁 Structure

```
docs/
├── public/                  # 📦 NuGet package user documentation
│   ├── README.md            # Entry point: choose your integration model
│   ├── authentication.md    # JWT setup (both models)
│   ├── iqueryable/          # Model 1: IQueryable / any data source
│   │   ├── getting-started.md
│   │   ├── repository.md
│   │   ├── filtering.md
│   │   ├── scim-attributes.md
│   │   └── schema-extensions.md
│   └── efcore/              # Model 2: EF Core / DbContext
│       ├── getting-started.md
│       ├── iscimentity.md
│       ├── efrepositorybase.md
│       └── multi-provider.md
├── internal/                # 🔧 Contributor / maintainer documentation
│   ├── README.md
│   ├── architecture.md
│   ├── development-setup.md
│   ├── testing.md
│   ├── scim-validator.md
│   └── issues/
│       └── scim-validator-bug-patch-replace-attributes.md
├── scim-test-results/       # Raw SCIM validator JSON exports
└── archive/                 # Historical documentation (not maintained)
```

---

**Last Updated**: April 24, 2026
