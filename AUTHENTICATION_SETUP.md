# Guide de Configuration de l'Authentification JWT

## Vue d'ensemble

L'API SCIM utilise l'authentification **JWT Bearer Token** pour sécuriser tous les endpoints. Les tokens JWT sont signés avec une clé secrète HS256.

- **Développement**: Les tokens sont générés via l'endpoint `/scim/auth/token`
- **Production**: Les tokens doivent être générés et stockés sécurisés (la clé secrète est dans Azure Key Vault)

## Configuration Développement

### 1. Démarrer l'Application

```bash
cd ScimAPI
dotnet run
```

L'application démarre avec la configuration `appsettings.Development.json`:
- Clé secrète de développement: `dev-secret-key-12345678901234567890`
- Durée d'expiration: 1440 minutes (24 heures)

### 2. Générer un Token

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

#### Via PowerShell Script Fourni

```powershell
.\test-auth.ps1 -ApiBaseUrl "https://localhost:7001"
```

### 3. Utiliser le Token

Inclure le token dans le header `Authorization`:

```powershell
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/scim+json"
}

Invoke-RestMethod -Uri "https://localhost:7001/scim/Users" `
    -Headers $headers `
    -Method Get
```

ou avec cURL:

```bash
curl -X GET "https://localhost:7001/scim/Users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/scim+json"
```

## Configuration Production

### 1. Préparer la Clé Secrète

Générer une clé secrète sécurisée (minimum 32 caractères):

```bash
# Linux/macOS
openssl rand -hex 32

# Windows PowerShell
[System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).Guid + (New-Guid).Guid))
```

Résultat: `abc123def456789012345678901234567890...`

### 2. Stocker dans Azure Key Vault

```bash
az login
az keyvault secret set \
  --vault-name "your-keyvault-name" \
  --name "Jwt-SecretKey" \
  --value "abc123def456..."
```

### 3. Configurer l'Application

Créer/mettre à jour `appsettings.Production.json`:

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

### 4. Configurer Managed Identity (Azure)

L'application doit avoir accès à Key Vault via Managed Identity:

```bash
# Créer Managed Identity
az identity create -g your-resource-group -n scim-api-identity

# Assigner accès à Key Vault
az keyvault set-policy \
  --name your-keyvault-name \
  --object-id <MANAGED_IDENTITY_PRINCIPAL_ID> \
  --secret-permissions get list
```

### 5. Générer JWT pour Entra ID

**Option A**: Via Application CLI (recommandée)

Créer un petit script/application pour générer le JWT:

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

**Option B**: Via JWT.io (développement uniquement)

1. Aller à https://jwt.io
2. Header: `{"alg":"HS256","typ":"JWT"}`
3. Payload: `{"sub":"scim-client","jti":"unique-id","exp":1234567890}`
4. Secret: `your-secret-key`
5. Copier le token complet

### 6. Configurer Entra ID

1. Allez à **Azure Portal → Microsoft Entra ID → Applications d'entreprise**
2. Sélectionnez votre application SCIM
3. **Approvisionnement → Admin Credentials**
4. **Tenant URL**: `https://your-domain.com/scim`
5. **Secret Token**: `Bearer eyJ...` (le JWT complet)
6. Cliquez **Test Connection**

## Problèmes Courants

### Token invalide/expiré en production

**Cause**: La clé secrète stockée en Key Vault ne correspond pas à celle utilisée pour signer le JWT

**Solution**: 
- Vérifier que le JWT a été signé avec la même clé secrète
- Régénérer et redéployer le JWT dans Entra

### HTTP 401 Unauthorized partout

**Causes possibles**:
1. Token manquant du header Authorization
2. Format incorrect: doit être `Bearer <token>` (avec espace)
3. Token expiré
4. Token invalide/tampered

**Vérification**:
```bash
curl -v https://localhost:7001/scim/Users \
  -H "Authorization: Bearer $TOKEN"
```

Regarder:
- Header `Authorization` est présent
- Token commence par `eyJ` (base64)
- Format: `Authorization: Bearer eyJ...`

### Endpoint /scim/auth/token retourne 403 en production

**C'est normal**. Cet endpoint est accessible uniquement en développement. En production, générer le token via une application CLI sécurisée.

### La clé secrète n'est pas chargée depuis Key Vault

**Vérifier**:
1. Variable d'environnement `ASPNETCORE_ENVIRONMENT=Production`
2. Managed Identity a accès à Key Vault
3. URI Key Vault correct dans `appsettings.Production.json`
4. Logs pour erreurs de connexion Key Vault

```bash
# Vérifier accès Key Vault
az keyvault secret list --vault-name your-keyvault-name
```

## Tests Unitaires

Les tests incluent le mock de l'authentification via `AuthenticationTestHelper`:

```bash
cd ScimAPI.Tests
dotnet test
```

Tous les tests passent avec l'authentification mocée (sans besoin de tokens réels).

## Sécurité - Checklist

- [ ] Clé secrète minimum 32 caractères
- [ ] Clé secrète ne jamais commitée dans Git
- [ ] Clé secrète stockée dans Azure Key Vault en production
- [ ] Managed Identity configurée pour accès Key Vault
- [ ] Tokens expirent après 60 minutes par défaut
- [ ] HTTPS requis en production
- [ ] HTTP 401 retourné pour requêtes non authentifiées
- [ ] Logs d'erreurs JWT activés pour audit
- [ ] Token JWT ne contient pas d'infos sensibles

## Références

- [JWT.io](https://jwt.io) - Décodeur/validateur JWT
- [Microsoft.IdentityModel.Tokens](https://www.nuget.org/packages/Microsoft.IdentityModel.Tokens/)
- [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/)
- [SCIM 2.0 Specification](https://tools.ietf.org/html/rfc7644)

