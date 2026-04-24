# JWT Authentication Setup Guide

## Overview

The SCIM API uses **JWT Bearer Token** authentication to secure all endpoints. JWT tokens are signed with an HS256 secret key.

- **Development**: Tokens are generated via the `/scim/auth/token` endpoint
- **Production**: Tokens must be generated and stored securely (the secret key is stored in Azure Key Vault)

## Development Configuration

### 1. Start the Application

```bash
cd ScimAPI
dotnet run
```

The application starts with the `appsettings.Development.json` configuration:
- Development secret key: `dev-secret-key-12345678901234567890`
- Expiration duration: 1440 minutes (24 hours)

### 2. Generate a Token

#### Via PowerShell (Windows)

```powershell
$ApiUrl = "https://localhost:7001"
$response = Invoke-RestMethod -Uri "$ApiUrl/scim/auth/token" -Method Get
$token = $response.token
Write-Host "Token: $token"
```

#### Via Bash/cURL (macOS/Linux)

```bash
API_URL="https://localhost:7001"
TOKEN=$(curl -s "$API_URL/scim/auth/token" | jq -r '.token')
echo "Token: $TOKEN"
```

#### Via Provided PowerShell Script

```powershell
.\test-auth.ps1 -ApiBaseUrl "https://localhost:7001"
```

### 3. Use the Token

Include the token in the `Authorization` header:

```powershell
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/scim+json"
}

Invoke-RestMethod -Uri "https://localhost:7001/scim/Users" `
    -Headers $headers `
    -Method Get
```

Or with cURL:

```bash
curl -X GET "https://localhost:7001/scim/Users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/scim+json"
```

## Production Configuration

### 1. Prepare the Secret Key

Generate a secure secret key (minimum 32 characters):

```bash
# Linux/macOS
openssl rand -hex 32

# Windows PowerShell
[System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).Guid + (New-Guid).Guid))
```

Result: `abc123def456789012345678901234567890...`

### 2. Store in Azure Key Vault

```bash
az login
az keyvault secret set \
  --vault-name "your-keyvault-name" \
  --name "Jwt-SecretKey" \
  --value "abc123def456..."
```

### 3. Configure the Application

Create or update `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "SecretKey": "this-will-be-loaded-from-keyvault",
    "ExpirationMinutes": 60
  },
  "AzureKeyVault": {
    "VaultUri": "https://your-keyvault-name.vault.azure.net/"
  }
}
```

### 4. Configure Managed Identity (Azure)

The application must have access to Key Vault via Managed Identity:

```bash
# Create Managed Identity
az identity create -g your-resource-group -n scim-api-identity

# Assign access to Key Vault
az keyvault set-policy \
  --name your-keyvault-name \
  --object-id <MANAGED_IDENTITY_PRINCIPAL_ID> \
  --secret-permissions get list
```

### 5. Generate JWT for Entra ID

**Option A**: Via CLI application (recommended)

Create a small script/application to generate the JWT:

```csharp
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var secretKey = "your-secret-key";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, "scim-client"),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};

var token = new JwtSecurityToken(
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(60),
    signingCredentials: credentials
);

var handler = new JwtSecurityTokenHandler();
var jwt = handler.WriteToken(token);
Console.WriteLine($"Bearer {jwt}");
```

**Option B**: Via JWT.io (development only)

1. Go to https://jwt.io
2. Header: `{"alg":"HS256","typ":"JWT"}`
3. Payload: `{"sub":"scim-client","jti":"unique-id","exp":1234567890}`
4. Secret: `your-secret-key`
5. Copy the complete token

### 6. Configure Entra ID

1. Go to **Azure Portal → Microsoft Entra ID → Enterprise Applications**
2. Select your SCIM application
3. **Provisioning → Admin Credentials**
4. **Tenant URL**: `https://your-domain.com/scim`
5. **Secret Token**: `Bearer eyJ...` (the full JWT)
6. Click **Test Connection**

## Common Issues

### Invalid/Expired Token in Production

**Cause**: The secret key stored in Key Vault does not match the key used to sign the JWT.

**Solution**:
- Verify that the JWT was signed with the same secret key
- Regenerate and redeploy the JWT in Entra

### HTTP 401 Unauthorized Everywhere

**Possible Causes**:
1. Token missing from the Authorization header
2. Incorrect format: must be `Bearer <token>` (with a space)
3. Token expired
4. Token invalid or tampered

**Verification**:
```bash
curl -v https://localhost:7001/scim/Users \
  -H "Authorization: Bearer $TOKEN"
```

Check:
- `Authorization` header is present
- Token starts with `eyJ` (base64)
- Format: `Authorization: Bearer eyJ...`

### /scim/auth/token Endpoint Returns 403 in Production

**This is expected behavior.** This endpoint is only accessible in development. In production, generate the token via a secure CLI application.

### Secret Key Is Not Loaded from Key Vault

**Check**:
1. Environment variable `ASPNETCORE_ENVIRONMENT=Production`
2. Managed Identity has access to Key Vault
3. Correct Key Vault URI in `appsettings.Production.json`
4. Logs for Key Vault connection errors

```bash
# Verify Key Vault access
az keyvault secret list --vault-name your-keyvault-name
```

## Unit Tests

Tests include authentication mocking via `AuthenticationTestHelper`:

```bash
cd ScimAPI.Tests
dotnet test
```

All tests pass with mocked authentication (no need for real tokens).

## Security Checklist

- [ ] Secret key minimum 32 characters
- [ ] Secret key never committed to Git
- [ ] Secret key stored in Azure Key Vault in production
- [ ] Managed Identity configured for Key Vault access
- [ ] Tokens expire after 60 minutes by default
- [ ] HTTPS required in production
- [ ] HTTP 401 returned for unauthenticated requests
- [ ] JWT error logs enabled for audit
- [ ] JWT token does not contain sensitive information

## References

- [JWT.io](https://jwt.io) - JWT decoder/validator
- [Microsoft.IdentityModel.Tokens](https://www.nuget.org/packages/Microsoft.IdentityModel.Tokens/)
- [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/)
- [SCIM 2.0 Specification](https://tools.ietf.org/html/rfc7644)

