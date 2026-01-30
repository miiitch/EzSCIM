## 🔗 URL Encoding pour les Filtres SCIM

Quand vous utilisez des filtres dans les URL, certains caractères doivent être encodés.

---

## Caractères à Encoder

| Caractère | Code | Usage |
|-----------|------|-------|
| ` ` (espace) | `%20` | Séparation entre opérateurs |
| `(` | `%28` | Parenthèse ouvrante |
| `)` | `%29` | Parenthèse fermante |
| `"` | `%22` | Guillemets (valeurs) |
| `@` | `%40` | Symbole at (@) |
| `-` | `%2D` | Tiret |
| `_` | `%5F` | Underscore |
| `.` | `%2E` | Point |

---

## Exemples d'Encoding

### Exemple 1: Filtre simple

**Sans encoding:**
```
active eq true
```

**Avec encoding:**
```
active%20eq%20true
```

### Exemple 2: Avec guillemets

**Sans encoding:**
```
userName eq "john.doe"
```

**Avec encoding:**
```
userName%20eq%20%22john.doe%22
```

### Exemple 3: Avec email (@)

**Sans encoding:**
```
emails.value ew "@company.com"
```

**Avec encoding:**
```
emails.value%20ew%20%22%40company.com%22
```

### Exemple 4: Imbriqué avec parenthèses

**Sans encoding:**
```
active eq true and (title eq "Admin" or title eq "Manager")
```

**Avec encoding:**
```
active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22%29
```

### Exemple 5: Très imbriqué

**Sans encoding:**
```
((active eq true and (title eq "Admin" or title eq "Manager")) or (active eq false and title eq "Director")) and departmentName eq "Engineering"
```

**Avec encoding:**
```
%28%28active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22%29%29%20or%20%28active%20eq%20false%20and%20title%20eq%20%22Director%22%29%29%20and%20departmentName%20eq%20%22Engineering%22
```

---

## Outils pour Encoder

### En Ligne
- https://www.urlencoder.org/
- https://www.urldecoder.org/

### PowerShell
```powershell
[uri]::EscapeDataString("active eq true")
# Résultat: active%20eq%20true

[uri]::EscapeDataString('userName eq "john.doe"')
# Résultat: userName%20eq%20%22john.doe%22
```

### cURL (automatique)
```bash
# cURL encode automatiquement avec --data-urlencode
curl --get --data-urlencode 'filter=active eq true' \
  "https://localhost:7001/scim/Users" \
  -H "Authorization: Bearer $TOKEN"
```

### Python
```python
from urllib.parse import quote
quote('active eq true')
# Résultat: 'active%20eq%20true'

quote('userName eq "john.doe"')
# Résultat: 'userName%20eq%20%22john.doe%22'
```

### JavaScript
```javascript
encodeURIComponent('active eq true')
// Résultat: 'active%20eq%20true'

encodeURIComponent('userName eq "john.doe"')
// Résultat: 'userName%20eq%20%22john.doe%22'
```

---

## Tableau Complet d'Encoding ASCII

| Caractère | Code | Caractère | Code |
|-----------|------|-----------|------|
| ` ` | %20 | `!` | %21 |
| `"` | %22 | `#` | %23 |
| `$` | %24 | `%` | %25 |
| `&` | %26 | `'` | %27 |
| `(` | %28 | `)` | %29 |
| `*` | %2A | `+` | %2B |
| `,` | %2C | `-` | %2D |
| `.` | %2E | `/` | %2F |
| `:` | %3A | `;` | %3B |
| `<` | %3C | `=` | %3D |
| `>` | %3E | `?` | %3F |
| `@` | %40 | `[` | %5B |
| `\` | %5C | `]` | %5D |
| `^` | %5E | `` ` `` | %60 |
| `{` | %7B | `\|` | %7C |
| `}` | %7D | `~` | %7E |

---

## Exemples Pratiques

### PowerShell - Encoding Automatique

```powershell
# Les bons outils font l'encoding automatiquement
$filter = 'active eq true and (title eq "Admin" or title eq "Manager")'
$encodedFilter = [uri]::EscapeDataString($filter)

Write-Host "Filtre original: $filter"
Write-Host "Filtre encodé: $encodedFilter"

# Utiliser dans une URL
$uri = "https://localhost:7001/scim/Users?filter=$encodedFilter"
$response = Invoke-RestMethod -Uri $uri -Headers $headers
```

### Construct URL Manuellement

```powershell
$baseUrl = "https://localhost:7001/scim/Users"
$filter = "active eq true"

# Encoder le filtre
$encodedFilter = [uri]::EscapeDataString($filter)

# Construire l'URL complète
$url = "$baseUrl?filter=$encodedFilter&startIndex=1&count=50"

Write-Host "URL complète: $url"
# Résultat: https://localhost:7001/scim/Users?filter=active%20eq%20true&startIndex=1&count=50
```

### cURL avec Encoding

```bash
# Option 1: Encoder manuellement
FILTER="active%20eq%20true"
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=$FILTER"

# Option 2: Laisser cURL encoder
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users" \
  --data-urlencode "filter=active eq true"
```

---

## Pièges Communs

### ❌ Espace Non Encodé

```
❌ FAUX: /scim/Users?filter=active eq true
✅ BON:  /scim/Users?filter=active%20eq%20true
```

### ❌ Guillemets Non Encodés

```
❌ FAUX: filter=userName eq "john"
✅ BON:  filter=userName%20eq%20%22john%22
```

### ❌ Parenthèses Non Encodées

```
❌ FAUX: filter=active eq true and (title eq "Admin")
✅ BON:  filter=active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%29
```

### ❌ Oublier le @ dans Email

```
❌ FAUX: filter=emails.value ew "@company.com"
✅ BON:  filter=emails.value%20ew%20%22%40company.com%22
```

---

## Vérification

Vous pouvez vérifier votre encoding sur https://www.urldecoder.org/

Copiez votre URL encodée et décoder-la:
```
Input:  active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%29
Output: active eq true and (title eq "Admin")
```

Si le résultat est correct, votre encoding est bon!

---

## Résumé Quick

```powershell
# Le plus simple en PowerShell:
$filter = "active eq true"
$encoded = [uri]::EscapeDataString($filter)
$url = "https://localhost:7001/scim/Users?filter=$encoded"
```

C'est tout ce que vous devez savoir! Les outils gèrent l'encoding automatiquement.
