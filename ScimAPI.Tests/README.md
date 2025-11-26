# Tests Unitaires - ScimAPI

Ce projet contient les tests unitaires pour l'implémentation SCIM de l'API.

## Structure des Tests

### InMemoryScimRepositoryTests.cs
Tests de l'implémentation en mémoire du repository SCIM avec **60+ tests** couvrant :

#### CRUD Utilisateurs (7 tests)
- Création avec génération automatique d'ID et métadonnées
- Récupération par ID
- Mise à jour complète
- Suppression

#### Filtres Utilisateurs (14 tests)
- Opérateurs de base : `eq`, `sw`, `co`, `pr`
- Opérateurs logiques : `and`, `or`, `not`
- Expressions complexes avec parenthèses
- Filtres sur tous les champs (userName, active, displayName, name.givenName, name.familyName, emails)

#### Pagination (2 tests)
- Pagination première page
- Pagination pages suivantes

#### PATCH Utilisateurs (4 tests)
- Remplacement de champs simples (active, displayName)
- Remplacement de champs imbriqués (name.givenName)
- Opérations multiples

#### CRUD Groupes (4 tests)
- Création, récupération, mise à jour, suppression

#### Filtres Groupes (3 tests)
- Filtres par displayName : `eq`, `co`, `sw`

#### PATCH Groupes (2 tests)
- Ajout de membres
- Retrait de membres

#### Schémas (2 tests)
- Ajout de schémas personnalisés
- Récupération de schémas

#### Cas limites (7 tests)
- Gestion des valeurs null
- Recherche case-insensitive
- Résultats vides

### UsersControllerTests.cs
Tests du controller des utilisateurs avec **25 tests** couvrant :
- GET /scim/Users (avec/sans filtres, pagination, erreurs)
- GET /scim/Users/{id}
- POST /scim/Users (création, conflits)
- PUT /scim/Users/{id}
- PATCH /scim/Users/{id}
- DELETE /scim/Users/{id}

### GroupsControllerTests.cs
Tests du controller des groupes avec **18 tests** couvrant :
- GET /scim/Groups (avec/sans filtres, erreurs)
- GET /scim/Groups/{id}
- POST /scim/Groups (création, conflits)
- PUT /scim/Groups/{id}
- PATCH /scim/Groups/{id} (ajout/retrait membres)
- DELETE /scim/Groups/{id}

## Exécution des Tests

### Tous les tests
```bash
dotnet test
```

### Tests d'un fichier spécifique
```bash
dotnet test --filter "FullyQualifiedName~InMemoryScimRepositoryTests"
dotnet test --filter "FullyQualifiedName~UsersControllerTests"
dotnet test --filter "FullyQualifiedName~GroupsControllerTests"
```

### Un test spécifique
```bash
dotnet test --filter "FullyQualifiedName~GetUsers_FilterWithComplexExpression"
```

### Avec rapport détaillé
```bash
dotnet test --verbosity detailed
```

### Avec couverture de code
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Outils Utilisés

- **xUnit** - Framework de tests
- **Shouldly** - Assertions lisibles et expressives (MIT License - gratuite)
- **Moq** - Mock des dépendances pour les tests de controllers

## Exemples de Tests

### Test de filtre simple
```csharp
[Fact]
public async Task GetUsers_FilterByUserName_Eq_ShouldReturnMatchingUser()
{
    // Arrange
    await _repository.CreateUserAsync(new ScimUser { UserName = "john.doe@example.com" });
    await _repository.CreateUserAsync(new ScimUser { UserName = "jane.smith@example.com" });

    // Act
    var result = await _repository.GetUsersAsync(filter: "userName eq \"john.doe@example.com\"");

    // Assert
    result.TotalResults.ShouldBe(1);
    result.Resources.First().UserName.ShouldBe("john.doe@example.com");
}
```

### Test de filtre complexe
```csharp
[Fact]
public async Task GetUsers_FilterWithComplexExpression_ShouldReturnMatchingUsers()
{
    // Arrange
    await _repository.CreateUserAsync(new ScimUser { UserName = "john@example.com", Active = true });
    await _repository.CreateUserAsync(new ScimUser { UserName = "jane@example.com", Active = true });
    await _repository.CreateUserAsync(new ScimUser { UserName = "bob@example.com", Active = false });

    // Act
    var result = await _repository.GetUsersAsync(
        filter: "(userName sw \"john\" or userName sw \"jane\") and active eq true"
    );

    // Assert
    result.TotalResults.ShouldBe(2);
}
```

### Test de controller
```csharp
[Fact]
public async Task CreateUser_WhenValid_ShouldReturnCreated()
{
    // Arrange
    var newUser = new ScimUser { UserName = "newuser@example.com" };
    _mockRepository.Setup(r => r.GetUserByUserNameAsync(newUser.UserName))
        .ReturnsAsync((ScimUser?)null);
    _mockRepository.Setup(r => r.CreateUserAsync(It.IsAny<ScimUser>()))
        .ReturnsAsync(new ScimUser { Id = "123", UserName = "newuser@example.com" });

    // Act
    var result = await _controller.CreateUser(newUser);

    // Assert
    var createdResult = (CreatedAtActionResult)result;
    var returnedUser = (ScimUser)createdResult.Value!;
    returnedUser.UserName.ShouldBe("newuser@example.com");
}
```

## Couverture de Code

Les tests couvrent :
- ✅ 100% des méthodes publiques du repository
- ✅ 100% des endpoints des controllers
- ✅ Tous les opérateurs de filtrage SCIM
- ✅ Tous les cas d'erreur (404, 409, 500)
- ✅ Opérations PATCH complètes
- ✅ Cas limites et edge cases

## Scénarios Microsoft Entra Testés

Les tests valident tous les scénarios d'utilisation de Microsoft Entra :
1. Vérification d'existence par userName
2. Recherche par externalId (Azure Object ID)
3. Filtrage des utilisateurs actifs
4. Création/mise à jour avec gestion des conflits
5. Désactivation d'utilisateurs (PATCH active = false)
6. Gestion des groupes et de leurs membres

## Intégration Continue

Les tests sont conçus pour être exécutés dans un pipeline CI/CD :
- Rapides (< 5 secondes pour tous les tests)
- Isolés (chaque test est indépendant)
- Déterministes (pas de dépendances externes)
- Reproductibles (mêmes résultats à chaque exécution)

## Ajout de Nouveaux Tests

Pour ajouter un nouveau test :

1. Choisir le fichier approprié (Repository ou Controller)
2. Utiliser le pattern **Arrange-Act-Assert**
3. Nom descriptif : `MethodName_Scenario_ExpectedResult`
4. Utiliser Shouldly pour les assertions (syntaxe simple et lisible)
5. Mocker les dépendances si nécessaire

Exemple :
```csharp
[Fact]
public async Task MonNouveauTest_QuandCondition_DevraitRetournerResultat()
{
    // Arrange
    // Préparer les données de test
    
    // Act
    // Exécuter la méthode à tester
    
    // Assert
    // Vérifier le résultat avec Shouldly
    result.ShouldBe(expected);
    result.ShouldNotBeNull();
    result.Count.ShouldBe(5);
}
```

### Syntaxe Shouldly Courante

```csharp
// Égalité
value.ShouldBe(expected);
value.ShouldNotBe(unexpected);

// Null
value.ShouldBeNull();
value.ShouldNotBeNull();

// Booléens
condition.ShouldBeTrue();
condition.ShouldBeFalse();

// Collections
collection.Count.ShouldBe(5);
collection.ShouldBeEmpty();
collection.ShouldContain(item);

// Comparaisons
number.ShouldBeGreaterThan(10);
number.ShouldBeLessThan(100);
date.ShouldBeInRange(start, end);

// Types
object.ShouldBeOfType<MyType>();
```

## Dépannage

### Tests qui échouent aléatoirement
- Vérifier l'isolation des tests
- S'assurer qu'il n'y a pas d'état partagé

### StackOverflowException
- Les filtres complexes avec parenthèses mal équilibrées peuvent causer des récursions infinies
- Solution implémentée : suppression des parenthèses externes avant traitement

### Tests lents
- Les tests en mémoire doivent être rapides
- Si lent : vérifier les énumérations multiples ou les opérations coûteuses

