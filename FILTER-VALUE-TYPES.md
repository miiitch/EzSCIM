## 📝 SCIM Filter Values - Types de Valeurs Supportées

Cette documentation décrit tous les types de valeurs que vous pouvez utiliser à droite des opérateurs binaires dans les filtres SCIM.

---

## Types de Valeurs Supportées

### 1. String (Chaîne de Caractères)

**Format:**
```
attributeName OPERATOR "string value"
```

**Opérateurs supportés:** `eq`, `ne`, `co`, `sw`, `ew`

**Exemples:**
```bash
# Égal
filter=userName eq "john.doe"

# Contient
filter=displayName co "John"

# Commence par
filter=userName sw "admin"

# Finit par
filter=emails.value ew "@company.com"

# Pas égal
filter=userType ne "Guest"

# Avec caractères spéciaux
filter=displayName eq "O'Connor"

# Avec espaces
filter=displayName eq "John Doe"
```

**Cas sensibilité:**
- Noms d'attributs: **Case-sensitive**
- Valeurs: **Case-insensitive** (par défaut)

---

### 2. Boolean (Vrai/Faux)

**Format:**
```
attributeName OPERATOR true|false
```

**Opérateurs supportés:** `eq`, `ne`

**Exemples:**
```bash
# Utilisateurs actifs
filter=active eq true

# Utilisateurs inactifs
filter=active eq false

# Pas actif
filter=active ne true

# Utilisateurs dont email est vérifié
filter=emailVerified eq true
```

**Notes:**
- Les valeurs booléennes ne sont **PAS entre guillemets**
- Lowercase: `true` et `false` (pas `True` ou `False`)

---

### 3. Numeric (Nombres)

**Format:**
```
attributeName OPERATOR number
```

**Opérateurs supportés:** `eq`, `ne`, `gt`, `ge`, `lt`, `le`

**Exemples:**
```bash
# ID égal à
filter=id eq 12345

# ID plus grand que
filter=id gt 1000

# ID moins que ou égal à
filter=id le 9999

# Salaire supérieur à
filter=salary gt 50000

# Nombre d'employés
filter=memberCount ge 5
```

**Notes:**
- Les nombres ne sont **PAS entre guillemets**
- Peut être entier ou décimal
- Comparaisons numériques uniquement (`gt`, `ge`, `lt`, `le`)

---

### 4. DateTime (Date/Heure - Spécification)

**Format:**
```
attributeName OPERATOR "2024-01-15T10:00:00Z"
```

**Opérateurs supportés:** `eq`, `ne`, `gt`, `ge`, `lt`, `le`

**Format ISO 8601:**
```
YYYY-MM-DDTHH:MM:SSZ
```

**Exemples:**
```bash
# Créé après une date
filter=meta.created gt "2024-01-01T00:00:00Z"

# Modifié avant une date
filter=meta.lastModified lt "2024-12-31T23:59:59Z"

# Créé à une date exacte
filter=meta.created eq "2024-01-15T10:00:00Z"

# Utilisateurs ajoutés récemment
filter=meta.created ge "2024-01-01T00:00:00Z"
```

**Variants de format:**
```bash
# Avec timezone
filter=meta.created gt "2024-01-15T10:00:00+02:00"

# UTC
filter=meta.created gt "2024-01-15T10:00:00Z"

# Sans timezone (traité comme UTC)
filter=meta.created gt "2024-01-15T10:00:00"
```

---

### 5. Null/Absent (Présence)

**Format:**
```
attributeName pr|not (attributeName pr)
```

**Opérateurs supportés:** `pr` (present)

**Exemples:**
```bash
# Utilisateurs avec un numéro de téléphone
filter=phoneNumbers pr

# Utilisateurs sans numéro de téléphone
filter=not (phoneNumbers pr)

# Utilisateurs avec adresse email
filter=emails pr

# Utilisateurs sans description
filter=not (description pr)

# Utilisateurs avec manager
filter=manager pr
```

**Notes:**
- `pr` vérifie juste **la présence**, pas la valeur
- Utiliser `not (attr pr)` pour l'absence
- Peut s'appliquer à tout attribut

---

## Types de Données par Attribut

### Attributs String Courants

| Attribut | Type | Exemple |
|----------|------|---------|
| `userName` | String | `"john.doe"` |
| `displayName` | String | `"John Doe"` |
| `emails.value` | String | `"john@company.com"` |
| `title` | String | `"Manager"` |
| `department` | String | `"Engineering"` |
| `userType` | String | `"Employee"` |
| `description` | String | `"Description text"` |

### Attributs Boolean Courants

| Attribut | Type | Valeurs |
|----------|------|---------|
| `active` | Boolean | `true`, `false` |
| `emailVerified` | Boolean | `true`, `false` |
| `phoneNumberVerified` | Boolean | `true`, `false` |

### Attributs DateTime Courants

| Attribut | Type | Format |
|----------|------|--------|
| `meta.created` | DateTime | `"2024-01-15T10:00:00Z"` |
| `meta.lastModified` | DateTime | `"2024-01-15T10:00:00Z"` |

### Attributs Numeric Courants

| Attribut | Type | Exemple |
|----------|------|---------|
| `id` | Numeric | `12345` |
| `memberCount` | Numeric | `10` |

---

## Exemples par Type - Users

### String Values

```bash
# Exact match
filter=userName eq "john.doe"

# Contains
filter=displayName co "John"

# Starts with
filter=userName sw "admin"

# Ends with (email domain)
filter=emails.value ew "@company.com"

# Multiple email domains (OR)
filter=emails.value ew "@company.com" or emails.value ew "@company.org"
```

### Boolean Values

```bash
# Active users
filter=active eq true

# Inactive users
filter=active eq false

# Active AND verified
filter=active eq true and emailVerified eq true
```

### DateTime Values

```bash
# Created after date
filter=meta.created gt "2024-01-01T00:00:00Z"

# Modified before date
filter=meta.lastModified lt "2024-06-01T00:00:00Z"

# Created in specific month
filter=meta.created ge "2024-01-01T00:00:00Z" and meta.created lt "2024-02-01T00:00:00Z"
```

### Null/Presence Values

```bash
# Has phone number
filter=phoneNumbers pr

# No phone number
filter=not (phoneNumbers pr)

# Has manager
filter=manager pr
```

---

## Exemples par Type - Groups

### String Values

```bash
# Group by exact name
filter=displayName eq "Engineering Team"

# Groups containing word
filter=displayName co "Engineering"

# Groups starting with
filter=displayName sw "Team"

# Groups ending with
filter=displayName ew "Committee"

# Multiple patterns (OR)
filter=displayName co "Engineering" or displayName co "Architecture"
```

### Presence

```bash
# Groups with description
filter=description pr

# Groups without description
filter=not (description pr)
```

---

## Combinaisons de Types

### String + Boolean

```bash
# Active users named John
filter=active eq true and displayName co "John"

# Inactive admins
filter=active eq false and userName sw "admin"
```

### String + DateTime

```bash
# Created after date AND from specific domain
filter=meta.created gt "2024-01-01T00:00:00Z" and emails.value ew "@company.com"

# Modified recently AND is active
filter=meta.lastModified gt "2024-01-01T00:00:00Z" and active eq true
```

### Boolean + DateTime

```bash
# Active AND created recently
filter=active eq true and meta.created gt "2024-01-01T00:00:00Z"

# Verified AND modified this month
filter=emailVerified eq true and meta.lastModified ge "2024-01-01T00:00:00Z"
```

### String + Presence

```bash
# Users named John with phone
filter=displayName co "John" and phoneNumbers pr

# Admins without manager
filter=userName sw "admin" and not (manager pr)
```

### Tout Combiné

```bash
# Active users from company, created recently, with phone
filter=active eq true and emails.value ew "@company.com" and meta.created gt "2024-01-01T00:00:00Z" and phoneNumbers pr
```

---

## Escaped Values (Caractères Spéciaux)

### Guillemets dans les Valeurs

```bash
# Guillemets doublés
filter=displayName eq "John ""The Boss"" Doe"

# Valeur avec apostrophe
filter=displayName eq "O'Connor"

# Valeur avec parenthèses (pas besoin d'escape)
filter=description co "Team (Sales)"
```

### Caractères Spéciaux

```bash
# Slash
filter=description co "path/to/file"

# Backslash (escape avec backslash)
filter=description co "path\\to\\file"

# Dollar sign
filter=description co "price $100"

# Pound sign
filter=description co "#hashtag"
```

---

## Limitations et Restrictions

### Pas d'Arithmetic

```bash
# ❌ PAS SUPPORTÉ
filter=salary gt salary * 2

# ❌ PAS SUPPORTÉ
filter=age lt (2024 - 1990)
```

### Pas de Conversions de Type

```bash
# ❌ PAS SUPPORTÉ - String ne peut pas être comparé numériquement
filter=id eq "12345"

# ✅ BON - Numérique
filter=id eq 12345
```

### Pas de Valeurs NULL Explicites

```bash
# ❌ PAS SUPPORTÉ
filter=manager eq null

# ✅ BON - Utiliser 'pr' pour la présence
filter=not (manager pr)
```

---

## Cas Particuliers

### Noms d'Attributs Composés

```bash
# Attribut imbriqué
filter=name.givenName eq "John"

# Attribut multi-valué
filter=emails.value eq "john@company.com"

# Attribut imbriqué multi-valué
filter=addresses.streetAddress co "Main St"
```

### Attributs Multi-Valués

```bash
# Au moins un email de ce domaine
filter=emails.value ew "@company.com"

# Au moins un téléphone (présence)
filter=phoneNumbers pr

# Au moins une adresse avec type "work"
filter=addresses.type eq "work"
```

### Comparaisons Implicites

```bash
# String comparison (insensitive par défaut)
filter=userName eq "John.Doe"  # Peut matcher "john.doe"

# Boolean comparison
filter=active eq true           # Exact match

# Numeric comparison
filter=id gt 100                # Valeur > 100
```

---

## Table de Référence - Types et Opérateurs

| Type | Opérateurs Supportés | Format | Exemple |
|------|---------------------|--------|---------|
| **String** | eq, ne, co, sw, ew | `"value"` | `"john"` |
| **Boolean** | eq, ne | `true`/`false` | `true` |
| **Numeric** | eq, ne, gt, ge, lt, le | `number` | `100` |
| **DateTime** | eq, ne, gt, ge, lt, le | `"2024-01-15T..."` | `"2024-01-15T10:00:00Z"` |
| **Null** | pr | - | `phoneNumbers pr` |

---

## PowerShell Examples

```powershell
$token = "YOUR_TOKEN"
$baseUrl = "https://localhost:7001"

# String value
$filter = 'userName eq "john.doe"'
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$result = Invoke-RestMethod -Uri $uri -Headers $headers

# Boolean value
$filter = "active eq true"
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$result = Invoke-RestMethod -Uri $uri -Headers $headers

# DateTime value
$filter = 'meta.created gt "2024-01-01T00:00:00Z"'
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$result = Invoke-RestMethod -Uri $uri -Headers $headers

# Numeric value
$filter = "id gt 1000"
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$result = Invoke-RestMethod -Uri $uri -Headers $headers

# Presence check
$filter = "phoneNumbers pr"
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$result = Invoke-RestMethod -Uri $uri -Headers $headers
```

---

## cURL Examples

```bash
# String value
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=userName%20eq%20%22john.doe%22"

# Boolean value
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20true"

# DateTime value
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=meta.created%20gt%20%222024-01-01T00%3A00%3A00Z%22"

# Numeric value
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=id%20gt%201000"

# Presence check
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=phoneNumbers%20pr"
```

---

## Références RFC 7644

- [Filter Values](https://tools.ietf.org/html/rfc7644#section-3.4.2.2)
- [Attribute Value Comparisons](https://tools.ietf.org/html/rfc7644#section-3.4.2.2.4)
- [DateTime Format](https://tools.ietf.org/html/rfc7644#section-3.3)
