# Authentication

EzSCIM uses **JWT Bearer token** authentication (HS256). All SCIM endpoints require a valid
`Authorization: Bearer <token>` header.

---

## 1. Installation

Register the JWT token service in `Program.cs`:

```csharp
// Register JWT token service
builder.Services.AddJwtTokenService();

// Configure JWT authentication
builder.Services.AddAuthentication()
    .AddScheme<JwtBearerTokenAuthenticationOptions, JwtBearerTokenAuthenticationHandler>(
        "Bearer", null);

builder.Services.AddAuthorization();
```

For **development only**, you can enable a token generation endpoint:

```csharp
// Exposes GET /scim/auth/token (disabled in production automatically)
builder.Services.AddScimTokenGeneratorEndpoint();
```

---

## 2. Configuration

### appsettings.json (development)

```json
{
  "Jwt": {
    "SecretKey": "dev-secret-key-12345678901234567890",
    "ExpirationMinutes": 1440
  }
}
```

### appsettings.Production.json

```json
{
  "Jwt": {
    "SecretKey": "loaded-from-key-vault",
    "ExpirationMinutes": 60
  },
  "AzureKeyVault": {
    "VaultUri": "https://your-keyvault.vault.azure.net/"
  }
}
```

---

## 3. Generate a token (development)

```bash
# cURL
curl -s https://localhost:7001/scim/auth/token | jq '.token'

# PowerShell
$token = (Invoke-RestMethod -Uri "https://localhost:7001/scim/auth/token").token
```

Response:
```json
{ "token": "eyJ0eXAiOiJKV1Qi...", "expiresIn": "1440 minutes" }
```

---

## 4. Use the token

```bash
curl -H "Authorization: Bearer $TOKEN" https://localhost:7001/scim/Users
```

```powershell
$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/scim+json" }
Invoke-RestMethod -Uri "https://localhost:7001/scim/Users" -Headers $headers
```

---

## 5. Production: Azure Key Vault

### Generate a secret key

```bash
# Linux/macOS
openssl rand -hex 32

# PowerShell
[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes((New-Guid).ToString() + (New-Guid).ToString()))
```

### Store in Key Vault

```bash
az keyvault secret set \
  --vault-name "your-keyvault" \
  --name "Jwt-SecretKey" \
  --value "your-generated-secret"
```

### Configure the application to read from Key Vault

In `Program.cs`:

```csharp
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUrl = builder.Configuration["AzureKeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
    }
}
```

### Configure Managed Identity

```bash
az identity create -g your-rg -n scim-api-identity

az keyvault set-policy \
  --name your-keyvault \
  --object-id <MANAGED_IDENTITY_PRINCIPAL_ID> \
  --secret-permissions get list
```

---

## 6. Configure Entra ID (Microsoft)

1. **Azure Portal → Microsoft Entra ID → Enterprise Applications**
2. Select your SCIM application → **Provisioning → Admin Credentials**
3. **Tenant URL**: `https://your-domain.com/scim`
4. **Secret Token**: paste the full JWT prefixed with `Bearer ` (e.g. `Bearer eyJ...`)
5. Click **Test Connection**

To generate a long-lived token for Entra ID:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var secretKey = "your-production-secret-key";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var token = new JwtSecurityToken(
    claims: new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, "scim-client"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    },
    expires: DateTime.UtcNow.AddMinutes(525600), // 1 year
    signingCredentials: credentials
);

Console.WriteLine("Bearer " + new JwtSecurityTokenHandler().WriteToken(token));
```

---

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `HTTP 401` on all requests | Token missing or wrong format | Ensure `Authorization: Bearer <token>` header |
| `HTTP 403` on `/scim/auth/token` | Expected in production | Generate token via CLI script |
| Token rejected | Secret mismatch between signing and validation | Verify `Jwt:SecretKey` config matches signing key |
| Key Vault error at startup | Managed Identity not configured | Check identity assignment and Key Vault access policy |

---

## Security checklist

- [ ] Secret key is at least 32 characters
- [ ] Secret key is never committed to Git
- [ ] Secret key is stored in Azure Key Vault in production
- [ ] Managed Identity is configured for Key Vault access
- [ ] Tokens expire (60 minutes recommended in production)
- [ ] HTTPS is enforced in production
- [ ] `/scim/auth/token` returns `403` in production
- [ ] JWT logs do not contain the secret key or token content

---

**Next**: [Repository interfaces →](./iqueryable/repository.md) | [EF Core setup →](./efcore/getting-started.md)

