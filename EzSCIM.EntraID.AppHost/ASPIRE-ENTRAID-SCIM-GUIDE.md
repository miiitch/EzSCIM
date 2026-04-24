# Entra ID Demo with Aspire + SCIM Integration Guide

This comprehensive guide explains how to set up and test the SCIM API with Aspire, create DevTunnels for public access, generate authentication tokens, and integrate with Microsoft Entra ID.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Architecture Overview](#architecture-overview)
3. [Starting the Application](#starting-the-application)
4. [Accessing the DevTunnel](#accessing-the-devtunnel)
5. [Token Generation](#token-generation)
6. [SCIM API Testing](#scim-api-testing)
7. [Microsoft Entra ID Integration](#microsoft-entra-id-integration)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

Before you begin, ensure you have:

- **.NET SDK**: Version 8.0 or later
- **Visual Studio 2022** or **Visual Studio Code** with C# support
- **PowerShell**: 5.1 or later (for token generation scripts)
- **Azure CLI** (optional, for advanced scenarios): `az` command-line tool
- **Postman** or **curl** (for manual API testing)
- **Microsoft Entra ID tenant**: For integration testing

---

## Architecture Overview

The solution consists of multiple projects:

```
EzSCIM.EntraID.AppHost (Aspire Orchestration)
├── Manages DevTunnel exposure
├── Coordinates services
└── Provides unified dashboard

    ↓
    
EzSCIM.EntraID.Demo (SCIM API Service)
├── Handles SCIM endpoints (Users, Groups)
├── JWT token authentication
├── ServiceProviderConfig endpoint
└── Schemas endpoint

    ↓
    
EzSCIM (Core Library)
├── SCIM models and contracts
├── Authentication handlers
├── Repository patterns
└── JWT token service
```

### Key Components

1. **Aspire AppHost**: Orchestrates the SCIM API and manages DevTunnel exposure
2. **DevTunnel**: Creates a public HTTPS endpoint for your local development environment
3. **JWT Authentication**: Secures SCIM endpoints with Bearer token authentication
4. **SCIM Controllers**: Implements SCIM 2.0 protocol for user and group provisioning

---

## Starting the Application

### Step 1: Open the Solution

Navigate to the repository root and open the solution:

```powershell
# Navigate to the project directory
cd C:\Users\MichelPerfetti\src\private\scimwork

# Open in Visual Studio
start TestSCIM.sln
```

### Step 2: Start Aspire AppHost

Open a PowerShell terminal at the repository root and run:

```powershell
# Start the Aspire AppHost
dotnet run --project .\EzSCIM.EntraID.AppHost

# Or with verbose output
dotnet run --project .\EzSCIM.EntraID.AppHost --verbose
```

Expected output:

```
info: Aspire.Hosting[0]
      Starting application using the Distributed Application runtime...
info: Aspire.Hosting[0]
      Aspire dashboard running at http://localhost:18888
info: Aspire.Hosting[0]
      scimapi resource starting...
...
Resources started. Press Ctrl+C to shut down.
```

### Step 3: Access Aspire Dashboard

The Aspire dashboard provides visibility into running services:

- **Dashboard URL**: `http://localhost:18888`
- **Features**:
  - Real-time service status
  - Resource logs
  - Endpoint links
  - Environment variables

---

## Accessing the DevTunnel

### Understanding DevTunnels

DevTunnels provide secure, public HTTPS endpoints for your local services without port forwarding or complex networking:

- Automatically renews certificates
- No firewall configuration needed
- Works across networks and corporate firewalls
- Accessible from anywhere globally

### Step 1: Retrieve the DevTunnel URL

After starting the AppHost, the DevTunnel URL is displayed:

**From Aspire Dashboard:**
1. Go to `http://localhost:18888`
2. Click on the `scimapi` resource
3. Click the endpoint link labeled `https`

**From Console Output:**
Look for the URL pattern in the AppHost console:

```
Resource: scim (DevTunnel)
Status: Running
Endpoint: https://<your-tunnel-id>.devtunnels.ms
```

### Step 2: Verify the Connection

Test connectivity to your DevTunnel:

```powershell
# Replace <your-tunnel-id> with your actual tunnel ID
$tunnelUrl = "https://<your-tunnel-id>.devtunnels.ms"

# Test basic connectivity
Invoke-WebRequest -Uri "$tunnelUrl/scim/ServiceProviderConfig" -Method Get -SkipCertificateCheck
```

If successful, you'll see response headers and a 401 (Unauthorized) status, which is expected since no token is provided yet.

---

## Token Generation

JWT tokens are required to authenticate against protected SCIM endpoints. The token service is configured with HS256 (HMAC SHA-256) signing.

### Token Configuration

Token settings are defined in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-that-should-be-at-least-32-characters-long",
    "Issuer": "EzSCIM",
    "Audience": "SCIM-Client"
  }
}
```

**Important**: In production, use Azure Key Vault to store the secret key.

### Method 1: Using PowerShell Script

The `Generate-Token.ps1` script simplifies token generation:

```powershell
# Basic usage
.\Generate-Token.ps1 -ApiBaseUrl "https://<your-tunnel-id>.devtunnels.ms"

# Copy token to clipboard
.\Generate-Token.ps1 -ApiBaseUrl "https://<your-tunnel-id>.devtunnels.ms" -CopyToClipboard

# Generate and save to file
.\Generate-Token.ps1 -ApiBaseUrl "https://<your-tunnel-id>.devtunnels.ms" -OutputFile "token.txt"
```

### Method 2: Direct API Call

Generate a token by calling the token endpoint:

```powershell
$response = Invoke-RestMethod `
  -Uri "https://<your-tunnel-id>.devtunnels.ms/scim/auth/token" `
  -Method Get

$token = $response.token
Write-Host "Bearer $token"
```

### Method 3: Manual Token Creation

If the API endpoint is unavailable, create a token using the JWT service in code:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

// Configuration
var secretKey = "your-super-secret-key-that-should-be-at-least-32-characters-long";
var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

// Create key
var key = new SymmetricSecurityKey(secretKeyBytes);
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

// Create claims
var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, "scim-client"),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};

// Create token
var token = new JwtSecurityToken(
    issuer: null,
    audience: null,
    claims: claims,
    expires: DateTime.UtcNow.AddHours(1),
    signingCredentials: credentials
);

// Write token
var handler = new JwtSecurityTokenHandler();
string jwt = handler.WriteToken(token);
Console.WriteLine($"Bearer {jwt}");
```

### Token Lifetime

- **Default Duration**: 60 minutes
- **Custom Duration**: Pass `expirationMinutes` parameter to the token service
- **Renewal**: Generate a new token when the current one expires

---

## SCIM API Testing

### Test Endpoints Overview

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/scim/ServiceProviderConfig` | GET | Returns server capabilities |
| `/scim/Schemas` | GET | Returns supported SCIM schemas |
| `/scim/Users` | GET | List all users |
| `/scim/Users` | POST | Create a new user |
| `/scim/Users/{id}` | GET | Get specific user |
| `/scim/Users/{id}` | PUT | Update entire user |
| `/scim/Users/{id}` | PATCH | Update user fields |
| `/scim/Groups` | GET | List all groups |
| `/scim/Groups` | POST | Create a new group |

### Setup Bearer Token

All authenticated requests require a Bearer token in the Authorization header:

```powershell
$token = "eyJhbGc..."  # Your JWT token
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}
```

### Example 1: Get Service Provider Configuration

```powershell
$tunnelUrl = "https://<your-tunnel-id>.devtunnels.ms"
$token = "YOUR_JWT_TOKEN"
$headers = @{ "Authorization" = "Bearer $token" }

$response = Invoke-RestMethod `
  -Uri "$tunnelUrl/scim/ServiceProviderConfig" `
  -Headers $headers `
  -Method Get

$response | ConvertTo-Json | Write-Host
```

### Example 2: List All Users

```powershell
$response = Invoke-RestMethod `
  -Uri "$tunnelUrl/scim/Users" `
  -Headers $headers `
  -Method Get

$response.Resources | Format-Table -Property id, userName, displayName
```

### Example 3: Create a New User

```powershell
$newUser = @{
    userName = "john.doe@example.com"
    name = @{
        givenName = "John"
        familyName = "Doe"
        formatted = "John Doe"
    }
    displayName = "John Doe"
    emails = @(
        @{
            value = "john.doe@example.com"
            type = "work"
            primary = $true
        }
    )
    active = $true
}

$response = Invoke-RestMethod `
  -Uri "$tunnelUrl/scim/Users" `
  -Headers $headers `
  -Body ($newUser | ConvertTo-Json) `
  -Method Post

Write-Host "User created: $($response.id)"
```

### Example 4: Update a User (PATCH)

```powershell
$userId = "550e8400-e29b-41d4-a716-446655440000"

$update = @{
    displayName = "John Michael Doe"
}

$response = Invoke-RestMethod `
  -Uri "$tunnelUrl/scim/Users/$userId" `
  -Headers $headers `
  -Body ($update | ConvertTo-Json) `
  -Method Patch

Write-Host "User updated successfully"
```

### Example 5: Create a Group

```powershell
$newGroup = @{
    displayName = "Engineering Team"
}

$response = Invoke-RestMethod `
  -Uri "$tunnelUrl/scim/Groups" `
  -Headers $headers `
  -Body ($newGroup | ConvertTo-Json) `
  -Method Post

Write-Host "Group created: $($response.id)"
```

---

## Microsoft Entra ID Integration

### Prerequisites for Entra Integration

1. **Microsoft Entra ID Tenant**: Access to at least one Azure AD / Entra tenant
2. **Admin Privileges**: Ability to configure Enterprise Applications
3. **Public URL**: DevTunnel URL (already created)
4. **Valid JWT Token**: Generated from the SCIM API

### Step 1: Create Enterprise Application

In Microsoft Entra ID:

1. Go to **Azure Portal** → **Microsoft Entra ID** → **Enterprise applications**
2. Click **+ New application**
3. Click **+ Create your own application**
4. Name: `SCIM Demo` (or your preferred name)
5. Select **Integrate any other application you don't find in the gallery**
6. Click **Create**

### Step 2: Configure SCIM Provisioning

1. In your new Enterprise Application, go to **Provisioning**
2. Click **Get started**
3. Select **Automated** provisioning method
4. In **Admin Credentials** section, configure:

   - **Tenant URL**: `https://<your-tunnel-id>.devtunnels.ms/scim`
   - **Secret Token**: `<your-jwt-token>` (without "Bearer" prefix)
   - Click **Test Connection**

5. If test passes, click **Save**

### Step 3: Configure Attribute Mappings

1. Go to **Provisioning** → **Mappings** → **Provision Azure Active Directory Users**
2. Review default mappings (they should work for basic scenarios)
3. Click **Save** to apply

### Step 4: Enable Provisioning

1. In **Provisioning** section, set **Provisioning Status** to **On**
2. Click **Save**
3. Click **Provision on demand** to test with a sample user

### Step 5: Monitor Provisioning

Monitor provisioning activity:

- Go to **Provisioning** → **Provisioning logs**
- View successful and failed provisioning attempts
- Review operation details and error messages

### Example: Provision a User to SCIM

```powershell
# Assign a user to the Enterprise Application in Entra
# This triggers SCIM provisioning

$headers = @{ "Authorization" = "Bearer $token" }

# Verify user was created in SCIM API
$users = Invoke-RestMethod `
  -Uri "https://<your-tunnel-id>.devtunnels.ms/scim/Users" `
  -Headers $headers `
  -Method Get

$users.Resources | Where-Object { $_.userName -eq "user@example.com" }
```

---

## Troubleshooting

### Issue: "Connection refused" or "Unable to connect"

**Cause**: AppHost not running or DevTunnel not active

**Solution**:
1. Verify AppHost is still running: `dotnet run --project .\EzSCIM.EntraID.AppHost`
2. Check console for the DevTunnel URL
3. Test connectivity: `Invoke-WebRequest -Uri "https://<your-tunnel>.devtunnels.ms/scim/ServiceProviderConfig" -SkipCertificateCheck`

### Issue: "401 Unauthorized"

**Cause**: Missing, invalid, or expired Bearer token

**Solution**:
1. Generate a fresh token: `.\Generate-Token.ps1 -ApiBaseUrl "https://<your-tunnel>.devtunnels.ms"`
2. Verify token format: `Bearer <token>`
3. Check token hasn't expired (default 60 minutes)

### Issue: "403 Forbidden"

**Cause**: Token service endpoint not available

**Solution**:
1. Confirm running in development environment
2. Check configuration: `appsettings.Development.json`
3. Restart AppHost to reload settings

### Issue: "Invalid token signature"

**Cause**: Secret key mismatch

**Solution**:
1. Verify `Jwt:SecretKey` in `appsettings.json`
2. Ensure secret is at least 32 characters
3. Regenerate tokens after changing the secret

### Issue: "DevTunnel URL keeps changing"

**Cause**: DevTunnel session expires or reconnects

**Solution**:
1. DevTunnels renew automatically (expected behavior)
2. Stop AppHost and restart for a fresh tunnel
3. Store tunnel ID if persistence is needed

### Issue: Entra Integration Test Connection Fails

**Cause**: Incorrect tenant URL or token format

**Solution**:
1. Verify exact URL: `https://<your-tunnel>.devtunnels.ms/scim`
2. Use token WITHOUT "Bearer" prefix in Secret Token field
3. Check SCIM API logs for detailed error
4. Verify firewall/network allows DevTunnel access

### Issue: CORS Errors

**Cause**: Cross-origin requests blocked

**Solution**:
1. CORS is enabled in development by default
2. If issues persist, check `appsettings.Development.json` for CORS configuration
3. Restart AppHost after modifying CORS settings

---

## Advanced Topics

### Using Azure Key Vault for Secrets

In production, store the JWT secret in Azure Key Vault:

```csharp
var keyVaultUrl = builder.Configuration["AzureKeyVault:VaultUri"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}
```

### Custom Aspire Configuration

The Aspire configuration is in `EzSCIM.EntraID.AppHost/Program.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.EzSCIM_EntraID_Demo>("scimapi")
    .WithExternalHttpEndpoints();

var tunnel = builder.AddDevTunnel("scim")
    .WithReference(api)
    .WithAnonymousAccess()
    .WithHttpsEndpoint();

builder.Build().Run();
```

- Modify route prefixes by changing project names
- Add additional services by chaining `.AddProject()` calls
- Configure DevTunnel access restrictions with `.WithAccess()`

### Performance Testing

For load testing SCIM endpoints:

```powershell
# Using Apache Bench (if installed)
ab -n 1000 -c 10 -H "Authorization: Bearer $token" `
   "https://<your-tunnel>/scim/Users"

# Using PowerShell with loop
1..100 | ForEach-Object {
    Invoke-RestMethod -Uri "https://<your-tunnel>/scim/Users" `
      -Headers $headers -AsJob
}
```

---

## Additional Resources

- [SCIM 2.0 Specification](https://tools.ietf.org/html/rfc7643)
- [Microsoft Entra Provisioning Documentation](https://learn.microsoft.com/en-us/azure/active-directory/app-provisioning/)
- [Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire)
- [DevTunnels Documentation](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/)
- [JWT.io Token Debugger](https://jwt.io)

---

## Support

For issues or questions:

1. Check the troubleshooting section above
2. Review application logs in Aspire dashboard
3. Verify all prerequisites are installed
4. Consult SCIM specification for protocol-specific issues

---

**Last Updated**: February 20, 2026  
**Version**: 1.0

