# Development Setup

## Prerequisites

- .NET 9+ SDK
- Docker Desktop (for SQL Server via Aspire and PostgreSQL via Testcontainers)
- [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling):
  `dotnet workload install aspire`

---

## Run the Demo API

The Demo API uses **Aspire** to orchestrate a SQL Server container.

```bash
cd EzSCIM.EntraID.AppHost
dotnet run
```

Aspire starts:
- SQL Server container (auto-provisioned)
- `EzSCIM.EntraID.Demo` (the SCIM API)
- Aspire dashboard at `https://localhost:15000`

Default SCIM API URL: `https://localhost:7001`

---

## Run tests

```bash
# All tests
dotnet test

# Unit tests only (fast, no Docker required)
dotnet test EzSCIM.UnitTests

# Integration tests only (requires Docker for PostgreSQL via Testcontainers)
dotnet test EzSCIM.IntegrationTests

# With output
dotnet test --logger "console;verbosity=detailed"
```

---

## Temporary files

All temporary files created during development must go in the `tmp/` folder at
the repository root. It is covered by `.gitignore` and never committed.

```csharp
string tmpFolder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "tmp");
string logFile = Path.Combine(tmpFolder, "debug.log");
```

---

## DevTunnel (Entra ID integration)

To test with a real Entra ID tenant, expose the API publicly via DevTunnel.

In `EzSCIM.EntraID.AppHost/Program.cs`:

```csharp
builder.AddProject<Projects.EzSCIM_EntraID_Demo>("scimapi")
    .WithExternalHttpEndpoints();  // Expose endpoints publicly via DevTunnel
```

The public URL is printed to the console on startup:
```
https://<random-id>.devtunnels.ms/
```

Configure this URL in **Entra ID → Enterprise Applications → Provisioning → Tenant URL**.

---

## Migrations (SQL Server)

```bash
cd EzSCIM.EntraID.Demo

# Add a new migration
dotnet ef migrations add <MigrationName> \
  --context DemoScimDbContext

# Apply to local SQL Server (via Aspire)
dotnet ef database update --context DemoScimDbContext

# Generate SQL script
dotnet ef migrations script --context DemoScimDbContext
```

---

## Useful commands

```bash
# Build all
dotnet build

# Run a specific test
dotnet test --filter "FullyQualifiedName~EntraId_TestConnection"

# Clean build artifacts
dotnet clean

# Check for outdated NuGet packages
dotnet list package --outdated
```

---

**See also**: [architecture.md](./architecture.md) | [testing.md](./testing.md)

