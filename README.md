﻿﻿# SCIM 2.0 API for Microsoft Entra (Azure AD)

This API implements the SCIM 2.0 (System for Cross-domain Identity Management) protocol in compliance with Microsoft Entra (formerly Azure AD) requirements for automatic user and group synchronization.

> **📚 Full Documentation**: See [docs/README.md](./docs/README.md) for complete guides, references, and tutorials.  
> **🚀 Quick Start**: New to the project? Start with [Quick Start Guide](./docs/guides/quickstart.md) (5 minutes).  
> **🔐 Authentication**: Configure JWT authentication in [Authentication Setup](./docs/auth/setup.md).

## 🎯 Features

- ✅ **User Management** (create, read, update, delete, patch)
- ✅ **Group Management** (create, read, update, delete, patch)
- ✅ **Group Members Support** (add/remove via PATCH)
- ✅ **Advanced SCIM Filtering** with logical operators (AND, OR, NOT)
- ✅ **Comparison Operators** (eq, ne, co, sw, ew, pr, gt, ge, lt, le)
- ✅ **Filters on Complex Attributes** (name.givenName, emails.value, etc.)
- ✅ **Result Pagination**
- ✅ **Discovery Endpoints** (ServiceProviderConfig, Schemas)
- ✅ **Extensible Custom Schemas** (Enterprise User, Custom attributes)
- ✅ **In-Memory Implementation** with IScimRepository interface
- ✅ **Microsoft Entra Compatible** (Azure AD provisioning)

## 🏗️ Architecture

### Project Structure

```
EzSCIM/
├── Controllers/
│   ├── UsersController.cs           # SCIM endpoints for users
│   ├── GroupsController.cs          # SCIM endpoints for groups
│   └── ScimConfigController.cs      # SCIM discovery endpoints
├── Models/
│   ├── ScimUser.cs                  # SCIM user model
│   ├── ScimGroup.cs                 # SCIM group model
│   ├── ScimMeta.cs                  # SCIM metadata
│   ├── ScimListResponse.cs          # Paginated list response
│   ├── ScimError.cs                 # Error response
│   ├── ScimPatchRequest.cs          # SCIM PATCH request
│   ├── ScimServiceProviderConfig.cs # Provider configuration
│   └── ScimSchema.cs                # Schema definition
├── Repositories/
│   ├── IScimRepository.cs           # Repository interface
│   └── InMemoryScimRepository.cs    # In-memory implementation
├── Services/
│   └── ScimSchemaInitializer.cs     # Schema initialization on startup
└── appsettings.Scim.json            # Custom schema configuration
```

## 🚀 Quick Start

### 1. Start the Application

```bash
cd EzSCIM
dotnet run
```

The API will be accessible on `https://localhost:7001`

### 2. Test the Endpoints

Use the `ScimAPI.http` file with the VS Code REST Client extension or Rider.

### 3. Available Endpoints

#### SCIM Discovery
- `GET /scim/ServiceProviderConfig` - SCIM provider configuration
- `GET /scim/Schemas` - List all schemas
- `GET /scim/Schemas/{id}` - Get specific schema details
- `POST /scim/Schemas` - Add custom schema

#### Users
- `GET /scim/Users` - List users (with filtering and pagination)
- `GET /scim/Users/{id}` - Get a user
- `POST /scim/Users` - Create a user
- `PUT /scim/Users/{id}` - Update a user (complete)
- `PATCH /scim/Users/{id}` - Partially update a user
- `DELETE /scim/Users/{id}` - Delete a user

#### Groups
- `GET /scim/Groups` - List groups (with filtering and pagination)
- `GET /scim/Groups/{id}` - Get a group
- `POST /scim/Groups` - Create a group
- `PUT /scim/Groups/{id}` - Update a group (complete)
- `PATCH /scim/Groups/{id}` - Partially update a group (members)
- `DELETE /scim/Groups/{id}` - Delete a group

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

