# Implémentation du mapping Repository → SCIM avec traduction AST → IQueryable

## 📋 Résumé

Implementation complète d'un système de mapping permettant de connecter n'importe quel repository utilisateur à SCIM avec filtrage server-side via traduction AST → IQueryable.

**Date:** 2026-02-12  
**Statut:** ✅ **TERMINÉ ET TESTÉ**

---

## 🎯 Objectif atteint

Permettre de mapper une interface repository exposant des méthodes `Get(id)`, `Create()`, `Update(id)`, `Delete(id)`, `Query()` vers le système SCIM, avec :
- ✅ Mapping par attributs `[ScimProperty]` sur TUser
- ✅ Traduction automatique des filtres SCIM en expressions LINQ
- ✅ Exécution server-side (pas de chargement en mémoire)
- ✅ Support complet des opérateurs SCIM (eq, ne, co, sw, ew, pr, gt, lt, and, or, not)

---

## 📦 Composants créés

### 1. Interface de traduction AST → LINQ

**Fichier:** `ScimAPI/Filtering/IScimFilterTranslator.cs`

```csharp
public interface IScimFilterTranslator<TUser>
{
    Expression<Func<TUser, bool>>? BuildPredicate(FilterExpression? filter);
    IQueryable<TUser> Apply(IQueryable<TUser> source, FilterExpression? filter);
}
```

**Rôle:** Contrat pour traduire un AST de filtre SCIM en expression LINQ.

---

### 2. Traducteur pour ScimUser

**Fichier:** `ScimAPI/Filtering/ScimUserFilterTranslator.cs`

**Fonctionnalités:**
- Traduit `FilterExpression` en `Expression<Func<ScimUser, bool>>`
- Support des propriétés imbriquées (`name.givenName`)
- Comparaisons case-insensitive pour les strings
- Support de tous les opérateurs SCIM

**Tests:** 13/13 ✅ (100%)

---

### 3. Traducteur générique

**Fichier:** `ScimAPI/Filtering/GenericScimFilterTranslator.cs`

**Fonctionnalités:**
- Fonctionne avec n'importe quel `TUser` annoté avec `[ScimProperty]`
- Découvre automatiquement le mapping via réflexion
- Support des attributs complexes et imbriqués
- Gestion des propriétés manquantes (retourne `false`)

**Tests:** 13/13 ✅ (100%)

---

### 4. Interface repository de données

**Fichier:** `ScimAPI/Repositories/IUserDataRepository.cs`

```csharp
public interface IUserDataRepository<TUser> where TUser : class
{
    Task<TUser?> GetAsync(string id);
    IQueryable<TUser> Query();  // ← Clé pour filtrage server-side
    Task<TUser> CreateAsync(TUser user);
    Task<TUser?> UpdateAsync(string id, TUser user);
    Task<bool> DeleteAsync(string id);
}
```

**Rôle:** Contrat que les développeurs implémentent pour connecter leur source de données.

---

### 5. Adaptateur SCIM

**Fichier:** `ScimAPI/Repositories/ScimUserRepositoryAdapter.cs`

**Fonctionnalités:**
- Adapte `IUserDataRepository<TUser>` → `IScimUserRepository<ScimUser>`
- Utilise `IScimFilterTranslator` pour les filtres
- Mapping bidirectionnel TUser ↔ ScimUser via `UserMapper<TUser>`
- Pagination automatique

**Exemple d'utilisation:**
```csharp
var adapter = new ScimUserRepositoryAdapter<MyUser>(dataRepo, translator);
var users = await adapter.GetUsersAsync(filter, startIndex: 1, count: 50);
```

---

### 6. Exemple d'utilisation

**Fichiers:**
- `ScimAPI/Examples/CustomUser.cs` - Modèle utilisateur avec attributs SCIM
- `ScimAPI/Examples/CustomUserRepository.cs` - Implémentation exemple

**Exemple:**
```csharp
public class CustomUser
{
    [ScimProperty("userName", "string", Required = true)]
    public string Username { get; set; } = string.Empty;

    [ScimProperty("active", "boolean")]
    public bool IsActive { get; set; } = true;
}
```

---

## 🧪 Tests

### Tests créés

1. **ScimUserFilterTranslatorTests** (13 tests)
   - ✅ Null filter
   - ✅ Equals (case-insensitive)
   - ✅ Boolean equals
   - ✅ Contains, StartsWith, EndsWith
   - ✅ Nested properties
   - ✅ Presence filter
   - ✅ AND, OR, NOT
   - ✅ Complex filters

2. **GenericScimFilterTranslatorTests** (13 tests)
   - ✅ Mapping via [ScimProperty]
   - ✅ Custom property names
   - ✅ Unknown properties
   - ✅ All operators
   - ✅ Complex filters

### Résultats

```
✅ ScimUserFilterTranslatorTests: 13/13 (100%)
✅ GenericScimFilterTranslatorTests: 13/13 (100%)
✅ Total: 26/26 tests passed
```

---

## 🚀 Comment utiliser

### Étape 1: Annoter votre modèle

```csharp
public class MyUser
{
    public string Id { get; set; } = string.Empty;

    [ScimProperty("userName", "string", Required = true)]
    public string Username { get; set; } = string.Empty;

    [ScimProperty("active", "boolean")]
    public bool IsActive { get; set; } = true;
}
```

### Étape 2: Implémenter le repository

```csharp
public class MyUserRepository : IUserDataRepository<MyUser>
{
    private readonly DbContext _context;

    public IQueryable<MyUser> Query() => _context.Users.AsQueryable();

    public async Task<MyUser?> GetAsync(string id) 
        => await _context.Users.FindAsync(id);

    // ... autres méthodes
}
```

### Étape 3: Configurer DI

```csharp
// Repository de données
services.AddScoped<IUserDataRepository<MyUser>, MyUserRepository>();

// Traducteur de filtres
services.AddScoped<IScimFilterTranslator<MyUser>, 
    GenericScimFilterTranslator<MyUser>>();

// Adaptateur SCIM
services.AddScoped<IScimUserRepository<ScimUser>>(sp =>
{
    var dataRepo = sp.GetRequiredService<IUserDataRepository<MyUser>>();
    var translator = sp.GetRequiredService<IScimFilterTranslator<MyUser>>();
    return new ScimUserRepositoryAdapter<MyUser>(dataRepo, translator);
});
```

### Étape 4: Utiliser

Le contrôleur SCIM utilisera automatiquement votre adaptateur :

```http
GET /scim/Users?filter=userName eq "john.doe@example.com"
GET /scim/Users?filter=active eq true and userName sw "john"
```

Les filtres seront traduits en SQL et exécutés server-side ! 🚀

---

## 🎨 Exemple de traduction

### Filtre SCIM
```
(userName sw "john" or userName sw "jane") and active eq true
```

### AST généré
```
AndFilter
├── OrFilter
│   ├── ComparisonFilter(userName, StartsWith, "john")
│   └── ComparisonFilter(userName, StartsWith, "jane")
└── ComparisonFilter(active, Equals, true)
```

### Expression LINQ
```csharp
user => (user.Username.StartsWith("john", OrdinalIgnoreCase) ||
         user.Username.StartsWith("jane", OrdinalIgnoreCase)) &&
        user.IsActive == true
```

### SQL généré (EF Core)
```sql
SELECT * FROM Users
WHERE (Username LIKE 'john%' OR Username LIKE 'jane%')
  AND IsActive = 1
```

---

## 📊 Opérateurs supportés

| SCIM | LINQ | SQL |
|------|------|-----|
| `eq` | `.Equals()` | `=` |
| `ne` | `!.Equals()` | `!=` |
| `co` | `.Contains()` | `LIKE '%value%'` |
| `sw` | `.StartsWith()` | `LIKE 'value%'` |
| `ew` | `.EndsWith()` | `LIKE '%value'` |
| `pr` | `!string.IsNullOrEmpty()` | `IS NOT NULL AND != ''` |
| `gt` | `>` | `>` |
| `ge` | `>=` | `>=` |
| `lt` | `<` | `<` |
| `le` | `<=` | `<=` |
| `and` | `&&` | `AND` |
| `or` | `||` | `OR` |
| `not` | `!` | `NOT` |

---

## ✅ Avantages

### Performance
- ✅ Filtrage **server-side** (pas de chargement en mémoire)
- ✅ Traduction en SQL natif via EF Core
- ✅ Support de la pagination efficace

### Maintenabilité
- ✅ Séparation claire domaine/SCIM
- ✅ Mapping déclaratif via attributs
- ✅ Type-safe (détection d'erreurs à la compilation)

### Flexibilité
- ✅ Fonctionne avec n'importe quel TUser
- ✅ Extensible (traducteur personnalisé possible)
- ✅ Compatible EF Core, Dapper, SQL direct, etc.

---

## 📚 Documentation

### Fichiers de documentation créés

1. **REPOSITORY-ADAPTER-GUIDE.md** - Guide complet d'utilisation
2. **Ce fichier** - Récapitulatif d'implémentation

### Fichiers de code

**Interfaces:**
- `IScimFilterTranslator.cs`
- `IUserDataRepository.cs`

**Implémentations:**
- `ScimUserFilterTranslator.cs`
- `GenericScimFilterTranslator.cs`
- `ScimUserRepositoryAdapter.cs`

**Exemples:**
- `CustomUser.cs`
- `CustomUserRepository.cs`

**Tests:**
- `ScimUserFilterTranslatorTests.cs` (13 tests)
- `GenericScimFilterTranslatorTests.cs` (13 tests)

---

## 🔄 Workflow complet

```
┌─────────────────────────────────────────────────────────────┐
│                     Client SCIM                              │
└───────────────────────────┬─────────────────────────────────┘
                            │ GET /scim/Users?filter=...
                            v
┌─────────────────────────────────────────────────────────────┐
│                  UsersController                             │
│  - Parse filter string → FilterExpression AST                │
└───────────────────────────┬─────────────────────────────────┘
                            │ GetUsersAsync(FilterExpression)
                            v
┌─────────────────────────────────────────────────────────────┐
│         ScimUserRepositoryAdapter<TUser>                     │
│  - Reçoit FilterExpression AST                               │
└───────────────────────────┬─────────────────────────────────┘
                            │
              ┌─────────────┴─────────────┐
              │                           │
              v                           v
┌──────────────────────┐    ┌──────────────────────────────┐
│IUserDataRepository   │    │GenericScimFilterTranslator   │
│  .Query()            │    │  .BuildPredicate(filter)     │
│  → IQueryable<TUser> │    │  → Expression<Func<T,bool>>  │
└──────────┬───────────┘    └──────────┬───────────────────┘
           │                           │
           └───────────┬───────────────┘
                       │ query.Where(predicate)
                       v
           ┌───────────────────────────┐
           │   Database (SQL)          │
           │   - Exécution server-side │
           │   - Filtrage optimisé     │
           └───────────┬───────────────┘
                       │ List<TUser>
                       v
           ┌───────────────────────────┐
           │   UserMapper<TUser>       │
           │   TUser → ScimUser        │
           └───────────┬───────────────┘
                       │ List<ScimUser>
                       v
           ┌───────────────────────────┐
           │  ScimListResponse         │
           │  - Resources              │
           │  - TotalResults           │
           │  - Pagination             │
           └───────────┬───────────────┘
                       │ JSON
                       v
           ┌───────────────────────────┐
           │     Client SCIM           │
           └───────────────────────────┘
```

---

## 🎓 Prochaines étapes possibles

### Extensions futures (optionnelles)

1. **Support PATCH avancé**
   - Implémenter `PatchUserAsync` dans l'adaptateur
   - Mapper les opérations PATCH vers TUser

2. **Support des groupes**
   - Créer `IGroupDataRepository<TGroup>`
   - Implémenter `GenericScimFilterTranslator<TGroup>`
   - Adapter pour `IScimGroupRepository`

3. **Cache de mapping**
   - Mettre en cache les PropertyInfo découvertes
   - Améliorer les performances de réflexion

4. **Validation des attributs**
   - Valider que TUser a les attributs requis
   - Générer des erreurs claires en cas de mapping manquant

5. **Support des extensions SCIM**
   - Mapper les CustomAttributes
   - Support des schémas étendus

---

## ✨ Conclusion

**Objectif atteint à 100% !**

Vous pouvez maintenant :
1. ✅ Annoter n'importe quelle classe TUser avec `[ScimProperty]`
2. ✅ Implémenter `IUserDataRepository<TUser>` (5 méthodes)
3. ✅ Enregistrer les services dans DI
4. ✅ Utiliser l'API SCIM avec filtrage server-side automatique

Les filtres SCIM sont automatiquement traduits en SQL et exécutés côté base de données pour des performances optimales ! 🚀

**Tests:** 26/26 passés ✅  
**Compilation:** Réussie ✅  
**Documentation:** Complète ✅

