# Filtres SCIM Supportés

Ce document décrit les opérateurs et filtres SCIM supportés par l'API, compatibles avec Microsoft Entra (Azure AD).

## Opérateurs Logiques

### AND
Combine plusieurs conditions qui doivent toutes être vraies.

**Exemples :**
```
userName eq "john.doe@example.com" and active eq true
name.givenName co "John" and name.familyName co "Doe"
```

### OR
Combine plusieurs conditions dont au moins une doit être vraie.

**Exemples :**
```
userName eq "john.doe@example.com" or userName eq "jane.doe@example.com"
displayName co "Admin" or displayName co "Manager"
```

### NOT
Inverse une condition.

**Exemples :**
```
not (active eq false)
not (userName sw "test")
```

### Expressions Complexes
Vous pouvez combiner les opérateurs avec des parenthèses.

**Exemples :**
```
(userName sw "john" or userName sw "jane") and active eq true
```

## Opérateurs de Comparaison

### eq (égal)
Vérifie l'égalité stricte.

**Exemples :**
```
userName eq "john.doe@example.com"
externalId eq "12345"
active eq true
```

### ne (différent)
Vérifie la non-égalité (supporté par l'extraction de valeur, à implémenter dans les filtres).

### co (contient)
Vérifie si une chaîne contient une sous-chaîne.

**Exemples :**
```
userName co "doe"
displayName co "Admin"
name.givenName co "John"
```

### sw (commence par)
Vérifie si une chaîne commence par une sous-chaîne.

**Exemples :**
```
userName sw "john"
externalId sw "EXT-"
displayName sw "Test"
```

### ew (finit par)
Vérifie si une chaîne finit par une sous-chaîne (supporté par l'extraction de valeur).

**Exemples :**
```
userName ew "@example.com"
```

### pr (présent)
Vérifie si un attribut est présent et non vide.

**Exemples :**
```
userName pr
displayName pr
externalId pr
```

### gt, ge, lt, le (comparaisons numériques/dates)
Opérateurs pour comparer des nombres ou des dates (supportés par l'extraction de valeur, à implémenter dans les filtres).

## Attributs Supportés pour les Utilisateurs

### Attributs de Base
- `userName` - Nom d'utilisateur unique
- `externalId` - Identifiant externe
- `displayName` - Nom d'affichage
- `active` - État actif/inactif (booléen)

### Attributs de Nom (name.*)
- `name.givenName` - Prénom
- `name.familyName` - Nom de famille
- `name.formatted` - Nom complet formaté
- `name.middleName` - Second prénom
- `name.honorificPrefix` - Préfixe honorifique (M., Mme, etc.)
- `name.honorificSuffix` - Suffixe honorifique

### Attributs Complexes
- `emails[type eq "work"].value` - Email de type "work"
- `emails.value` - N'importe quel email
- `phoneNumbers.value` - N'importe quel numéro de téléphone
- `addresses.streetAddress` - Adresse postale

### Autres Attributs
- `title` - Titre professionnel
- `userType` - Type d'utilisateur
- `preferredLanguage` - Langue préférée
- `locale` - Locale
- `timezone` - Fuseau horaire

## Attributs Supportés pour les Groupes

### Attributs de Base
- `displayName` - Nom d'affichage du groupe
- `externalId` - Identifiant externe

### Attributs de Membres
- `members[value eq "user-id"]` - Recherche par ID de membre
- `members.value` - N'importe quel membre

## Exemples d'Utilisation avec Microsoft Entra

### Recherche d'utilisateurs
```bash
# Rechercher un utilisateur par userName
GET /scim/Users?filter=userName eq "john.doe@example.com"

# Rechercher les utilisateurs actifs
GET /scim/Users?filter=active eq true

# Rechercher par nom et prénom
GET /scim/Users?filter=name.givenName eq "John" and name.familyName eq "Doe"

# Rechercher les utilisateurs dont le nom contient "Admin"
GET /scim/Users?filter=displayName co "Admin"

# Rechercher par external ID
GET /scim/Users?filter=externalId eq "EXT-12345"

# Combinaison complexe
GET /scim/Users?filter=(userName sw "john" or displayName co "Doe") and active eq true
```

### Recherche de groupes
```bash
# Rechercher un groupe par nom
GET /scim/Groups?filter=displayName eq "Administrators"

# Rechercher les groupes dont le nom contient "Dev"
GET /scim/Groups?filter=displayName co "Dev"

# Rechercher par external ID
GET /scim/Groups?filter=externalId eq "GRP-001"
```

## Pagination

L'API supporte la pagination avec les paramètres :
- `startIndex` - Index de départ (commence à 1)
- `count` - Nombre d'éléments par page

**Exemple :**
```bash
GET /scim/Users?startIndex=1&count=100
GET /scim/Users?filter=active eq true&startIndex=1&count=50
```

## Notes pour Microsoft Entra

Microsoft Entra (Azure AD) utilise principalement ces filtres lors de la synchronisation :

1. **Vérification d'existence** : `userName eq "user@domain.com"`
2. **Recherche par externalId** : `externalId eq "azure-object-id"`
3. **Vérification de groupe** : `displayName eq "GroupName"`
4. **État actif** : `active eq true` ou `active eq false`

L'API est configurée pour gérer ces cas d'usage de manière optimale.

## Extensions et Attributs Personnalisés

Les attributs personnalisés définis dans `appsettings.Scim.json` sont automatiquement supportés et stockés dans le dictionnaire `CustomAttributes` des utilisateurs et groupes.

Pour filtrer par attributs personnalisés, utilisez la syntaxe SCIM standard :
```
urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:employeeNumber eq "12345"
```

*Note : Le filtrage par attributs personnalisés nécessitera une implémentation supplémentaire dans `ApplyUserFilter`.*

