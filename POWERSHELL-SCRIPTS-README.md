# PowerShell Scripts for JWT Authentication

All scripts are written in **PowerShell only** with complete English documentation and comments.

## Available Scripts

### 1. Generate-Token.ps1

Generates a JWT Bearer Token by calling the test endpoint `/scim/auth/token`.

**Usage:**
```powershell
# Simple usage
.\Generate-Token.ps1

# Show decoded payload
.\Generate-Token.ps1 -ShowDecoded

# Copy token to clipboard automatically
.\Generate-Token.ps1 -CopyToClipboard

# Use custom API URL
.\Generate-Token.ps1 -ApiBaseUrl "https://your-api:7001"
```

**Output:** Displays JWT token, optionally copies to clipboard, optionally shows decoded payload

---

### 2. test-auth.ps1

Tests JWT authentication with 4 comprehensive tests.

**Tests performed:**
- ✓ Token generation via `/scim/auth/token`
- ✓ Rejection without token (HTTP 401)
- ✓ Access to protected endpoints with valid token
- ✓ Rejection with invalid token (HTTP 401)

**Usage:**
```powershell
# Simple test
.\test-auth.ps1

# With custom API URL
.\test-auth.ps1 -ApiBaseUrl "https://your-api:7001"

# Verbose mode
.\test-auth.ps1 -Verbose
```

**Output:** Detailed test report with success rate

---

### 3. Setup-KeyVault.ps1

Configures Azure Key Vault for production JWT secret management.

**Prerequisites:**
- Azure CLI installed
- Azure authenticated (`az login`)
- Key Vault exists

**Usage:**
```powershell
# Generate new secret key
.\Setup-KeyVault.ps1 -KeyVaultName "mykeyvault" -GenerateNewKey

# With specific resource group
.\Setup-KeyVault.ps1 -KeyVaultName "mykeyvault" `
  -ResourceGroup "my-rg" -GenerateNewKey

# Use existing key
.\Setup-KeyVault.ps1 -KeyVaultName "mykeyvault" `
  -ExistingKey "your-32-char-secret-key"
```

**Actions performed:**
- Verifies Azure authentication
- Validates Key Vault exists
- Generates secure secret key
- Creates `Jwt-SecretKey` secret
- Displays Managed Identity instructions

---

### 4. verify-implementation.ps1

Verifies that all JWT implementation files and configurations are in place.

**Checks:**
- 12+ files created
- 8 files modified
- 5 NuGet packages
- JWT configuration
- [Authorize] attributes
- Mocked tests
- Documentation

**Usage:**
```powershell
# Simple verification
.\verify-implementation.ps1

# Custom path
.\verify-implementation.ps1 -ScimworkDir "C:\path\to\scimwork"
```

**Output:** Colorized report with checklist

---

## Typical Workflow

### Development

```powershell
# 1. Verify everything is set up
.\verify-implementation.ps1

# 2. Compile
dotnet build

# 3. Run tests
dotnet test

# 4. Start application (Window 1)
dotnet run

# 5. Generate token (Window 2)
.\Generate-Token.ps1 -CopyToClipboard

# 6. Test authentication
.\test-auth.ps1
```

### Production Setup

```powershell
# 1. Configure Key Vault
.\Setup-KeyVault.ps1 -KeyVaultName "myvault" -GenerateNewKey

# 2. Generate JWT for Entra
# Use secure CLI app to generate JWT with Key Vault secret

# 3. Configure Entra ID
# Admin Credentials → Secret Token → Bearer <jwt>

# 4. Validate
# Test Connection → Success ✓
```

---

## Tips & Tricks

### Helper Function

Add to your PowerShell profile (`$PROFILE`):

```powershell
function Get-ScimToken {
    .\Generate-Token.ps1 -CopyToClipboard
}

function Test-Scim {
    .\test-auth.ps1
}

# Usage
Get-ScimToken
Test-Scim
```

### Testing Loop

```powershell
while ($true) {
    .\test-auth.ps1
    Start-Sleep -Seconds 30
}
```

### Use Token with cURL

```powershell
$token = (.\Generate-Token.ps1 | Select-Object -Last 1)
curl.exe -H "Authorization: Bearer $token" `
  https://localhost:7001/scim/Users
```

---

## Error Handling

All scripts include:
- ✅ Clear error messages
- ✅ Helpful suggestions
- ✅ Color-coded output
- ✅ Exit codes for automation

**Example:** If Azure CLI is missing, script suggests installation link

---

## Features

- ✅ PowerShell only (no Bash/shell scripts)
- ✅ English comments and messages
- ✅ Colorized output with emojis
- ✅ Robust error handling
- ✅ Production-ready
- ✅ Azure Key Vault integration
- ✅ Windows native (works with PowerShell 5.1+)

---

## Quick Start

```powershell
cd C:\Users\MichelPerfetti\src\private\scimwork

# Verify implementation
.\verify-implementation.ps1

# Generate token
.\Generate-Token.ps1

# Test authentication
.\test-auth.ps1
```

---

## Support

Each script includes help:

```powershell
Get-Help .\Generate-Token.ps1 -Full
Get-Help .\test-auth.ps1 -Full
Get-Help .\Setup-KeyVault.ps1 -Full
Get-Help .\verify-implementation.ps1 -Full
```

---

**Status:** ✅ All scripts in PowerShell with English documentation
