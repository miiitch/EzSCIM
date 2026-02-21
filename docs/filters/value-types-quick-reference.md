## ⚡ Quick Reference - Filter Value Types

**Q: Quels sont les types de valeurs utilisables à droite des opérateurs?**

**A: 5 types principaux**

---

## Types de Valeurs - Vue d'Ensemble

### 1. 🔤 String (Chaîne)
```bash
filter=userName eq "john.doe"
filter=displayName co "John"
filter=emails.value ew "@company.com"
```
- Entre **guillemets** `"..."`
- Opérateurs: `eq`, `ne`, `co`, `sw`, `ew`

---

### 2. ✅ Boolean (Vrai/Faux)
```bash
filter=active eq true
filter=active eq false
filter=emailVerified eq true
```
- **Sans guillemets** - lowercase: `true` ou `false`
- Opérateurs: `eq`, `ne`

---

### 3. 🔢 Numeric (Nombre)
```bash
filter=id eq 12345
filter=id gt 1000
filter=salary le 50000
```
- **Sans guillemets** - entier ou décimal
- Opérateurs: `eq`, `ne`, `gt`, `ge`, `lt`, `le`

---

### 4. 📅 DateTime (Date/Heure)
```bash
filter=meta.created gt "2024-01-15T10:00:00Z"
filter=meta.lastModified lt "2024-06-01T00:00:00Z"
```
- Entre **guillemets** - Format ISO 8601: `"YYYY-MM-DDTHH:MM:SSZ"`
- Opérateurs: `eq`, `ne`, `gt`, `ge`, `lt`, `le`

---

### 5. ❓ Null/Absence (Présence)
```bash
filter=phoneNumbers pr
filter=not (manager pr)
filter=description pr
```
- **Pas de valeur** - juste l'opérateur `pr`
- Opérateurs: `pr` (present)

---

## Tableau Récapitulatif

| Type | Format | Opérateurs | Exemple |
|------|--------|-----------|---------|
| **String** | `"value"` | eq, ne, co, sw, ew | `"john.doe"` |
| **Boolean** | true/false | eq, ne | `true` |
| **Numeric** | number | eq, ne, gt, ge, lt, le | `1000` |
| **DateTime** | `"2024-01-15T10:00:00Z"` | eq, ne, gt, ge, lt, le | `"2024-01-15T10:00:00Z"` |
| **Null** | (none) | pr | `phoneNumbers pr` |

---

## Exemples par Type

### String
```bash
eq:   userName eq "john.doe"
ne:   userName ne "admin"
co:   displayName co "John"
sw:   userName sw "admin"
ew:   emails.value ew "@company.com"
```

### Boolean
```bash
eq:   active eq true
ne:   active ne false
```

### Numeric
```bash
eq:   id eq 12345
ne:   id ne 0
gt:   id gt 1000
ge:   id ge 1000
lt:   id lt 9999
le:   salary le 50000
```

### DateTime
```bash
eq:   meta.created eq "2024-01-15T10:00:00Z"
gt:   meta.created gt "2024-01-01T00:00:00Z"
lt:   meta.lastModified lt "2024-12-31T23:59:59Z"
```

### Null/Presence
```bash
pr:       phoneNumbers pr
not pr:   not (manager pr)
```

---

## Combinaisons

### String + Boolean
```bash
filter=active eq true and displayName co "John"
filter=active eq false and userName sw "admin"
```

### String + DateTime
```bash
filter=emails.value ew "@company.com" and meta.created gt "2024-01-01T00:00:00Z"
```

### Boolean + Numeric
```bash
filter=active eq true and id gt 1000
```

### Tout ensemble
```bash
filter=active eq true and emails.value ew "@company.com" and meta.created gt "2024-01-01T00:00:00Z" and phoneNumbers pr
```

---

## Piège Commun - Type Mismatch

### ❌ Mauvais
```bash
# String au lieu de numeric
filter=id eq "12345"

# Numeric au lieu de string
filter=userName eq 12345

# Boolean au lieu de string
filter=active eq "true"
```

### ✅ Bon
```bash
# Numeric
filter=id eq 12345

# String
filter=userName eq "12345"

# Boolean
filter=active eq true
```

---

## Cas Spéciaux

### Caractères Spéciaux dans Strings
```bash
# Guillemets doublés
filter=displayName eq "John ""The Boss"" Doe"

# Apostrophe (pas d'escape)
filter=displayName eq "O'Connor"

# Backslash
filter=description co "path\\to\\file"
```

### Attributs Imbriqués
```bash
filter=name.givenName eq "John"
filter=emails.value ew "@company.com"
filter=addresses.type eq "work"
```

### Multi-Valués
```bash
# Au moins un email de ce domaine
filter=emails.value ew "@company.com"

# Au moins un téléphone présent
filter=phoneNumbers pr
```

---

## Astuce PowerShell

```powershell
# Encoder automatiquement
$filter = 'active eq true and displayName co "John"'
$encoded = [uri]::EscapeDataString($filter)

# Utiliser
$uri = "https://localhost:7001/scim/Users?filter=$encoded"
```

---

## Astuce cURL

```bash
# cURL encode automatiquement
curl --get --data-urlencode 'filter=active eq true' \
  "https://localhost:7001/scim/Users"
```

---

## Résumé

| Besoin | Type | Format | Exemple |
|--------|------|--------|---------|
| Texte exact | String | `"value"` | `"john.doe"` |
| Texte partiel | String | `"value"` | `"John"` |
| Oui/Non | Boolean | true/false | `true` |
| Nombre | Numeric | number | `1000` |
| Date/Heure | DateTime | ISO 8601 | `"2024-01-15T10:00:00Z"` |
| Existe/N'existe pas | Null | pr | `phoneNumbers pr` |

---

Voir **FILTER-VALUE-TYPES.md** pour la documentation complète!
