﻿# 🚀 Quick Start - Authentification JWT

## Démarrage Rapide (5 minutes)

### 1️⃣ Démarrer l'Application

```bash
cd C:\Users\MichelPerfetti\src\private\scimwork
dotnet run
```

L'application démarre sur: `https://localhost:7001`

### 2️⃣ Générer un Token (PowerShell)

Avant d'utiliser le point de terminaison, activez-le dans le démarrage de votre application:

```csharp
builder.Services.AddScimControllers();
builder.Services.AddScimTokenGeneratorEndpoint();
```

```powershell
$response = Invoke-RestMethod -Uri "https://localhost:7001/scim/auth/token" -Method Get
$token = $response.token
Write-Host "Token généré: $token"
```

Résultat: `{"token":"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...","expiresIn":"60 minutes"}`

### 3️⃣ Utiliser le Token

```powershell
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/scim+json"
}

# Récupérer les utilisateurs
Invoke-RestMethod -Uri "https://localhost:7001/scim/Users" `
    -Headers $headers -Method Get | ConvertTo-Json
```

### ✅ Succès!

Si vous voyez la liste des utilisateurs, l'authentification fonctionne! 🎉

---

## Tester sans Token (Doit retourner 401)

```powershell
Invoke-RestMethod -Uri "https://localhost:7001/scim/Users" -Method Get
# Résultat: HTTP 401 Unauthorized ✓
```

---

## Utiliser le Script de Test Complet

```powershell
# Windows
.\test-auth.ps1

# Linux/macOS
./test-auth.sh https://localhost:7001
```

Ce script:
- ✓ Génère un token
- ✓ Teste accès à 5 endpoints
- ✓ Valide protections
- ✓ Affiche résumé

---

## Exécuter les Tests Unitaires

```bash
dotnet test
```

Tous les tests passent avec authentification mocée.

---

## Configuration Production

### 1. Générer Clé Secrète

```bash
openssl rand -hex 32
# Résultat: abc123def456789...
```

### 2. Créer Secret dans Azure Key Vault

```bash
az keyvault secret set \
  --vault-name "your-keyvault" \
  --name "Jwt-SecretKey" \
  --value "abc123def456..."
```

### 3. Configurer Application

Mettre à jour `appsettings.Production.json`:

```json
{
  "AzureKeyVault": {
    "VaultUri": "https://your-keyvault.vault.azure.net/"
  }
}
```

### 4. Configurer Entra ID

1. Générer JWT: `openssl` ou script .NET
2. Copier le token complet
3. Entra ID → Approvisionnement → **Secret Token**
4. Coller: `Bearer eyJ...`
5. **Test Connection** → ✓ Succès

---

## Structure JWT

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

## Endpoints Clés

| URL | Auth | Description |
|-----|------|-------------|
| `GET /scim/auth/token` | ✗ Non | Generate token (dev-only, opt-in via `AddScimTokenGeneratorEndpoint()`) |
| `GET /scim/ServiceProviderConfig` | ✓ Oui | Config du serveur SCIM |
| `GET /scim/Schemas` | ✓ Oui | Schémas disponibles |
| `GET /scim/Users` | ✓ Oui | Liste utilisateurs |
| `POST /scim/Users` | ✓ Oui | Créer utilisateur |

---

## Dépannage Rapide

| Problème | Solution |
|----------|----------|
| **HTTP 401** | Vérifier `Authorization: Bearer <token>` |
| **Token invalide** | Régénérer avec `/scim/auth/token` |
| **403 sur /scim/auth/token** | Endpoint is opt-in; enable via `AddScimTokenGeneratorEndpoint()` |
| **Key Vault erreur** | Vérifier Managed Identity + URI |

---

## Ressources

- 📚 [AUTHENTICATION_SETUP.md](./AUTHENTICATION_SETUP.md) - Configuration détaillée
- 📚 [TODO_AUTH.md](./TODO_AUTH.md) - Checklist avant prod
- 📚 [IMPLEMENTATION_COMPLETE.md](./IMPLEMENTATION_COMPLETE.md) - Vue d'ensemble technique
- 🧪 [test-auth.ps1](./test-auth.ps1) - Script de test complet

---

**Status:** ✅ Ready to Go! Commencez par l'étape 1 ci-dessus.
