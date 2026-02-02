# Système de génération automatique des schémas SCIM

Ce projet implémente un système de génération automatique des schémas SCIM basé sur des attributs C#. Les schémas sont calculés à partir d'annotations sur les classes et propriétés, avec un cache thread-safe via constructeur statique.

## 🚀 Quick Start

### 1. Définir un modèle SCIM

```csharp
using ScimAPI.Attributes;

[ScimResource(
    "urn:ietf:params:scim:schemas:core:2.0:User",
    "User",
    "User Account")]
public class ScimUser
{
    // Propriété système (sans attribut = pas dans le schéma)
    public string Id { get; set; }
    
    // Attribut SCIM simple
    [ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
    public string UserName { get; set; }
    
    // Attribut complexe
    [ScimProperty("name", "complex")]
    public ScimName Name { get; set; }
    
    // Multi-valué
    [ScimProperty("emails", "complex", MultiValued = true)]
    public List<ScimEmail> Emails { get; set; }
}

// Classe complexe avec sous-attributs
public class ScimName
{
    [ScimProperty("givenName", "string")]
    public string? GivenName { get; set; }
    
    [ScimProperty("familyName", "string")]
    public string? FamilyName { get; set; }
}
```

### 2. Accéder aux schémas

```csharp
using ScimAPI.Helpers;

// Schémas pré-calculés (thread-safe, calculés une seule fois)
var userSchema = ScimSchemaGenerator.UserSchema;
var groupSchema = ScimSchemaGenerator.GroupSchema;

Console.WriteLine($"User schema: {userSchema.Attributes.Count} attributes");
```

### 3. Dans un contrôleur

```csharp
[HttpGet("Schemas")]
public IActionResult GetSchemas()
{
    return Ok(new[] 
    { 
        ScimSchemaGenerator.UserSchema,
        ScimSchemaGenerator.GroupSchema 
    });
}
```

## 📖 Concepts clés

### Approche opt-in

**Seules les propriétés avec `[ScimProperty]` sont incluses dans le schéma.**

```csharp
public class ScimUser
{
    // ❌ Pas dans le schéma (pas d'attribut)
    public string Id { get; set; }
    public ScimMeta Meta { get; set; }
    
    // ✅ Dans le schéma (attribut présent)
    [ScimProperty("userName", "string")]
    public string UserName { get; set; }
}
```

### Héritage automatique

Les propriétés annotées de la classe parent sont automatiquement incluses :

```csharp
public class ScimUser
{
    [ScimProperty("userName", "string")]
    public string UserName { get; set; }
}

// Hérite userName + ajoute employeeNumber
public class EnterpriseUser : ScimUser
{
    [ScimProperty("employeeNumber", "string")]
    public string EmployeeNumber { get; set; }
}

// Le schéma contiendra userName ET employeeNumber
var schema = ScimSchemaGenerator.GetSchema<EnterpriseUser>();
```

### Types complexes

Les sous-attributs sont découverts automatiquement :

```csharp
[ScimProperty("name", "complex")]
public ScimName Name { get; set; }

public class ScimName
{
    [ScimProperty("givenName", "string")]
    public string? GivenName { get; set; }
    
    [ScimProperty("familyName", "string")]
    public string? FamilyName { get; set; }
}

// Génère automatiquement name.givenName et name.familyName
```

### Thread-safety

Le constructeur statique garantit un calcul unique et thread-safe :

```csharp
public static class ScimSchemaGenerator
{
    public static ScimSchema UserSchema { get; }
    public static ScimSchema GroupSchema { get; }
    
    static ScimSchemaGenerator()
    {
        // Exécuté une seule fois, même en multi-thread
        UserSchema = GenerateSchema<ScimUser>();
        GroupSchema = GenerateSchema<ScimGroup>();
    }
}
```

## 🎨 Attributs disponibles

### `[ScimResource]` - Niveau classe

```csharp
[ScimResource(
    "urn:ietf:params:scim:schemas:core:2.0:User",  // URN du schéma
    "User",                                          // Nom
    "User Account"                                   // Description
)]
```

### `[ScimProperty]` - Niveau propriété

```csharp
[ScimProperty(
    "userName",              // Nom de l'attribut (obligatoire)
    "string",                // Type (obligatoire)
    Required = true,         // Obligatoire ?
    MultiValued = false,     // Multi-valué ?
    Description = "...",     // Description
    Uniqueness = "server",   // "none", "server", "global"
    Mutability = "readWrite", // "readOnly", "readWrite", "immutable", "writeOnly"
    Returned = "default",    // "always", "never", "default", "request"
    CaseExact = false        // Sensible à la casse ?
)]
```

**Types SCIM supportés** :
- `string`
- `boolean`
- `integer`
- `decimal`
- `dateTime`
- `reference`
- `complex`
- `binary`

## 🔧 Extension du système

### Créer un type personnalisé

```csharp
[ScimResource(
    "urn:enterprise:params:scim:schemas:extension:MyCompany:2.0",
    "EnterpriseUser",
    "Enterprise User with custom attributes")]
public class EnterpriseUser : ScimUser
{
    [ScimProperty("employeeNumber", "string", Required = true)]
    public string EmployeeNumber { get; set; }
    
    [ScimProperty("department", "string")]
    public string? Department { get; set; }
    
    [ScimProperty("badge", "complex")]
    public BadgeInfo? Badge { get; set; }
}

public class BadgeInfo
{
    [ScimProperty("badgeNumber", "string")]
    public string? BadgeNumber { get; set; }
    
    [ScimProperty("accessLevel", "integer")]
    public int AccessLevel { get; set; }
}
```

### Générer le schéma

```csharp
// Option 1 : Classe helper statique (RECOMMANDÉ)
public static class EnterpriseSchemaGenerator
{
    public static ScimSchema EnterpriseUserSchema { get; }
    
    static EnterpriseSchemaGenerator()
    {
        EnterpriseUserSchema = ScimSchemaGenerator.GetSchema<EnterpriseUser>();
    }
}

// Option 2 : À la demande (utilise la réflexion à chaque appel)
var schema = ScimSchemaGenerator.GetSchema<EnterpriseUser>();
```

### Utiliser dans un contrôleur

```csharp
[HttpGet("Schemas")]
public IActionResult GetSchemas()
{
    return Ok(new[] 
    { 
        ScimSchemaGenerator.UserSchema,
        ScimSchemaGenerator.GroupSchema,
        EnterpriseSchemaGenerator.EnterpriseUserSchema
    });
}
```

## 📚 Documentation complète

- **Guide d'extension détaillé** : `SCHEMA-EXTENSION-GUIDE.md`
- **Détails d'implémentation** : `SCHEMA-GENERATION-IMPLEMENTATION-COMPLETE.md`
- **Résumé court** : `SCHEMA-IMPLEMENTATION-COMPLETE.md`

## ✅ Tests

```bash
# Compilation
cd ScimAPI
dotnet build

# Démarrage
dotnet run

# Test des endpoints
curl http://localhost:5000/scim/Schemas -H "Authorization: Bearer <token>"
curl "http://localhost:5000/scim/Schemas/urn:ietf:params:scim:schemas:core:2.0:User" \
  -H "Authorization: Bearer <token>"
```

## 🎓 Exemples d'utilisation

### Propriétés système exclues
```csharp
public class ScimUser
{
    // Ces propriétés N'ONT PAS d'attribut = pas dans le schéma
    public string Id { get; set; }
    public List<string> Schemas { get; set; }
    public ScimMeta Meta { get; set; }
    public Dictionary<string, object> CustomAttributes { get; set; }
}
```

### Attributs multi-valués complexes
```csharp
[ScimProperty("emails", "complex", MultiValued = true)]
public List<ScimEmail> Emails { get; set; }

public class ScimEmail
{
    [ScimProperty("value", "string")]
    public string Value { get; set; }
    
    [ScimProperty("type", "string")]
    public string? Type { get; set; }
    
    [ScimProperty("primary", "boolean")]
    public bool Primary { get; set; }
}
```

### Attribut en lecture seule
```csharp
[ScimProperty("groups", "complex", MultiValued = true, Mutability = "readOnly")]
public List<ScimGroupMembership> Groups { get; set; }
```

## 🏗️ Architecture

```
┌─────────────────────────────────────────┐
│  ScimUser / ScimGroup                   │
│  (classes avec attributs)               │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  ScimSchemaGenerator                    │
│  (constructeur statique)                │
│  - Réflexion sur les types              │
│  - Lecture des attributs                │
│  - Découverte récursive                 │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  Propriétés statiques (cache)           │
│  - UserSchema                           │
│  - GroupSchema                          │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  Contrôleurs                            │
│  GET /scim/Schemas                      │
└─────────────────────────────────────────┘
```

## 🎯 Avantages

✅ **Déclaratif** : Schémas définis directement sur les modèles  
✅ **Type-safe** : Validation à la compilation  
✅ **Performant** : Calcul unique via constructeur statique  
✅ **Thread-safe** : Garanti par le runtime C#  
✅ **Extensible** : Support facile de types personnalisés  
✅ **Sécurisé** : Opt-in évite les fuites de données  
✅ **Maintenable** : Pas de code de schéma dupliqué  

## 📝 Notes

- Les schémas sont calculés au **premier accès** à `ScimSchemaGenerator.UserSchema` ou `.GroupSchema`
- Le constructeur statique garantit un **calcul unique** même en environnement multi-thread
- Les propriétés **héritées** sont automatiquement incluses (`BindingFlags.FlattenHierarchy`)
- Les types **complexes** sont analysés récursivement pour générer les sous-attributs

## 🤝 Contribution

Pour ajouter un nouveau type de ressource SCIM :

1. Créer la classe héritant de `ScimUser` ou `ScimGroup`
2. Annoter avec `[ScimResource]` et `[ScimProperty]`
3. Créer un helper statique pour pré-calculer le schéma
4. Exposer dans les endpoints `/scim/Schemas`

Voir `SCHEMA-EXTENSION-GUIDE.md` pour des exemples détaillés.
