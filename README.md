﻿# API SCIM 2.0 pour Microsoft Entra (Azure AD)

Cette API implémente le protocole SCIM 2.0 (System for Cross-domain Identity Management) conforme aux exigences de Microsoft Entra (anciennement Azure AD) pour la synchronisation automatique des utilisateurs et groupes.

## 🎯 Fonctionnalités

- ✅ **Gestion des utilisateurs** (création, lecture, mise à jour, suppression, patch)
- ✅ **Gestion des groupes** (création, lecture, mise à jour, suppression, patch)
- ✅ **Support des membres de groupe** (ajout/retrait via PATCH)
- ✅ **Filtrage SCIM avancé** avec opérateurs logiques (AND, OR, NOT)
- ✅ **Opérateurs de comparaison** (eq, ne, co, sw, ew, pr, gt, ge, lt, le)
- ✅ **Filtres sur attributs complexes** (name.givenName, emails.value, etc.)
- ✅ **Pagination** des résultats
- ✅ **Endpoints de découverte** (ServiceProviderConfig, Schemas)
- ✅ **Schémas customs extensibles** (Enterprise User, Custom attributes)
- ✅ **Implémentation en mémoire** avec interface IScimRepository
- ✅ **Compatible Microsoft Entra** (Azure AD provisioning)

## 🏗️ Architecture

### Structure du projet

```
ScimAPI/
├── Controllers/
│   ├── UsersController.cs           # Endpoints SCIM pour les utilisateurs
│   ├── GroupsController.cs          # Endpoints SCIM pour les groupes
│   └── ScimConfigController.cs      # Endpoints de découverte SCIM
├── Models/
│   ├── ScimUser.cs                  # Modèle utilisateur SCIM
│   ├── ScimGroup.cs                 # Modèle groupe SCIM
│   ├── ScimMeta.cs                  # Métadonnées SCIM
│   ├── ScimListResponse.cs          # Réponse de liste paginée
│   ├── ScimError.cs                 # Réponse d'erreur
│   ├── ScimPatchRequest.cs          # Requête PATCH SCIM
│   ├── ScimServiceProviderConfig.cs # Configuration du fournisseur
│   └── ScimSchema.cs                # Définition de schéma
├── Repositories/
│   ├── IScimRepository.cs           # Interface du repository
│   └── InMemoryScimRepository.cs    # Implémentation en mémoire
├── Services/
│   └── ScimSchemaInitializer.cs     # Initialisation des schémas au démarrage
└── appsettings.Scim.json            # Configuration des schémas customs
```

## 🚀 Démarrage rapide

### 1. Lancer l'application

```bash
cd ScimAPI
dotnet run
```

L'API sera accessible sur `https://localhost:7091`

### 2. Tester les endpoints

Utilisez le fichier `ScimAPI.http` avec l'extension REST Client de VS Code ou Rider.

### 3. Endpoints disponibles

#### Découverte SCIM
- `GET /scim/ServiceProviderConfig` - Configuration du fournisseur SCIM
- `GET /scim/Schemas` - Liste de tous les schémas
- `GET /scim/Schemas/{id}` - Détails d'un schéma spécifique
- `POST /scim/Schemas` - Ajouter un schéma custom

#### Utilisateurs
- `GET /scim/Users` - Lister les utilisateurs (avec filtrage et pagination)
- `GET /scim/Users/{id}` - Obtenir un utilisateur
- `POST /scim/Users` - Créer un utilisateur
- `PUT /scim/Users/{id}` - Mettre à jour un utilisateur (complet)
- `PATCH /scim/Users/{id}` - Mettre à jour partiellement un utilisateur
- `DELETE /scim/Users/{id}` - Supprimer un utilisateur

#### Groupes
- `GET /scim/Groups` - Lister les groupes (avec filtrage et pagination)
- `GET /scim/Groups/{id}` - Obtenir un groupe
- `POST /scim/Groups` - Créer un groupe
- `PUT /scim/Groups/{id}` - Mettre à jour un groupe (complet)
- `PATCH /scim/Groups/{id}` - Mettre à jour partiellement un groupe (membres)
- `DELETE /scim/Groups/{id}` - Supprimer un groupe

## 📋 Exemples d'utilisation

### Créer un utilisateur avec des attributs customs

```http
POST /scim/Users
Content-Type: application/scim+json

{
  "schemas": [
    "urn:ietf:params:scim:schemas:core:2.0:User",
    "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User"
  ],
  "userName": "john.doe@example.com",
  "name": {
    "givenName": "John",
    "familyName": "Doe",
    "formatted": "John Doe"
  },
  "displayName": "John Doe",
  "active": true,
  "emails": [
    {
      "value": "john.doe@example.com",
      "type": "work",
      "primary": true
    }
  ],
  "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User": {
    "employeeNumber": "EMP001",
    "department": "IT",
    "costCenter": "CC-IT-001"
  }
}
```

### Filtrer les utilisateurs

```http
# Recherche simple par userName
GET /scim/Users?filter=userName eq "john.doe@example.com"

# Recherche par nom et prénom
GET /scim/Users?filter=name.givenName eq "John" and name.familyName eq "Doe"

# Recherche avec contient
GET /scim/Users?filter=displayName co "Admin"

# Recherche avec commence par
GET /scim/Users?filter=userName sw "john"

# Utilisateurs actifs
GET /scim/Users?filter=active eq true

# Opérateur OR
GET /scim/Users?filter=userName eq "john@example.com" or userName eq "jane@example.com"

# Combinaison complexe avec parenthèses
GET /scim/Users?filter=(userName sw "john" or displayName co "Admin") and active eq true
```

**Voir [SCIM_FILTERS.md](SCIM_FILTERS.md) pour la liste complète des opérateurs et exemples.**

### Ajouter un membre à un groupe

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
          "value": "{userId}",
          "display": "John Doe"
        }
      ]
    }
  ]
}
```

### Désactiver un utilisateur

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

## 🔧 Configuration des schémas customs

Les schémas customs sont définis dans `appsettings.Scim.json`. Vous pouvez ajouter vos propres attributs :

```json
{
  "CustomSchemas": [
    {
      "id": "urn:ietf:params:scim:schemas:extension:mycompany:2.0:User",
      "name": "MyCompanyUser",
      "description": "Extension custom pour mes utilisateurs",
      "attributes": [
        {
          "name": "badgeNumber",
          "type": "string",
          "description": "Numéro de badge",
          "required": false,
          "mutability": "readWrite"
        }
      ]
    }
  ]
}
```

## 🔌 Intégration avec Microsoft Entra

### Configuration dans Azure AD

1. Dans Azure AD, allez dans **Applications d'entreprise**
2. Créez ou sélectionnez votre application
3. Allez dans **Provisionnement**
4. Configurez l'URL du tenant : `https://votre-serveur/scim`
5. Configurez l'authentification (Bearer Token recommandé)
6. Testez la connexion
7. Mappez les attributs selon vos besoins
8. Activez le provisionnement

### Attributs supportés par défaut

**Utilisateurs :**
- userName (requis, unique)
- name (givenName, familyName)
- displayName
- emails
- phoneNumbers
- addresses
- active
- title
- userType
- externalId
- Attributs enterprise (employeeNumber, department, costCenter, etc.)

**Groupes :**
- displayName (requis)
- members
- externalId

## 🔐 Sécurité

> ⚠️ **Important** : Cette implémentation est à des fins de démonstration. Pour la production :

1. Ajoutez une authentification (OAuth 2.0 Bearer Token)
2. Implémentez HTTPS obligatoire
3. Ajoutez la validation des entrées
4. Implémentez un stockage persistant (base de données)
5. Ajoutez des logs d'audit
6. Limitez le taux de requêtes (rate limiting)

### Exemple d'ajout d'authentification Bearer Token

```csharp
// Dans Program.cs
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // Configuration JWT
    });

// Après app.Build()
app.UseAuthentication();
app.UseAuthorization();
```

Puis ajoutez `[Authorize]` sur les controllers.

## 🧪 Tests

### Données de test

En mode développement, des données de test sont automatiquement créées :
- Un utilisateur : `test.user@example.com`
- Un groupe : `Test Group`

Désactivez cela en production en définissant `Scim:LoadTestData` à `false`.

### Tests avec curl

```bash
# Obtenir la config
curl https://localhost:7091/scim/ServiceProviderConfig

# Lister les utilisateurs
curl https://localhost:7091/scim/Users

# Créer un utilisateur
curl -X POST https://localhost:7091/scim/Users \
  -H "Content-Type: application/scim+json" \
  -d '{"schemas":["urn:ietf:params:scim:schemas:core:2.0:User"],"userName":"test@example.com","active":true}'
```

## 📦 Implémentation alternative du repository

Pour utiliser une base de données, créez une nouvelle implémentation de `IScimRepository` :

```csharp
public class SqlScimRepository : IScimRepository
{
    private readonly ApplicationDbContext _context;
    
    public SqlScimRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Implémentez les méthodes...
}
```

Puis changez l'enregistrement dans `Program.cs` :

```csharp
builder.Services.AddScoped<IScimRepository, SqlScimRepository>();
```

## 📚 Ressources

- [Spécification SCIM 2.0](https://datatracker.ietf.org/doc/html/rfc7643)
- [Documentation Microsoft Entra SCIM](https://learn.microsoft.com/en-us/azure/active-directory/app-provisioning/use-scim-to-provision-users-and-groups)
- [Tutoriel de provisionnement Azure AD](https://learn.microsoft.com/en-us/azure/active-directory/app-provisioning/use-scim-to-build-users-and-groups-endpoints)

## 🤝 Contribution

N'hésitez pas à étendre cette implémentation selon vos besoins :
- Ajoutez de nouveaux attributs customs
- Implémentez une authentification robuste
- Ajoutez un stockage persistant
- Améliorez le filtrage SCIM
- Ajoutez des tests unitaires

## 📄 Licence

MIT

