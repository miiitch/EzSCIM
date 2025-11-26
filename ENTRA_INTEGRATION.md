# Guide d'Intégration Microsoft Entra (Azure AD)

Ce guide explique comment configurer l'approvisionnement automatique des utilisateurs et groupes depuis Microsoft Entra (anciennement Azure Active Directory) vers votre application via SCIM 2.0.

## 📋 Prérequis

- Application enregistrée dans Microsoft Entra
- API SCIM déployée et accessible (HTTPS requis pour la production)
- Token d'authentification Bearer (recommandé)

## 🔧 Configuration dans Microsoft Entra

### Étape 1 : Accéder aux Applications d'Entreprise

1. Connectez-vous au [Portail Azure](https://portal.azure.com)
2. Allez dans **Microsoft Entra ID** (anciennement Azure Active Directory)
3. Cliquez sur **Applications d'entreprise**
4. Créez une nouvelle application ou sélectionnez une application existante

### Étape 2 : Configurer l'Approvisionnement

1. Dans votre application, allez dans **Approvisionnement** (Provisioning)
2. Cliquez sur **Prise en main** (Get started)
3. Sélectionnez le mode d'approvisionnement : **Automatique**

### Étape 3 : Configurer l'URL du Tenant et l'Authentification

#### URL du Tenant
```
https://votre-serveur.com/scim
```

Pour le développement local avec ngrok :
```
https://your-subdomain.ngrok.io/scim
```

#### Token Secret
Générez un token Bearer et configurez-le. Pour la production, utilisez un token JWT sécurisé.

Pour le développement, vous pouvez utiliser un token simple :
```
Bearer my-secret-token-12345
```

### Étape 4 : Tester la Connexion

1. Cliquez sur **Tester la connexion**
2. Azure va appeler les endpoints suivants :
   - `GET /scim/ServiceProviderConfig`
   - `GET /scim/Schemas`
   - `GET /scim/Users?filter=userName eq "testuser@domain.com"`

Si tout est correct, vous verrez un message de succès ✅

### Étape 5 : Configurer les Mappages d'Attributs

#### Mappages Utilisateur (User)

Mappages recommandés :

| Attribut Azure AD | Attribut SCIM | Type |
|-------------------|---------------|------|
| userPrincipalName | userName | Direct |
| objectId | externalId | Direct |
| displayName | displayName | Direct |
| givenName | name.givenName | Direct |
| surname | name.familyName | Direct |
| mail | emails[type eq "work"].value | Direct |
| accountEnabled | active | Direct |
| jobTitle | title | Direct |
| department | urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:department | Direct |
| employeeId | urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:employeeNumber | Direct |

#### Mappages Groupe (Group)

| Attribut Azure AD | Attribut SCIM | Type |
|-------------------|---------------|------|
| objectId | externalId | Direct |
| displayName | displayName | Direct |
| members | members | Direct |

### Étape 6 : Définir l'Étendue d'Approvisionnement

Choisissez qui sera approvisionné :

1. **Synchroniser uniquement les utilisateurs et groupes attribués** (recommandé)
   - Vous assignez manuellement les utilisateurs/groupes à synchroniser
   
2. **Synchroniser tous les utilisateurs et groupes**
   - Tous les utilisateurs de l'annuaire seront synchronisés

### Étape 7 : Lancer l'Approvisionnement

1. Sauvegardez la configuration
2. Activez l'approvisionnement
3. Cliquez sur **Démarrer l'approvisionnement**

Le premier cycle de synchronisation commence (peut prendre 20-40 minutes).

## 🔄 Cycles d'Approvisionnement

### Synchronisation Initiale
- Durée : 20-40 minutes (dépend du nombre d'utilisateurs)
- Microsoft Entra lit tous les utilisateurs/groupes dans l'étendue
- Compare avec les utilisateurs existants dans votre API SCIM
- Crée/met à jour les utilisateurs nécessaires

### Synchronisation Incrémentielle
- Fréquence : Toutes les 40 minutes par défaut
- Synchronise uniquement les changements depuis la dernière synchronisation

## 🔍 Opérations SCIM Utilisées par Microsoft Entra

### 1. Vérification d'Existence Utilisateur
```http
GET /scim/Users?filter=userName eq "user@domain.com"
```
Microsoft Entra vérifie si l'utilisateur existe avant de le créer.

### 2. Création d'Utilisateur
```http
POST /scim/Users
Content-Type: application/scim+json

{
  "schemas": [
    "urn:ietf:params:scim:schemas:core:2.0:User",
    "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User"
  ],
  "userName": "john.doe@domain.com",
  "externalId": "azure-object-id-123",
  "name": {
    "givenName": "John",
    "familyName": "Doe"
  },
  "active": true,
  "emails": [
    {
      "value": "john.doe@domain.com",
      "type": "work",
      "primary": true
    }
  ]
}
```

### 3. Mise à Jour d'Utilisateur (PATCH)
```http
PATCH /scim/Users/{id}
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    {
      "op": "replace",
      "path": "active",
      "value": false
    }
  ]
}
```

### 4. Vérification d'Existence Groupe
```http
GET /scim/Groups?filter=displayName eq "GroupName"
```

### 5. Création de Groupe
```http
POST /scim/Groups
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:schemas:core:2.0:Group"],
  "displayName": "Administrators",
  "externalId": "azure-group-id-456",
  "members": [
    {
      "value": "user-id-123",
      "display": "John Doe"
    }
  ]
}
```

### 6. Ajout de Membre à un Groupe
```http
PATCH /scim/Groups/{groupId}
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    {
      "op": "add",
      "path": "members",
      "value": [
        {
          "value": "user-id-123"
        }
      ]
    }
  ]
}
```

### 7. Retrait de Membre d'un Groupe
```http
PATCH /scim/Groups/{groupId}
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    {
      "op": "remove",
      "path": "members[value eq \"user-id-123\"]"
    }
  ]
}
```

### 8. Suppression d'Utilisateur
```http
DELETE /scim/Users/{id}
```

## 📊 Surveillance et Journaux

### Journaux d'Approvisionnement dans Azure

1. Allez dans votre application d'entreprise
2. Cliquez sur **Journaux d'approvisionnement** (Provisioning logs)
3. Vous verrez toutes les opérations :
   - ✅ Succès
   - ⚠️ Avertissements
   - ❌ Échecs

### Codes de Retour Attendus

| Code | Signification | Action Azure |
|------|---------------|--------------|
| 200 | OK | Opération réussie |
| 201 | Created | Ressource créée avec succès |
| 204 | No Content | Suppression réussie |
| 404 | Not Found | Ressource non trouvée |
| 409 | Conflict | Ressource existe déjà (userName/displayName dupliqué) |
| 500 | Server Error | Erreur interne, Azure va réessayer |

## 🔐 Sécurité

### Production
Pour la production, implémentez :

1. **HTTPS obligatoire**
2. **Authentification Bearer Token JWT**
3. **Rate limiting** pour éviter les abus
4. **Validation stricte des entrées**
5. **Audit logging** de toutes les opérations

### Exemple d'Authentification Bearer

```csharp
// Dans Program.cs
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://login.microsoftonline.com/{tenant-id}";
        options.Audience = "api://your-api-id";
    });

// Ajouter [Authorize] sur vos controllers
[Authorize]
[ApiController]
[Route("scim/[controller]")]
public class UsersController : ControllerBase
```

## 🐛 Dépannage

### Erreur : "Test connection failed"

**Causes possibles :**
1. URL du tenant incorrecte
2. Token d'authentification invalide
3. API non accessible (firewall, HTTPS requis)
4. Endpoints SCIM non conformes

**Solutions :**
- Vérifiez l'URL : doit se terminer par `/scim`
- Testez manuellement avec Postman/curl
- Vérifiez les logs de votre API

### Erreur : "userName already exists" (409)

**Cause :** Un utilisateur avec le même `userName` existe déjà.

**Solution :** Microsoft Entra utilisera PATCH pour mettre à jour l'utilisateur existant.

### Erreur : "Schema not found"

**Cause :** Les schémas customs ne sont pas chargés correctement.

**Solution :** Vérifiez `appsettings.Scim.json` et les logs au démarrage.

### Les utilisateurs ne se synchronisent pas

**Causes possibles :**
1. Utilisateurs pas dans l'étendue d'approvisionnement
2. Mappages d'attributs incorrects
3. Filtres de groupe/utilisateur restrictifs

**Solutions :**
- Vérifiez l'affectation des utilisateurs à l'application
- Consultez les journaux d'approvisionnement
- Vérifiez les règles d'étendue

## 📚 Références

- [Documentation Microsoft SCIM](https://learn.microsoft.com/en-us/azure/active-directory/app-provisioning/use-scim-to-provision-users-and-groups)
- [Spécification SCIM 2.0 (RFC 7644)](https://datatracker.ietf.org/doc/html/rfc7644)
- [SCIM Protocol (RFC 7644)](https://datatracker.ietf.org/doc/html/rfc7644)
- [SCIM Schema (RFC 7643)](https://datatracker.ietf.org/doc/html/rfc7643)

## 🎯 Checklist de Production

Avant de déployer en production, assurez-vous que :

- [ ] HTTPS est activé avec certificat valide
- [ ] Authentification Bearer Token JWT implémentée
- [ ] Rate limiting configuré
- [ ] Logs d'audit activés
- [ ] Base de données persistante (remplacer InMemory)
- [ ] Tests de charge effectués
- [ ] Sauvegarde et restauration configurées
- [ ] Monitoring et alertes en place
- [ ] Documentation API mise à jour
- [ ] Tests end-to-end avec Azure passés avec succès

