# EzSCIM Controllers

## Vue d'ensemble

Les contrôleurs SCIM ont été déplacés dans la bibliothèque EzSCIM pour permettre leur réutilisation dans n'importe quelle application ASP.NET Core.

## Contrôleurs disponibles

### ScimUsersController
- **Route**: `scim/Users`
- **Fonctionnalités**:
  - `GET /scim/Users` - Liste des utilisateurs avec filtrage, pagination
  - `GET /scim/Users/{id}` - Récupère un utilisateur par ID
  - `POST /scim/Users` - Crée un nouvel utilisateur
  - `PUT /scim/Users/{id}` - Met à jour un utilisateur existant
  - `PATCH /scim/Users/{id}` - Applique des modifications partielles
  - `DELETE /scim/Users/{id}` - Supprime un utilisateur

### ScimGroupsController
- **Route**: `scim/Groups`
- **Fonctionnalités**:
  - `GET /scim/Groups` - Liste des groupes avec filtrage, pagination
  - `GET /scim/Groups/{id}` - Récupère un groupe par ID
  - `POST /scim/Groups` - Crée un nouveau groupe
  - `PUT /scim/Groups/{id}` - Met à jour un groupe existant
  - `PATCH /scim/Groups/{id}` - Applique des modifications partielles
  - `DELETE /scim/Groups/{id}` - Supprime un groupe

## Utilisation

### 1. Configuration dans Program.cs

```csharp
using EzSCIM.Controllers;
using EzSCIM.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Enregistrer le repository SCIM
builder.Services.AddSingleton<IScimRepository, YourScimRepositoryImplementation>();

// Ajouter les contrôleurs SCIM
builder.Services.AddScimControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Configurer l'authentification
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // Votre configuration JWT
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 2. Méthode d'extension AddScimControllers()

La méthode `AddScimControllers()` :
- Enregistre automatiquement les contrôleurs SCIM depuis la bibliothèque EzSCIM
- Retourne un `IMvcBuilder` pour permettre le chaînage avec d'autres configurations (comme `AddJsonOptions`)
- Charge les contrôleurs via `AddApplicationPart`

### 3. Exemple de requêtes

```bash
# Lister les utilisateurs
GET /scim/Users

# Filtrer les utilisateurs
GET /scim/Users?filter=userName eq "john.doe@example.com"&startIndex=1&count=10

# Créer un utilisateur
POST /scim/Users
{
  "userName": "john.doe@example.com",
  "name": {
    "givenName": "John",
    "familyName": "Doe"
  },
  "emails": [{
    "value": "john.doe@example.com",
    "type": "work",
    "primary": true
  }]
}

# Mettre à jour un utilisateur
PUT /scim/Users/{id}
{
  "userName": "john.doe@example.com",
  "active": true
}

# Patch un utilisateur
PATCH /scim/Users/{id}
{
  "Operations": [{
    "op": "replace",
    "path": "active",
    "value": false
  }]
}

# Supprimer un utilisateur
DELETE /scim/Users/{id}
```

## Authentification

Les contrôleurs sont décorés avec `[Authorize]`, donc :
- L'authentification doit être configurée dans votre application
- Toutes les requêtes doivent inclure un token d'authentification valide
- Le format attendu est : `Authorization: Bearer {token}`

## Format de réponse

Les contrôleurs retournent :
- **200 OK** - Opération réussie
- **201 Created** - Ressource créée (POST)
- **204 No Content** - Suppression réussie (DELETE)
- **400 Bad Request** - Filtre invalide ou données incorrectes
- **404 Not Found** - Ressource non trouvée
- **409 Conflict** - Ressource existe déjà
- **500 Internal Server Error** - Erreur interne

Toutes les réponses sont au format `application/scim+json`.

## Migration depuis les contrôleurs Demo

Si vous utilisiez les contrôleurs dans `EzSCIM.EntraID.Demo` :

1. Les contrôleurs Demo sont maintenant obsolètes et marqués avec `[Obsolete]`
2. Ils restent disponibles sur la route `scim/demo/Users` et `scim/demo/Groups` pour la compatibilité
3. Les nouveaux contrôleurs EzSCIM sont sur `scim/Users` et `scim/Groups`
4. Mettez à jour vos clients pour utiliser les nouvelles routes

## Personnalisation

Pour personnaliser les contrôleurs :

1. **Option 1** : Hériter des contrôleurs EzSCIM
```csharp
public class CustomUsersController : ScimUsersController
{
    public CustomUsersController(IScimRepository repository, ILogger<CustomUsersController> logger)
        : base(repository, logger)
    {
    }
    
    // Ajouter des méthodes personnalisées
}
```

2. **Option 2** : Créer vos propres contrôleurs et utiliser `IScimRepository`
```csharp
[ApiController]
[Route("custom/users")]
public class MyCustomController : ControllerBase
{
    private readonly IScimRepository _repository;
    
    public MyCustomController(IScimRepository repository)
    {
        _repository = repository;
    }
    
    // Vos méthodes personnalisées
}
```

## Dépendances requises

Les contrôleurs nécessitent :
- `IScimRepository` - Doit être enregistré dans le conteneur DI
- `ILogger<T>` - Fourni automatiquement par ASP.NET Core
- ASP.NET Core 10.0 ou supérieur

## Notes importantes

1. Les contrôleurs utilisent le filtrage SCIM via `FilterParser`
2. La pagination est gérée avec `startIndex` et `count`
3. Les erreurs de filtrage retournent un `ScimError` avec code 400
4. Tous les contrôleurs sont thread-safe et peuvent être utilisés dans des environnements multi-threads

## Voir aussi

- [SCIM Filter Documentation](../SCIM-FILTER-DOCUMENTATION.md)
- [Repository Guide](../REPOSITORY-ADAPTER-GUIDE.md)
- [Quick Start](../QUICKSTART.md)

