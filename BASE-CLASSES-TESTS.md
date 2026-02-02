# Tests des Classes de Base SCIM

## Résumé

J'ai ajouté **10 nouveaux tests** dans `ScimAPI.Tests\ScimSchemaGeneratorTests.cs` pour vérifier que les classes de base `ScimUserBase` et `ScimGroupBase` contiennent bien les champs minimums requis conformément à la RFC 7643.

## Tests Ajoutés

### Tests pour ScimUserBase

#### 1. `ScimUserBase_ShouldHaveRequiredAttributesOnly()`
- **Objectif** : Vérifier que `ScimUserBase` ne contient que les attributs REQUIS
- **Vérifie** :
  - Le schéma a l'ID correct : `urn:ietf:params:scim:schemas:core:2.0:User`
  - Le nom du schéma est "User"
  - Un seul attribut est présent (userName)
  - L'attribut userName est bien présent

#### 2. `ScimUserBase_UserName_ShouldBeRequired()`
- **Objectif** : Vérifier que l'attribut `userName` est correctement configuré
- **Vérifie** :
  - userName est marqué comme REQUIS (`Required = true`)
  - Type est "string"
  - Unicité est définie à "server"

#### 3. `ScimUserBase_ShouldHaveSystemProperties()`
- **Objectif** : Vérifier que les propriétés système sont initialisées
- **Vérifie** :
  - `Id` est généré automatiquement
  - `Schemas` contient le bon URI
  - `Meta` est initialisé

### Tests pour ScimGroupBase

#### 4. `ScimGroupBase_ShouldHaveRequiredAttributesOnly()`
- **Objectif** : Vérifier que `ScimGroupBase` ne contient que les attributs REQUIS
- **Vérifie** :
  - Le schéma a l'ID correct : `urn:ietf:params:scim:schemas:core:2.0:Group`
  - Le nom du schéma est "Group"
  - Un seul attribut est présent (displayName)
  - L'attribut displayName est bien présent

#### 5. `ScimGroupBase_DisplayName_ShouldBeRequired()`
- **Objectif** : Vérifier que l'attribut `displayName` est correctement configuré
- **Vérifie** :
  - displayName est marqué comme REQUIS (`Required = true`)
  - Type est "string"

#### 6. `ScimGroupBase_ShouldHaveSystemProperties()`
- **Objectif** : Vérifier que les propriétés système sont initialisées
- **Vérifie** :
  - `Id` est généré automatiquement
  - `Schemas` contient le bon URI
  - `Meta` est initialisé

### Tests de Comparaison Base vs Complète

#### 7. `ScimUser_ShouldHaveMoreAttributesThanBase()`
- **Objectif** : Vérifier que `ScimUser` contient plus d'attributs que `ScimUserBase`
- **Vérifie** :
  - ScimUser a plus d'attributs que ScimUserBase
  - ScimUser a au moins 10 attributs (incluant les optionnels)

#### 8. `ScimGroup_ShouldHaveMoreAttributesThanBase()`
- **Objectif** : Vérifier que `ScimGroup` contient plus d'attributs que `ScimGroupBase`
- **Vérifie** :
  - ScimGroup a plus d'attributs que ScimGroupBase
  - ScimGroup contient les attributs optionnels : externalId, members

#### 9. `ScimUserBase_And_ScimUser_ShouldHaveSameSchemaId()`
- **Objectif** : Vérifier que la base et la classe complète partagent le même schéma
- **Vérifie** :
  - Les deux ont le même ID de schéma
  - Les deux ont le même nom de schéma

#### 10. `ScimGroupBase_And_ScimGroup_ShouldHaveSameSchemaId()`
- **Objectif** : Vérifier que la base et la classe complète partagent le même schéma
- **Vérifie** :
  - Les deux ont le même ID de schéma
  - Les deux ont le même nom de schéma

## Structure des Classes

### ScimUserBase (Attributs REQUIS uniquement)
```csharp
[ScimResource("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")]
public class ScimUserBase
{
    public string Id { get; set; }
    [ScimProperty(..., Required = true)]
    public string UserName { get; set; }
    public List<string> Schemas { get; set; }
    public ScimMeta Meta { get; set; }
}
```

### ScimUser (Attributs REQUIS + OPTIONNELS)
```csharp
public class ScimUser : ScimUserBase
{
    // +15 attributs optionnels
    public string? ExternalId { get; set; }
    public ScimName Name { get; set; }
    public string? DisplayName { get; set; }
    // ... etc
}
```

### ScimGroupBase (Attributs REQUIS uniquement)
```csharp
[ScimResource("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")]
public class ScimGroupBase
{
    public string Id { get; set; }
    [ScimProperty(..., Required = true)]
    public string DisplayName { get; set; }
    public List<string> Schemas { get; set; }
    public ScimMeta Meta { get; set; }
}
```

### ScimGroup (Attributs REQUIS + OPTIONNELS)
```csharp
public class ScimGroup : ScimGroupBase
{
    // +2 attributs optionnels
    public string? ExternalId { get; set; }
    public List<ScimMember> Members { get; set; }
    public Dictionary<string, object> CustomAttributes { get; set; }
}
```

## Exécution des Tests

### Via PowerShell Script
```powershell
.\Run-BaseClassesTests.ps1
```

### Via dotnet CLI
```powershell
# Tous les tests des classes de base
dotnet test --filter "FullyQualifiedName~Base"

# Test spécifique
dotnet test --filter "FullyQualifiedName~ScimUserBase_ShouldHaveRequiredAttributesOnly"

# Tous les tests de schéma
dotnet test ScimAPI.Tests/ScimAPI.Tests.csproj --filter "FullyQualifiedName~ScimSchemaGeneratorTests"
```

### Via Visual Studio / Rider
- Ouvrir `ScimSchemaGeneratorTests.cs`
- Naviguer vers la section `#region Base Classes Schema Tests`
- Exécuter les tests individuellement ou en groupe

## Fichiers Modifiés

1. **ScimAPI.Tests\ScimSchemaGeneratorTests.cs**
   - Ajout de 10 nouveaux tests dans une nouvelle région `#region Base Classes Schema Tests`
   - Ligne 507-657

2. **Run-BaseClassesTests.ps1** (nouveau fichier)
   - Script PowerShell pour exécuter facilement tous les tests des classes de base
   - Affiche un résumé coloré des résultats

## Conformité RFC 7643

Ces tests garantissent que :

✅ **ScimUserBase** contient uniquement l'attribut REQUIS `userName`
✅ **ScimGroupBase** contient uniquement l'attribut REQUIS `displayName`
✅ Les propriétés système (Id, Schemas, Meta) sont présentes dans les classes de base
✅ Les classes complètes (ScimUser, ScimGroup) héritent des bases et ajoutent les attributs optionnels
✅ L'attribut `[ScimResource]` est sur les classes de base (héritable)
✅ Les schémas générés reflètent correctement la hiérarchie

## Avantages de cette Architecture

1. **Séparation claire** entre attributs requis et optionnels
2. **Flexibilité** : possibilité d'utiliser la classe de base pour les opérations minimales
3. **Validation** : les tests garantissent la conformité SCIM
4. **Évolutivité** : facile d'ajouter de nouveaux attributs optionnels
5. **Type-safety** : le compilateur garantit la présence des attributs requis

## Date de Création

2026-02-02
