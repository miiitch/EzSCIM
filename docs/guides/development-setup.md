# TestSCIM Development Instructions

## Temporary file management

### General rule

All temporary files created during development must be written to the `tmp/` folder at the repository root.

### Why

- `tmp/` is already covered by `.gitignore`, so files are never committed
- Keeps the repository clean
- Prevents temporary artifacts from polluting Git history

### Typical temporary files

- Test outputs
- Log files
- Script output files
- Cache or scratch files
- Debug dumps

### Implementation example

```csharp
string tmpFolder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "tmp");
string logFile = Path.Combine(tmpFolder, "myapp.log");
File.WriteAllText(logFile, "log content");
```

## DevTunnel exposure

### ScimAPI - anonymous exposure

`TestSCIM.AppHost` exposes `ScimAPI` anonymously through DevTunnel.

**Configuration in `Program.cs`:**

```csharp
builder.AddProject<Projects.ScimAPI>("scimapi")
    .WithExternalHttpEndpoints();  // Expose endpoints publicly
```

**Result:**
- A public URL is generated automatically
- Accessible anonymously without authentication
- Useful for integration tests and demos
- Connection details are printed in the startup console

## Accessing ScimAPI

### Local

```text
http://localhost:5000
```

### Through DevTunnel

A public URL is displayed on startup:

```text
https://<random-id>.devtunnels.ms/
```

## Important resources

- [`../filters/overview.md`](../filters/overview.md) - SCIM filter documentation
- [`../tests/entra-integration.md`](../tests/entra-integration.md) - Entra ID integration testing
- [`../README.md`](../README.md) - Documentation index
