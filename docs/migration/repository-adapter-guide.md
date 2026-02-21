# Guide d'intégration SCIM avec repository personnalisé

## Vue d'ensemble

Ce guide explique comment intégrer votre système de gestion d'utilisateurs existant avec SCIM en utilisant les composants fournis :

1. **IUserDataRepository<TUser>** - Interface pour votre source de données
2. **IScimFilterTranslator<TUser>** - Traduction AST → IQueryable
3. **ScimUserRepositoryAdapter<TUser>** - Adaptateur SCIM

## Architecture

```
┌─────────────────────┐
│  SCIM Controller    │
└──────────┬──────────┘
           │
           v
┌─────────────────────────────────┐
│ IScimUserRepository<ScimUser>   │ (interface SCIM)
└──────────┬──────────────────────┘
           │
           v
┌─────────────────────────────────┐
│ ScimUserRepositoryAdapter<TUser>│ (adaptateur)
└──────────┬──────────────────────┘
           │
           ├─────────────────────────────┐
           │                             │
           v                             v
┌──────────────────────┐    ┌──────────────────────────┐
│IUserDataRepository   │    │IScimFilterTranslator     │
│    <TUser>           │    │    <TUser>               │
└──────────┬───────────┘    └──────────┬───────────────┘
           │                           │
           v                           v
┌──────────────────────┐    ┌──────────────────────────┐
│  Votre base de       │    │  AST → Expression<Func>  │
│  données (EF, SQL)   │    │  (filtrage server-side)  │
└──────────────────────┘    └──────────────────────────┘
```

## Étape 1 : Annoter votre modèle utilisateur

Ajoutez les attributs `[ScimProperty]` sur les propriétés de votre classe utilisateur :

```csharp
using ScimAPI.Attributes;

public class MyUser
{
    public string Id { get; set; } = string.Empty;

    [ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
    public string Username { get; set; } = string.Empty;

    [ScimProperty("email", "string")]
    public string EmailAddress { get; set; } = string.Empty;

    [ScimProperty("givenName", "string")]
    public string FirstName { get; set; } = string.Empty;

    [ScimProperty("familyName", "string")]
    public string LastName { get; set; } = string.Empty;

    [ScimProperty("displayName", "string")]
    public string DisplayName { get; set; } = string.Empty;

    [ScimProperty("active", "boolean")]
    public bool IsActive { get; set; } = true;

    [ScimProperty("title", "string")]
    public string JobTitle { get; set; } = string.Empty;

    // Propriétés non-SCIM (pas d'attribut)
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Attributs supportés

| Attribut SCIM | Type | Description |
|---------------|------|-------------|
| userName | string | Identifiant unique (REQUIS) |
| email | string | Adresse email |
| givenName | string | Prénom |
| familyName | string | Nom de famille |
| displayName | string | Nom d'affichage |
| active | boolean | Statut actif/inactif |
| title | string | Titre/poste |
| externalId | string | ID système externe |
| name.givenName | string | Prénom (attribut imbriqué) |
| name.familyName | string | Nom (attribut imbriqué) |

## Étape 2 : Implémenter IUserDataRepository

Créez un repository qui expose vos données sous forme d'IQueryable :

```csharp
using ScimAPI.Repositories;

public class MyUserRepository : IUserDataRepository<MyUser>
{
    private readonly MyDbContext _context;

    public MyUserRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<MyUser?> GetAsync(string id)
    {
        return await _context.Users.FindAsync(id);
    }

    // IMPORTANT : Retourner IQueryable pour permettre le filtrage server-side
    public IQueryable<MyUser> Query()
    {
        return _context.Users.AsQueryable();
    }

    public async Task<MyUser> CreateAsync(MyUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            user.Id = Guid.NewGuid().ToString();
        }
        
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<MyUser?> UpdateAsync(string id, MyUser user)
    {
        var existing = await _context.Users.FindAsync(id);
        if (existing == null)
            return null;

        user.Id = id;
        user.UpdatedAt = DateTime.UtcNow;
        
        _context.Entry(existing).CurrentValues.SetValues(user);
        await _context.SaveChangesAsync();
        
        return user;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
```

## Étape 3 : Configurer les services (Program.cs / Startup.cs)

Enregistrez les services dans le conteneur DI :

```csharp
using ScimAPI.Repositories;
using ScimAPI.Filtering;

// Votre repository
builder.Services.AddScoped<IUserDataRepository<MyUser>, MyUserRepository>();

// Traducteur de filtres (AST → IQueryable)
builder.Services.AddScoped<IScimFilterTranslator<MyUser>, GenericScimFilterTranslator<MyUser>>();

// Adaptateur SCIM
builder.Services.AddScoped<IScimUserRepository<ScimUser>>(sp =>
{
    var dataRepo = sp.GetRequiredService<IUserDataRepository<MyUser>>();
    var translator = sp.GetRequiredService<IScimFilterTranslator<MyUser>>();
    return new ScimUserRepositoryAdapter<MyUser>(dataRepo, translator);
});
```

## Étape 4 : Utilisation

Le contrôleur SCIM existant utilisera automatiquement votre adaptateur :

```csharp
// GET /scim/Users?filter=userName eq "john.doe@example.com"
// Le filtre SCIM sera converti en :
// context.Users.Where(u => u.Username.Equals("john.doe@example.com", StringComparison.OrdinalIgnoreCase))

// GET /scim/Users?filter=active eq true and givenName sw "John"
// Sera converti en :
// context.Users.Where(u => u.IsActive == true && u.FirstName.StartsWith("John", StringComparison.OrdinalIgnoreCase))
```

## Fonctionnement du filtrage

### Traduction AST → LINQ

Le `GenericScimFilterTranslator` convertit l'AST de filtre SCIM en arbre d'expressions LINQ :

```
Filtre SCIM : userName eq "john" and active eq true

AST :
AndFilter
├── ComparisonFilter(userName, eq, "john")
└── ComparisonFilter(active, eq, true)

Expression LINQ :
user => user.Username.Equals("john", StringComparison.OrdinalIgnoreCase) 
        && user.IsActive == true
```

### Opérateurs supportés

| SCIM | Description | Exemple |
|------|-------------|---------|
| eq | Égal | `userName eq "john"` |
| ne | Différent | `active ne false` |
| co | Contient | `email co "@example.com"` |
| sw | Commence par | `userName sw "john"` |
| ew | Termine par | `email ew ".com"` |
| pr | Présent (non null/vide) | `email pr` |
| gt | Supérieur | `age gt 18` |
| ge | Supérieur ou égal | `age ge 18` |
| lt | Inférieur | `age lt 65` |
| le | Inférieur ou égal | `age le 65` |
| and | ET logique | `active eq true and email pr` |
| or | OU logique | `userName sw "john" or userName sw "jane"` |
| not | NON logique | `not(active eq false)` |

### Exemples de filtres

```csharp
// Utilisateurs actifs
GET /scim/Users?filter=active eq true

// Utilisateurs avec email @example.com
GET /scim/Users?filter=email co "@example.com"

// Utilisateurs dont le nom commence par "John"
GET /scim/Users?filter=givenName sw "John"

// Filtres complexes
GET /scim/Users?filter=(userName sw "john" or userName sw "jane") and active eq true

// Présence d'attribut
GET /scim/Users?filter=email pr and title pr
```

## Avantages

✅ **Filtrage server-side** : Les filtres SCIM sont traduits en SQL (via EF Core)  
✅ **Performance** : Pas de chargement en mémoire, tout s'exécute côté base  
✅ **Type-safe** : Mapping via attributs, détection d'erreurs à la compilation  
✅ **Flexible** : Fonctionne avec n'importe quelle classe TUser annotée  
✅ **Maintenable** : Séparation claire entre modèle métier et SCIM  

## Personnalisation avancée

### Créer un traducteur personnalisé

Si vous avez des besoins spécifiques (propriétés complexes, jointures, etc.) :

```csharp
public class MyCustomTranslator : IScimFilterTranslator<MyUser>
{
    public Expression<Func<MyUser, bool>>? BuildPredicate(FilterExpression? filter)
    {
        // Logique personnalisée
    }

    public IQueryable<MyUser> Apply(IQueryable<MyUser> source, FilterExpression? filter)
    {
        var predicate = BuildPredicate(filter);
        if (predicate == null)
            return source;

        // Ajouter des jointures, includes, etc.
        return source
            .Include(u => u.Department)
            .Where(predicate);
    }
}
```

### Mapper des attributs complexes

```csharp
public class MyUser
{
    [ScimProperty("name", "complex")]
    public UserName Name { get; set; } = new();
}

public class UserName
{
    [ScimProperty("givenName", "string")]
    public string First { get; set; } = string.Empty;

    [ScimProperty("familyName", "string")]
    public string Last { get; set; } = string.Empty;
}

// Filtre SCIM : name.givenName eq "John"
// Sera mappé automatiquement sur : user.Name.First
```

## Tests

Des tests unitaires sont fournis pour valider le traducteur :

```bash
# Tester le traducteur générique
dotnet test --filter GenericScimFilterTranslatorTests

# Tester le traducteur ScimUser
dotnet test --filter ScimUserFilterTranslatorTests
```

## Résumé

1. **Annoter** votre modèle avec `[ScimProperty]`
2. **Implémenter** `IUserDataRepository<TUser>` (retourner `IQueryable`)
3. **Enregistrer** les services dans DI
4. **Utiliser** - les filtres SCIM sont automatiquement traduits en requêtes SQL

Le système traduit les filtres SCIM en expressions LINQ qui s'exécutent directement sur votre base de données, garantissant performance et scalabilité.

