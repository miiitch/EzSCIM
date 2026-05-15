# Authentication Reference

Full guide: https://ezscim.miiitch.dev/authentication/

EzSCIM uses **JWT Bearer token** authentication (HS256). All SCIM endpoints require
`Authorization: Bearer <token>`.

---

## Register in `Program.cs`

```csharp
// JWT token service
builder.Services.AddJwtTokenService();

// Bearer authentication handler
builder.Services.AddAuthentication()
    .AddScheme<JwtBearerTokenAuthenticationOptions, JwtBearerTokenAuthenticationHandler>(
        "Bearer", null);

builder.Services.AddAuthorization();

// Development-only token endpoint: GET /scim/auth/token
// Disabled automatically in production
builder.Services.AddScimTokenGeneratorEndpoint();
```

---

## Configure `appsettings.json`

```json
{
  "Jwt": {
    "SecretKey": "<at-least-32-char-secret — never commit>",
    "ExpirationMinutes": 1440
  }
}
```

**Production**: load the secret from Azure Key Vault:

```csharp
if (!builder.Environment.IsDevelopment())
{
    var kvUrl = builder.Configuration["AzureKeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(kvUrl))
        builder.Configuration.AddAzureKeyVault(new Uri(kvUrl), new DefaultAzureCredential());
}
```

---

## Generate a token (development)

```bash
# cURL
curl -s https://localhost:7001/scim/auth/token

# PowerShell
$token = (Invoke-RestMethod -Uri "https://localhost:7001/scim/auth/token").token
```

---

## Configure Entra ID

1. Azure Portal → **Entra ID → Enterprise Applications → your app → Provisioning**
2. **Tenant URL**: `https://your-domain.com/scim`
3. **Secret Token**: `Bearer <jwt-token>` (paste the full string with `Bearer ` prefix)
4. Click **Test Connection**

For a long-lived token (Entra ID provisioning jobs run on their own schedule):
https://ezscim.miiitch.dev/authentication/#6-configure-entra-id-microsoft
