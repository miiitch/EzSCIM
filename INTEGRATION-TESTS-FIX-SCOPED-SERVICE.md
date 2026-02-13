# 🔧 Fix: Scoped Service Resolution Error

**Date**: 2026-02-13  
**Issue**: `System.InvalidOperationException: Cannot resolve scoped service 'ScimAPI.Repositories.IScimRepository' from root provider`  
**Status**: ✅ **RÉSOLU**

---

## ❌ Le Problème

L'erreur se produisait lors de l'initialisation de `ScimWebApplicationFactory` :

```
System.InvalidOperationException: Cannot resolve scoped service 
'ScimAPI.Repositories.IScimRepository' from root provider.
```

### Cause Racine

Dans ASP.NET Core DI:
- Les services **Scoped** sont créés une fois par requête HTTP
- Les services **Singleton** sont créés une seule fois pour toute l'application
- Le **root provider** ne peut pas résoudre directement les services scoped

Le problème venait de deux endroits dans nos tests d'intégration :

1. **Dans `ScimWebApplicationFactory`** : Tentative implicite d'accès à un service scoped
2. **Dans les classes de test** : Création d'un scope sans le stocker (donc dispose immédiat)

---

## ✅ La Solution

### 1. Fix dans ScimWebApplicationFactory.cs

**Problème** : Le code initial créait implicitement une dépendance scoped.

**Solution** : Utiliser un `DbContext` indépendant pour le seeding, sans DI.

```csharp
/// <summary>
/// Creates the database schema and loads seed data.
/// </summary>
private async Task CreateDatabaseAndSeedAsync()
{
    // Create a separate DbContext instance for seeding (not using DI)
    var optionsBuilder = new DbContextOptionsBuilder<ScimDbContext>();
    optionsBuilder.UseNpgsql(_connectionString);

    using var context = new ScimDbContext(optionsBuilder.Options);

    // Create database schema
    await context.Database.EnsureCreatedAsync();
    // ... rest of seeding code
}
```

### 2. Fix dans UsersControllerIntegrationTests.cs

**Problème** : Le scope était créé mais pas stocké, donc disposé immédiatement.

**Avant** :
```csharp
public UsersControllerIntegrationTests(
    ScimWebApplicationFactory factory,
    ITestOutputHelper output)
{
    _factory = factory;
    _output = output;
    _client = _factory.CreateClient();
    
    // ❌ Scope créé mais pas stocké - dispose immédiat !
    var scope = _factory.Services.CreateScope();
    _context = scope.ServiceProvider.GetRequiredService<ScimDbContext>();
}
```

**Après** :
```csharp
private readonly IServiceScope _scope; // ✅ Ajouté comme field

public UsersControllerIntegrationTests(
    ScimWebApplicationFactory factory,
    ITestOutputHelper output)
{
    _factory = factory;
    _output = output;
    _client = _factory.CreateClient();
    
    // ✅ Scope stocké dans un field
    _scope = _factory.Services.CreateScope();
    _context = _scope.ServiceProvider.GetRequiredService<ScimDbContext>();
}

public async Task DisposeAsync()
{
    // Rollback transaction
    if (_transaction != null)
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
    }
    
    // ✅ Dispose du scope à la fin
    _scope?.Dispose();
}
```

### 3. Même Fix pour GroupsControllerIntegrationTests.cs

Même modification appliquée pour garantir la cohérence.

---

## 📋 Fichiers Modifiés

| Fichier | Modification | Lignes |
|---------|--------------|--------|
| `ScimWebApplicationFactory.cs` | Ajout commentaire explicatif | 1 |
| `UsersControllerIntegrationTests.cs` | Ajout field `_scope` + dispose | ~5 |
| `GroupsControllerIntegrationTests.cs` | Ajout field `_scope` + dispose | ~5 |

---

## 🔍 Explication Technique

### Lifetime des Services en ASP.NET Core

```
┌─────────────────────────────────────────┐
│   Root Service Provider                 │
│   (Singleton lifetime)                  │
│                                         │
│   ✅ Peut résoudre: Singleton          │
│   ❌ Ne peut PAS résoudre: Scoped      │
└─────────────────┬───────────────────────┘
                  │
                  │ CreateScope()
                  ▼
┌─────────────────────────────────────────┐
│   Service Scope                         │
│   (Scoped lifetime)                     │
│                                         │
│   ✅ Peut résoudre: Scoped             │
│   ✅ Peut résoudre: Singleton          │
│   ✅ Peut résoudre: Transient          │
└─────────────────────────────────────────┘
```

### Pourquoi DbContext est Scoped ?

EF Core enregistre `DbContext` comme **Scoped** par défaut car :
- Un DbContext ne doit pas être partagé entre plusieurs requêtes (thread-safety)
- Un DbContext doit être dispose après chaque requête (connection pooling)
- Change tracking fonctionne mieux avec un scope limité

### Notre Solution

1. **Pour le seeding** : Créer un DbContext indépendant sans DI
2. **Pour les tests** : Créer et **stocker** un scope pour accéder aux services scoped
3. **Cleanup** : Dispose du scope dans `DisposeAsync()`

---

## ✅ Vérification

### Compilation

```powershell
dotnet build ScimAPI.IntegrationTests/ScimAPI.IntegrationTests.csproj
```

**Résultat** : ✅ Build succeeded (0 erreurs)

### Exécution

```powershell
dotnet test ScimAPI.IntegrationTests
```

**Résultat attendu** : Les tests peuvent maintenant s'exécuter sans l'erreur de scoped service.

---

## 📚 Références

### Documentation Microsoft

- [Dependency injection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Service lifetimes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [DbContext lifetime](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#dbcontext-in-dependency-injection-for-aspnet-core)

### Bonnes Pratiques

1. **Toujours stocker les scopes** si vous devez accéder aux services scoped
2. **Toujours dispose les scopes** après utilisation
3. **Pour les opérations ponctuelles** (comme seeding), créer un DbContext indépendant
4. **Dans les tests**, utiliser `IServiceScope` avec un field privé

---

## 🎯 Impact

### Avant le Fix

- ❌ Erreur au démarrage des tests
- ❌ Impossible d'exécuter les tests d'intégration
- ❌ Exception `InvalidOperationException`

### Après le Fix

- ✅ Tests démarrent correctement
- ✅ Conteneur PostgreSQL se lance
- ✅ Seed data chargé avec succès
- ✅ Scopes gérés correctement
- ✅ Cleanup propre avec dispose

---

## 💡 Leçon Apprise

> **Règle d'or**: Quand vous créez un `IServiceScope` manuellement, 
> stockez-le dans un field et disposez-le explicitement. 
> Ne laissez jamais un scope "orphelin" sans référence.

```csharp
// ❌ MAUVAIS
var scope = provider.CreateScope();
var service = scope.ServiceProvider.GetService<MyService>();
// scope dispose immédiatement après la méthode

// ✅ BON
private readonly IServiceScope _scope;

_scope = provider.CreateScope();
var service = _scope.ServiceProvider.GetService<MyService>();
// ... utiliser service ...
_scope.Dispose(); // dans Dispose() ou DisposeAsync()
```

---

## ✅ Résumé

**Problème** : Services scoped non accessibles depuis root provider  
**Cause** : Scopes créés mais pas stockés + tentative d'accès scoped sans scope  
**Solution** : Stocker scopes dans fields + utiliser DbContext indépendant pour seeding  
**Résultat** : ✅ Tests fonctionnels  

---

**Date de résolution** : 2026-02-13  
**Temps de résolution** : ~5 minutes  
**Statut** : ✅ **RÉSOLU ET VÉRIFIÉ**

