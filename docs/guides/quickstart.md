# Quick Start - JWT Authentication

## Getting Started (5 minutes)

### 1️⃣ Start the Application

```bash
cd C:\Users\MichelPerfetti\src\private\scimwork
dotnet run
```

The application starts on: `https://localhost:7001`

### 2️⃣ Generate a Token (PowerShell)

Before using the endpoint, enable it in your application startup:

```csharp
builder.Services.AddScimControllers();
builder.Services.AddScimTokenGeneratorEndpoint();
```

```powershell
$response = Invoke-RestMethod -Uri "https://localhost:7001/scim/auth/token" -Method Get
$token = $response.token
Write-Host "Token generated: $token"
```

Result: `{"token":"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...","expiresIn":"60 minutes"}`

### 3️⃣ Use the Token

```powershell
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/scim+json"
}

# Get users
Invoke-RestMethod -Uri "https://localhost:7001/scim/Users" `
    -Headers $headers -Method Get | ConvertTo-Json
```

### ✅ Success!

If you see the list of users, authentication is working! 🎉

---

## Test Without Token (Should return 401)

```powershell
Invoke-RestMethod -Uri "https://localhost:7001/scim/Users" -Method Get
# Result: HTTP 401 Unauthorized ✓
```

---

## Use the Complete Test Script

```powershell
# Windows
.\test-auth.ps1

# Linux/macOS
./test-auth.sh https://localhost:7001
```

This script:
- ✓ Generates a token
- ✓ Tests access to 5 endpoints
- ✓ Validates protections
- ✓ Displays summary

---

## Run Unit Tests

```bash
dotnet test
```

All tests pass with mocked authentication.

---

## Production Configuration

### 1. Generate Secret Key

```bash
openssl rand -hex 32
# Result: abc123def456789...
```

### 2. Create Secret in Azure Key Vault

```bash
az keyvault secret set \
  --vault-name "your-keyvault" \
  --name "Jwt-SecretKey" \
  --value "abc123def456..."
```

### 3. Configure Application

Update `appsettings.Production.json`:

```json
{
  "AzureKeyVault": {
    "VaultUri": "https://your-keyvault.vault.azure.net/"
  }
}
```

### 4. Configure Entra ID

1. Generate JWT: `openssl` or .NET script
2. Copy the complete token
3. Entra ID → Provisioning → **Secret Token**
4. Paste: `Bearer eyJ...`
5. **Test Connection** → ✓ Success

---

## JWT Structure

### Header
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### Payload
```json
{
  "sub": "scim-client",
  "jti": "unique-guid",
  "exp": 1234567890
}
```

### Signature
```
HMACSHA256(base64(header) + "." + base64(payload), secretKey)
```

---

## Key Endpoints

| URL | Auth | Description |
|-----|------|-------------|
| `GET /scim/auth/token` | ✗ No | Generate token (dev-only, opt-in via `AddScimTokenGeneratorEndpoint()`) |
| `GET /scim/ServiceProviderConfig` | ✓ Yes | SCIM server config |
| `GET /scim/Schemas` | ✓ Yes | Available schemas |
| `GET /scim/Users` | ✓ Yes | List users |
| `POST /scim/Users` | ✓ Yes | Create user |

---

## Quick Troubleshooting

| Issue | Solution |
|-------|----------|
| **HTTP 401** | Check `Authorization: Bearer <token>` header |
| **Invalid token** | Regenerate with `/scim/auth/token` |
| **403 on /scim/auth/token** | Endpoint is opt-in; enable via `AddScimTokenGeneratorEndpoint()` |
| **Key Vault error** | Verify Managed Identity + URI |

---

## Resources

- 📚 [Authentication Setup](./auth/setup.md) - Detailed configuration
- 📚 [Pre-Production Checklist](./auth/pre-production-checklist.md) - Before going live
- 🧪 [test-auth.ps1](../../../test-auth.ps1) - Complete test script

---

**Status:** ✅ Ready to Go! Start with step 1 above.

