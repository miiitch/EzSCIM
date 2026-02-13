# 🎯 Tests d'Intégration Entra ID (Azure AD)

**Date**: 2026-02-13  
**Statut**: ✅ **17 tests Entra ID passent (100%)**

---

## 📋 Résumé

Tests d'intégration qui simulent exactement les patterns de requêtes envoyées par Microsoft Entra ID (Azure AD) lors du provisioning SCIM. Ces tests valident la compatibilité complète de l'API SCIM avec Entra ID.

---

## 🏗️ Architecture

### Fichier: `EntraIdRequestPatternsTests.cs`

```csharp
[Collection("EntraIdIntegration")]
public class EntraIdRequestPatternsTests : IClassFixture<ScimWebApplicationFactory>, IAsyncLifetime
{
    // Tests simulant les 6 phases du cycle de provisioning Entra ID
}
```

### Technologies utilisées
- **xUnit** - Framework de tests
- **Testcontainers** - PostgreSQL en conteneur Docker
- **Shouldly** - Assertions fluides
- **PostgreSQL** - Base de données réelle (pas de mock)

---

## 🧪 Tests par Phase

### Phase 1: Test de Connexion (3 tests)
| Test | Description |
|------|-------------|
| `EntraId_TestConnection_ServiceProviderConfig_ShouldReturn200` | Vérifie que GET /ServiceProviderConfig retourne 200 |
| `EntraId_TestConnection_Schemas_ShouldReturn200WithUserAndGroupSchemas` | Vérifie que GET /Schemas retourne User et Group |
| `EntraId_TestConnection_UserLookup_NonExistent_ShouldReturnEmptyList` | Vérifie qu'un utilisateur inexistant retourne totalResults=0 (pas 404) |

### Phase 2: Synchronisation Initiale (2 tests)
| Test | Description |
|------|-------------|
| `EntraId_InitialSync_GetAllUsers_WithPagination_ShouldReturnListResponse` | GET /Users avec pagination |
| `EntraId_InitialSync_GetAllGroups_WithPagination_ShouldReturnListResponse` | GET /Groups avec pagination |

### Phase 3: Provisioning Utilisateur (5 tests)
| Test | Description |
|------|-------------|
| `EntraId_UserProvisioning_CheckByUserName_ThenCreate_FullFlow` | Flow complet: check + create |
| `EntraId_UserProvisioning_CheckByExternalId_ShouldReturnEmptyList` | Recherche par Azure Object ID |
| `EntraId_UserProvisioning_PatchDisable_WhenUserUnassigned` | Désactivation via PATCH active=false |
| `EntraId_UserProvisioning_PatchUpdateMultipleAttributes_ProfileChange` | Mise à jour de plusieurs attributs |
| `EntraId_UserProvisioning_PatchEnterpriseExtension_DepartmentSync` | Sync department, employeeNumber |

### Phase 4: Provisioning Groupe (3 tests)
| Test | Description |
|------|-------------|
| `EntraId_GroupProvisioning_CheckByDisplayName_ThenCreate_FullFlow` | Flow complet: check + create |
| `EntraId_GroupProvisioning_AddMember_UserAssignedToGroup` | PATCH members (Add) |
| `EntraId_GroupProvisioning_RemoveMember_UserRemovedFromGroup` | PATCH members (Remove) |

### Phase 5: Gestion des Erreurs (3 tests)
| Test | Description |
|------|-------------|
| `EntraId_ErrorHandling_GetNonExistentUser_ShouldReturn404` | GET user inexistant → 404 |
| `EntraId_ErrorHandling_CreateDuplicateUser_ShouldReturn409Conflict` | POST duplicate → 409 |
| `EntraId_ErrorHandling_PatchNonExistentUser_ShouldReturn404` | PATCH user inexistant → 404 |

### Phase 6: Cycle Complet (1 test)
| Test | Description |
|------|-------------|
| `EntraId_FullProvisioningCycle_CreateUpdateDisableDeleteUser` | Create → Update → Disable → Delete → Verify |

---

## 📊 Résultats des Tests

```
Réussi! - Échec: 0, réussite: 17, ignorée(s): 0, total: 17
```

---

## 🔄 Séquence des Requêtes Entra ID

### 1. Test de Connexion
```http
GET /scim/ServiceProviderConfig
GET /scim/Schemas
GET /scim/Users?filter=userName eq "test@domain.com"
```

### 2. Sync Initiale
```http
GET /scim/Users?startIndex=1&count=100
GET /scim/Groups?startIndex=1&count=100
```

### 3. Création Utilisateur
```http
GET /scim/Users?filter=userName eq "user@domain.com"
GET /scim/Users?filter=externalId eq "azure-object-id"
POST /scim/Users
```

### 4. Mise à Jour
```http
PATCH /scim/Users/{id}
{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    { "op": "Replace", "path": "active", "value": false }
  ]
}
```

### 5. Gestion Groupes
```http
PATCH /scim/Groups/{id}
{
  "Operations": [
    { "op": "Add", "path": "members", "value": [{"value": "user-id"}] }
  ]
}
```

---

## 💡 Exécution des Tests

### Tous les tests d'intégration
```powershell
cd c:\Users\MichelPerfetti\src\private\scimwork
dotnet test ScimAPI.IntegrationTests
```

### Seulement les tests Entra ID
```powershell
dotnet test ScimAPI.IntegrationTests --filter "FullyQualifiedName~EntraId"
```

---

## 📁 Fichiers

| Fichier | Description |
|---------|-------------|
| `ScimAPI.IntegrationTests/EntraIdRequestPatternsTests.cs` | ✨ Tests Entra ID |
| `ScimAPI.IntegrationTests/ScimWebApplicationFactory.cs` | Factory avec PostgreSQL |
| `ScimAPI.IntegrationTests/Data/ScimDbContext.cs` | DbContext EF Core |

---

## 🔍 Comment Tester avec le Vrai Entra ID?

### Option 1: Utiliser ngrok
```bash
# Démarrer l'API
dotnet run --project ScimAPI

# Dans un autre terminal
ngrok http 5000 --host-header=localhost

# Configurer l'URL ngrok dans Entra ID
# https://xxxx.ngrok-free.app/scim
```

### Option 2: Logging Middleware
```csharp
// Dans Program.cs
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("[ENTRA] {Method} {Path}{Query}",
        context.Request.Method, context.Request.Path, context.Request.QueryString);
    await next();
});
```

---

## ⚠️ Points Importants pour la Compatibilité Entra ID

1. **Recherche vide → 200 avec totalResults=0** (pas 404)
2. **PATCH doit supporter `Replace`, `Add`, `Remove`**
3. **Les attributs `name.givenName`, `name.familyName` doivent être supportés**
4. **L'extension Enterprise est obligatoire** (department, employeeNumber)
5. **Les membres de groupe doivent supporter add/remove via PATCH**

---

**Implémentation complète** ✅  
**Tests: 17/17 passent** ✅  
**Compatible Entra ID: Oui** ✅

