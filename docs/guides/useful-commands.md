# 🛠️ Useful Commands - JWT Service & Aspire Integration

Quick reference for common commands.

## Build & Compilation

```powershell
# Clean build everything
dotnet clean
dotnet restore

# Build specific project
dotnet build "EzSCIM\EzSCIM.csproj"
dotnet build "EzSCIM.EntraID.Demo\EzSCIM.EntraID.Demo.csproj"
dotnet build "EzSCIM.EntraID.AppHost\EzSCIM.EntraID.AppHost.csproj"

# Build entire solution
dotnet build TestSCIM.sln

# Build with verbose output
dotnet build -v diagnostic
```

## Running the Application

```powershell
# Start Aspire AppHost
dotnet run --project .\EzSCIM.EntraID.AppHost

# With verbose logging
dotnet run --project .\EzSCIM.EntraID.AppHost --verbose

# Start Demo API directly (without Aspire)
dotnet run --project .\EzSCIM.EntraID.Demo
```

## Token Generation

```powershell
# Generate token with default settings
.\Generate-Token.ps1 -ApiBaseUrl "https://<tunnel-id>.devtunnels.ms"

# Generate and copy to clipboard
.\Generate-Token.ps1 -ApiBaseUrl "https://<tunnel-id>.devtunnels.ms" -CopyToClipboard

# Save to file
.\Generate-Token.ps1 -ApiBaseUrl "https://<tunnel-id>.devtunnels.ms" -OutputFile "token.txt"
```

## API Testing

### Using PowerShell

```powershell
# Set variables
$tunnelUrl = "https://<tunnel-id>.devtunnels.ms"
$token = "YOUR_JWT_TOKEN"
$headers = @{ "Authorization" = "Bearer $token" }

# Get service provider config
Invoke-RestMethod -Uri "$tunnelUrl/scim/ServiceProviderConfig" `
  -Headers $headers -Method Get

# Get all users
Invoke-RestMethod -Uri "$tunnelUrl/scim/Users" `
  -Headers $headers -Method Get

# Get specific user
Invoke-RestMethod -Uri "$tunnelUrl/scim/Users/{id}" `
  -Headers $headers -Method Get

# Create user
$user = @{
    userName = "test.user@example.com"
    displayName = "Test User"
}
Invoke-RestMethod -Uri "$tunnelUrl/scim/Users" `
  -Headers $headers `
  -Body ($user | ConvertTo-Json) `
  -Method Post

# Update user (PATCH)
$update = @{ displayName = "Updated Name" }
Invoke-RestMethod -Uri "$tunnelUrl/scim/Users/{id}" `
  -Headers $headers `
  -Body ($update | ConvertTo-Json) `
  -Method Patch

# Delete user
Invoke-RestMethod -Uri "$tunnelUrl/scim/Users/{id}" `
  -Headers $headers `
  -Method Delete
```

### Using curl

```bash
# Get service provider config
curl -H "Authorization: Bearer $TOKEN" \
  https://<tunnel-id>.devtunnels.ms/scim/ServiceProviderConfig

# Get all users
curl -H "Authorization: Bearer $TOKEN" \
  https://<tunnel-id>.devtunnels.ms/scim/Users

# Create user
curl -X POST \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"userName":"test@example.com","displayName":"Test"}' \
  https://<tunnel-id>.devtunnels.ms/scim/Users
```

## Testing & Validation

```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test "EzSCIM.UnitTests\EzSCIM.UnitTests.csproj"
dotnet test "EzSCIM.IntegrationTests\EzSCIM.IntegrationTests.csproj"

# Run tests with verbose output
dotnet test --verbosity detailed

# Run tests matching pattern
dotnet test --filter "JWT"
```

## Project Management

```powershell
# List projects in solution
dotnet sln list

# Add project to solution
dotnet sln add "NewProject/NewProject.csproj"

# Remove project from solution
dotnet sln remove "OldProject/OldProject.csproj"
```

## Dependency Management

```powershell
# List dependencies
dotnet list package

# Check for updates
dotnet package search [package-name]

# Update specific package
dotnet add package [package-name] --version [version]

# Remove package
dotnet remove package [package-name]
```

## Git Operations

```powershell
# View recent commits
git log --oneline -10

# Create feature branch
git checkout -b feature/jwt-service-extension

# Create commit
git commit -m "feat(jwt-service): add extension method for DI registration"

# Push changes
git push origin feature/jwt-service-extension

# Create pull request (GitHub CLI)
gh pr create --title "JWT Service Extension" --body "Adds DI extension method"
```

## Diagnostics & Troubleshooting

```powershell
# Check .NET version
dotnet --version

# Check installed SDKs
dotnet --list-sdks

# Check installed runtimes
dotnet --list-runtimes

# Get detailed build information
dotnet build -v diagnostic 2>&1 | Out-File build.log

# Check project references
dotnet list reference

# Verify solution can be restored
dotnet restore --dry-run
```

## Azure & Cloud Commands

```powershell
# Login to Azure
az login

# Check Azure subscription
az account show

# List key vaults
az keyvault list

# Get secret from Key Vault
az keyvault secret show --vault-name <vault-name> --name <secret-name>

# Set environment variable from Key Vault
$secret = az keyvault secret show --vault-name <vault-name> --name <secret-name> --query value -o tsv
$env:Jwt__SecretKey = $secret
```

## DevTunnel Commands

```powershell
# Check if DevTunnel is available
devtunnel --version

# List active tunnels
devtunnel list

# Close tunnel
devtunnel close [tunnel-id]
```

## Documentation Navigation

```powershell
# Open documentation in browser
Start-Process "QUICK-FIX-JWT-SERVICE.md"
Start-Process ".\EzSCIM.EntraID.AppHost\README.md"
Start-Process ".\EzSCIM.EntraID.AppHost\ASPIRE-ENTRAID-SCIM-GUIDE.md"

# Open in VS Code
code "QUICK-FIX-JWT-SERVICE.md"
code ".\EzSCIM.EntraID.AppHost\ASPIRE-ENTRAID-SCIM-GUIDE.md"
```

## Useful Shortcuts

```powershell
# Alias for common operations
function Start-Aspire {
    dotnet run --project .\EzSCIM.EntraID.AppHost
}

function Build-All {
    dotnet clean
    dotnet restore
    dotnet build
}

function Test-All {
    dotnet test --verbosity minimal
}

function Get-Token {
    .\Generate-Token.ps1 -ApiBaseUrl "https://<tunnel-id>.devtunnels.ms" -CopyToClipboard
}
```

## Docker Commands (if applicable)

```powershell
# Build Docker image
docker build -t ezscim:latest .

# Run Docker container
docker run -p 8080:80 ezscim:latest

# View logs
docker logs [container-id]

# Stop container
docker stop [container-id]
```

## Performance & Monitoring

```powershell
# Measure build time
Measure-Command {
    dotnet build
}

# Monitor application
Get-Process dotnet

# Check memory usage
dotnet-trace collect -p [pid]

# View real-time logs
Get-Content logs.txt -Tail 20 -Wait
```

## Common Workflows

### Quick Setup Flow

```powershell
# 1. Clean build
dotnet clean
dotnet restore

# 2. Build
dotnet build TestSCIM.sln

# 3. Start Aspire
dotnet run --project .\EzSCIM.EntraID.AppHost

# 4. In another terminal - generate token
.\Generate-Token.ps1 -ApiBaseUrl "https://<tunnel-id>.devtunnels.ms" -CopyToClipboard

# 5. Test API
$token = "YOUR_TOKEN"
$headers = @{ "Authorization" = "Bearer $token" }
Invoke-RestMethod -Uri "https://<tunnel-id>.devtunnels.ms/scim/Users" -Headers $headers
```

### Development Flow

```powershell
# 1. Create feature branch
git checkout -b feature/new-feature

# 2. Make changes
# ... edit files ...

# 3. Build
dotnet build

# 4. Test
dotnet test

# 5. Commit
git add .
git commit -m "feat: add new feature"

# 6. Push
git push origin feature/new-feature
```

### Debugging Flow

```powershell
# 1. Start with verbose logging
dotnet run --project .\EzSCIM.EntraID.AppHost --verbose

# 2. If issues, check logs
Get-Content logs.txt -Tail 50

# 3. Build in diagnostic mode
dotnet build -v diagnostic

# 4. Run tests with details
dotnet test --verbosity detailed
```

---

## Quick Reference Table

| Task | Command |
|------|---------|
| Build | `dotnet build` |
| Test | `dotnet test` |
| Run Aspire | `dotnet run --project .\EzSCIM.EntraID.AppHost` |
| Generate Token | `.\Generate-Token.ps1 -ApiBaseUrl "..."` |
| View Logs | `Get-Content logs.txt -Tail 20 -Wait` |
| Clean | `dotnet clean && dotnet restore` |
| Check Version | `dotnet --version` |

---

**Last Updated**: February 20, 2026  
**Version**: 1.0

