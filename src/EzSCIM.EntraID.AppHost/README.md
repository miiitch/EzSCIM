# EzSCIM Aspire AppHost - Quick Start

This is the orchestration layer for the EzSCIM SCIM API using .NET Aspire with DevTunnels for secure public access.

## Quick Start (30 seconds)

```powershell
# 1. Start the AppHost
dotnet run --project .\EzSCIM.EntraID.AppHost

# 2. Open Aspire Dashboard
# http://localhost:18888

# 3. Get DevTunnel URL from dashboard

# 4. Generate token
.\Generate-Token.ps1 -ApiBaseUrl "https://<your-tunnel>.devtunnels.ms"

# 5. Test SCIM API
$headers = @{ "Authorization" = "Bearer <token>" }
Invoke-RestMethod -Uri "https://<your-tunnel>/scim/Users" -Headers $headers
```

## Documentation

For comprehensive documentation, see:

- **Complete Guide**: [`ASPIRE-ENTRAID-SCIM-GUIDE.md`](./ASPIRE-ENTRAID-SCIM-GUIDE.md)
- **Quick Reference**: [`ENTRAID-DEMO-ASPIRE-SCIM.md`](./ENTRAID-DEMO-ASPIRE-SCIM.md)

## Key Features

✅ **Aspire Integration**: Unified service orchestration  
✅ **DevTunnels**: Secure public HTTPS endpoint  
✅ **JWT Authentication**: Bearer token security  
✅ **SCIM 2.0 Compliant**: Users, Groups, Schemas, ServiceProviderConfig  
✅ **Entra ID Ready**: Direct integration with Microsoft Entra ID  

## Prerequisites

- .NET 8.0 SDK or later
- PowerShell 5.1 or later
- Visual Studio 2022 (recommended)

## Architecture

```
┌─────────────────────────────────────┐
│   EzSCIM.EntraID.AppHost (Aspire)   │
│  - Service Orchestration             │
│  - DevTunnel Management              │
└────────────┬────────────────────────┘
             │
             ▼
┌─────────────────────────────────────┐
│  DevTunnel (Public HTTPS)            │
│  - Automatic Certificate Management  │
│  - Global Access                     │
└────────────┬────────────────────────┘
             │
             ▼
┌─────────────────────────────────────┐
│  EzSCIM.EntraID.Demo (SCIM API)     │
│  - JWT Authentication                │
│  - SCIM Endpoints (Users/Groups)     │
│  - ServiceProviderConfig             │
└──────────────────────────────────────┘
```

## Common Tasks

### Start Development Environment

```powershell
dotnet run --project .\EzSCIM.EntraID.AppHost
```

### Monitor Services

Open browser: `http://localhost:18888`

### Get Public URL

From Aspire Dashboard → `scimapi` resource → Click endpoint link

### Generate Authentication Token

```powershell
.\Generate-Token.ps1 -ApiBaseUrl "https://<your-tunnel>.devtunnels.ms" -CopyToClipboard
```

### Test SCIM API

```powershell
$token = "YOUR_JWT_TOKEN"
$headers = @{ "Authorization" = "Bearer $token" }
$base = "https://<your-tunnel>.devtunnels.ms"

# Get all users
Invoke-RestMethod -Uri "$base/scim/Users" -Headers $headers -Method Get

# Create user
$user = @{
    userName = "user@example.com"
    displayName = "Test User"
}
Invoke-RestMethod -Uri "$base/scim/Users" -Headers $headers `
  -Body ($user | ConvertTo-Json) -Method Post
```

### Connect to Microsoft Entra ID

1. Go to Microsoft Entra ID → Enterprise Applications
2. Create new application (or use existing)
3. Configure Provisioning:
   - **Tenant URL**: `https://<your-tunnel>/scim`
   - **Secret Token**: `<jwt-token>`
4. Click "Test Connection"
5. Enable provisioning and assign users

## Configuration

Configuration files:

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production settings

Key settings:

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## Troubleshooting

### AppHost won't start

- Verify .NET SDK version: `dotnet --version` (requires 8.0+)
- Check port 18888 is available
- Restart Terminal/PowerShell

### DevTunnel connection issues

- Restart AppHost
- Check Azure CLI is installed: `az version`
- Check internet connection

### Authentication fails (401 Unauthorized)

- Regenerate token: `.\Generate-Token.ps1 -ApiBaseUrl "..."`
- Verify Bearer prefix: `"Authorization" = "Bearer <token>"`
- Check token expiration

### Entra ID integration fails

- Verify exact Tenant URL format
- Use token WITHOUT "Bearer" prefix in Entra
- Check SCIM API logs in dashboard

## Support & Resources

- **Full Documentation**: See `ASPIRE-ENTRAID-SCIM-GUIDE.md`
- **SCIM Specification**: https://tools.ietf.org/html/rfc7643
- **Aspire Docs**: https://learn.microsoft.com/en-us/dotnet/aspire
- **DevTunnels**: https://learn.microsoft.com/en-us/azure/developer/dev-tunnels

## Project Structure

```
EzSCIM.EntraID.AppHost/
├── Program.cs                      # Aspire configuration
├── appsettings.json               # Settings
├── appsettings.Development.json   # Dev overrides
├── ASPIRE-ENTRAID-SCIM-GUIDE.md  # Comprehensive guide
└── ENTRAID-DEMO-ASPIRE-SCIM.md   # Quick reference
```

Related Projects:
- `../EzSCIM.EntraID.Demo/` - SCIM API service
- `../EzSCIM/` - Core SCIM library
- `../EzSCIM.ServiceDefaults/` - Shared configuration

---

**Next Steps**:

1. ✅ Ensure prerequisites are installed
2. ✅ Read `ASPIRE-ENTRAID-SCIM-GUIDE.md` for complete setup
3. ✅ Start AppHost: `dotnet run --project .\EzSCIM.EntraID.AppHost`
4. ✅ Access dashboard: `http://localhost:18888`
5. ✅ Generate token and test API

**Version**: 1.0  
**Last Updated**: February 20, 2026

