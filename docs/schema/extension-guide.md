﻿# SCIM Schema Extension Guide

Ce guide explique comment étendre le système de schémas SCIM en créant des types personnalisés basés sur `ScimUser` et `ScimGroup`.

## Table des matières

1. [Introduction au système d'attributs](#introduction-au-système-dattributs)
2. [Création d'un type utilisateur personnalisé](#création-dun-type-utilisateur-personnalisé)
3. [Configuration de l'injection de dépendances](#configuration-de-linjection-de-dépendances)
4. [Génération des schémas personnalisés](#génération-des-schémas-personnalisés)
5. [Bonnes pratiques](#bonnes-pratiques)

---

## Introduction au système d'attributs

Le système de schémas SCIM utilise une approche **déclarative opt-in** basée sur deux attributs :

### `[ScimResource]` - Niveau classe
Définit les métadonnées du schéma SCIM pour une ressource.

```csharp
[ScimResource(
    "urn:ietf:params:scim:schemas:core:2.0:User",
    "User",
    "User Account"
)]
public class ScimUser { ... }
```

**Propriétés** :
- `Schema` (obligatoire) : URN du schéma SCIM
- `Name` (obligatoire) : Nom de la ressource
- `Description` (obligatoire) : Description de la ressource

### `[ScimProperty]` - Niveau propriété
Marque une propriété comme faisant partie du schéma SCIM. **Seules les propriétés annotées sont incluses dans le schéma généré.**

```csharp
[ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
public string UserName { get; set; }
```

**Propriétés** :
- `Name` (obligatoire) : Nom de l'attribut SCIM
- `Type` (obligatoire) : Type de données SCIM ("string", "boolean", "complex", "reference", "dateTime", "integer", "decimal", "binary")
- `Required` (défaut: false) : Attribut obligatoire
- `MultiValued` (défaut: false) : Peut avoir plusieurs valeurs
- `Description` : Description de l'attribut
- `Uniqueness` (défaut: "none") : Contrainte d'unicité ("none", "server", "global")
- `Mutability` (défaut: "readWrite") : Mutabilité ("readOnly", "readWrite", "immutable", "writeOnly")
- `Returned` (défaut: "default") : Quand retourner l'attribut ("always", "never", "default", "request")
- `CaseExact` (défaut: false) : Comparaison sensible à la casse

---

## Création d'un type utilisateur personnalisé

### Exemple : Utilisateur d'entreprise avec attributs supplémentaires

```csharp
using ScimAPI.Attributes;
using ScimAPI.Models;

namespace MyCompany.Scim.Models
{
    /// <summary>
    /// Extension du modèle ScimUser pour inclure des attributs spécifiques à l'entreprise.
    /// </summary>
    [ScimResource(
        "urn:enterprise:params:scim:schemas:extension:MyCompany:2.0",
        "EnterpriseUser",
        "Enterprise User with custom attributes"
    )]
    public class EnterpriseUser : ScimUser
    {
        /// <summary>
        /// Numéro d'employé unique
        /// </summary>
        [ScimProperty("employeeNumber", "string", Required = true, Uniqueness = "server", 
            Description = "Unique employee number")]
        public string EmployeeNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Code du département
        /// </summary>
        [ScimProperty("department", "string", Description = "Department code")]
        public string? Department { get; set; }
        
        /// <summary>
        /// Nom du manager
        /// </summary>
        [ScimProperty("manager", "reference", Description = "Reference to manager")]
        public string? ManagerId { get; set; }
        
        /// <summary>
        /// Date d'embauche
        /// </summary>
        [ScimProperty("hireDate", "dateTime", Description = "Employee hire date")]
        public DateTime? HireDate { get; set; }
        
        /// <summary>
        /// Centres de coûts (multi-valué)
        /// </summary>
        [ScimProperty("costCenters", "string", MultiValued = true, 
            Description = "Cost centers")]
        public List<string> CostCenters { get; set; } = new();
        
        /// <summary>
        /// Informations de badge (type complexe)
        /// </summary>
        [ScimProperty("badge", "complex", Description = "Badge information")]
        public BadgeInfo? Badge { get; set; }
    }
    
    /// <summary>
    /// Informations de badge d'employé
    /// </summary>
    public class BadgeInfo
    {
        [ScimProperty("badgeNumber", "string", Description = "Badge number")]
        public string? BadgeNumber { get; set; }
        
        [ScimProperty("issueDate", "dateTime", Description = "Badge issue date")]
        public DateTime? IssueDate { get; set; }
        
        [ScimProperty("expiryDate", "dateTime", Description = "Badge expiry date")]
        public DateTime? ExpiryDate { get; set; }
        
        [ScimProperty("accessLevel", "integer", Description = "Access level")]
        public int AccessLevel { get; set; }
    }
}
```

### Points importants

1. **Héritage des propriétés** : Toutes les propriétés annotées de `ScimUser` (classe parente) sont automatiquement incluses dans le schéma généré.

2. **URN personnalisé** : Utilisez le format `urn:enterprise:params:scim:schemas:extension:{OrganizationName}:{Version}` pour les schémas d'extension.

3. **Types complexes** : Les types complexes (comme `BadgeInfo`) doivent avoir leurs propriétés annotées avec `[ScimProperty]`.

---

## Configuration de l'injection de dépendances

### Option 1 : Repository dédié pour le type personnalisé

```csharp
using ScimAPI.Repositories;
using MyCompany.Scim.Models;

namespace MyCompany.Scim.Repositories
{
    public class EnterpriseUserRepository : IScimUserOnlyRepository<EnterpriseUser>
    {
        private readonly Dictionary<string, EnterpriseUser> _users = new();
        
        public Task<EnterpriseUser?> GetUserAsync(string id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }
        
        // ... autres méthodes ...
    }
}
```

**Enregistrement dans Program.cs** :
```csharp
builder.Services.AddSingleton<IScimUserOnlyRepository<EnterpriseUser>, EnterpriseUserRepository>();
```

### Option 2 : Repository combiné (Users + Groups)

```csharp
public class EnterpriseScimRepository : 
    IScimUserGroupRepository<EnterpriseUser, ScimGroup>
{
    // Implements both user and group operations
    // Groups inherit user operations via interface hierarchy
}
```

**Enregistrement dans Program.cs** :
```csharp
builder.Services.AddSingleton<EnterpriseScimRepository>();
builder.Services.AddSingleton<IScimUserOnlyRepository<EnterpriseUser>>(sp => 
    sp.GetRequiredService<EnterpriseScimRepository>());
builder.Services.AddSingleton<IScimUserGroupRepository<EnterpriseUser, ScimGroup>>(sp => 
    sp.GetRequiredService<EnterpriseScimRepository>());
```

---

## Génération des schémas personnalisés

### Méthode 1 : Classe helper statique (RECOMMANDÉ)

Créez une classe helper similaire à `ScimSchemaGenerator` pour vos types personnalisés :

```csharp
using ScimAPI.Helpers;
using ScimAPI.Models;
using MyCompany.Scim.Models;

namespace MyCompany.Scim.Helpers
{
    public static class EnterpriseSchemaGenerator
    {
        /// <summary>
        /// Pre-generated schema for EnterpriseUser.
        /// Calculated once in static constructor (thread-safe).
        /// </summary>
        public static ScimSchema EnterpriseUserSchema { get; }
        
        static EnterpriseSchemaGenerator()
        {
            EnterpriseUserSchema = ScimSchemaGenerator.GetSchema<EnterpriseUser>();
            Console.WriteLine($"[EnterpriseSchemaGenerator] EnterpriseUser schema initialized: {EnterpriseUserSchema.Attributes.Count} attributes");
        }
    }
}
```

**Utilisation dans un contrôleur** :
```csharp
[HttpGet("Schemas")]
public IActionResult GetSchemas()
{
    var schemas = new List<ScimSchema> 
    { 
        ScimSchemaGenerator.UserSchema,
        ScimSchemaGenerator.GroupSchema,
        EnterpriseSchemaGenerator.EnterpriseUserSchema
    };
    return Ok(schemas);
}
```

### Méthode 2 : Génération à la demande

Si vous n'avez pas besoin de pré-calcul, appelez directement :

```csharp
var schema = ScimSchemaGenerator.GetSchema<EnterpriseUser>();
```

**⚠️ Note** : Cette méthode utilise la réflexion à chaque appel. Préférez la mise en cache via un helper statique.

---

## Bonnes pratiques

### 1. Convention de nommage des URNs

Pour les schémas d'extension personnalisés :
```
urn:enterprise:params:scim:schemas:extension:{OrganizationName}:{Version}
```

Exemples :
- `urn:enterprise:params:scim:schemas:extension:Contoso:2.0`
- `urn:enterprise:params:scim:schemas:extension:AcmeCorp:1.0`

Pour les schémas core SCIM (ne pas modifier) :
- `urn:ietf:params:scim:schemas:core:2.0:User`
- `urn:ietf:params:scim:schemas:core:2.0:Group`

### 2. Approche opt-in

**Seules les propriétés avec `[ScimProperty]` sont incluses dans le schéma.** Cela permet de :
- Exclure les propriétés internes (Id, Schemas, Meta, CustomAttributes)
- Contrôler précisément ce qui est exposé dans le schéma SCIM
- Éviter les fuites de données sensibles

### 3. Héritage des annotations

Les propriétés annotées dans la classe parente (`ScimUser`, `ScimGroup`) sont automatiquement incluses dans les classes dérivées grâce à `BindingFlags.FlattenHierarchy`.

**Exemple** :
```csharp
// ScimUser a userName annoté
public class ScimUser 
{
    [ScimProperty("userName", "string", Required = true)]
    public string UserName { get; set; }
}

// EnterpriseUser hérite automatiquement de userName dans son schéma
public class EnterpriseUser : ScimUser 
{
    [ScimProperty("employeeNumber", "string")]
    public string EmployeeNumber { get; set; }
}
```

Le schéma généré pour `EnterpriseUser` contiendra **à la fois** `userName` et `employeeNumber`.

### 4. Types complexes imbriqués

Pour les attributs complexes, annotez également les propriétés des types imbriqués :

```csharp
// Propriété complexe sur le type principal
[ScimProperty("address", "complex")]
public CustomAddress Address { get; set; }

// Propriétés annotées sur le type imbriqué
public class CustomAddress
{
    [ScimProperty("street", "string")]
    public string Street { get; set; }
    
    [ScimProperty("city", "string")]
    public string City { get; set; }
}
```

### 5. Multi-valued complex types

Pour les listes de types complexes :

```csharp
[ScimProperty("certifications", "complex", MultiValued = true)]
public List<Certification> Certifications { get; set; } = new();

public class Certification
{
    [ScimProperty("name", "string")]
    public string Name { get; set; }
    
    [ScimProperty("issueDate", "dateTime")]
    public DateTime IssueDate { get; set; }
}
```

### 6. Validation au démarrage

Pour valider que vos schémas sont correctement générés, ajoutez une vérification au démarrage :

```csharp
// Dans Program.cs après builder.Build()
var schema = EnterpriseSchemaGenerator.EnterpriseUserSchema;
if (schema.Attributes.Count == 0)
{
    app.Logger.LogWarning("EnterpriseUser schema has no attributes!");
}
else
{
    app.Logger.LogInformation("EnterpriseUser schema loaded with {Count} attributes", schema.Attributes.Count);
}
```

---

## Résumé

1. **Annotez votre classe** avec `[ScimResource]` pour définir l'URN et les métadonnées
2. **Annotez les propriétés** avec `[ScimProperty]` (opt-in : seules les propriétés annotées sont incluses)
3. **Les propriétés héritées** sont automatiquement incluses
4. **Créez un helper statique** pour pré-calculer le schéma (thread-safe, performant)
5. **Utilisez des URNs d'extension** au format `urn:enterprise:params:scim:schemas:extension:{Org}:{Version}`
6. **Configurez l'injection de dépendances** pour vos repositories personnalisés

Le système de génération de schémas est automatique, type-safe, et optimisé pour la performance grâce aux constructeurs statiques.
