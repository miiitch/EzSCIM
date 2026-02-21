✅ NESTED FILTERS & URL ENCODING - COMPLETE

## Réponse à Ta Question

**Q: Peut-il y avoir des AND et OR imbriqués?**

**A: OUI! Complètement supporté!**

Les filtres SCIM supportent:
- ✅ Parenthèses pour imbrication
- ✅ Multiples niveaux d'imbrication
- ✅ Combinaison d'AND et OR
- ✅ Opérateur NOT

---

## 2 Nouveaux Fichiers Créés

### 1. NESTED-FILTERS-DOCUMENTATION.md (1000+ lignes)
Couverture complète:
- ✅ Syntaxe des filtres imbriqués
- ✅ Règles de priorité (precedence)
- ✅ 20+ exemples pour Users
- ✅ 10+ exemples pour Groups
- ✅ Exemples PowerShell, cURL, C#
- ✅ Code d'implémentation
- ✅ Tableau de référence

### 2. URL-ENCODING-GUIDE.md (500+ lignes)
Encodage pour URL:
- ✅ Tableau des caractères à encoder
- ✅ Exemples d'encoding
- ✅ Outils pour encoder
- ✅ PowerShell examples
- ✅ Pièges communs
- ✅ Vérification

---

## Exemples Rapides - Filtres Imbriqués

### Simple AND
```
filter=active eq true and userName sw "admin"
```

### AND avec OR (imbriqué)
```
filter=active eq true and (title eq "Admin" or title eq "Manager")
```

### Très imbriqué
```
filter=((active eq true and (title eq "Admin" or title eq "Manager")) 
  or (active eq false and title eq "Director")) 
  and departmentName eq "Engineering"
```

### Avec NOT
```
filter=active eq true and not (userName sw "admin")
```

### Groups imbriqués
```
filter=(displayName sw "Team" or displayName sw "Department") 
  and (displayName co "Engineering" or displayName co "Architecture")
```

---

## URL Encoding Rapide

### Sans Encoding
```
active eq true and (title eq "Admin")
```

### Avec Encoding
```
active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%29
```

### PowerShell Automatique
```powershell
$filter = 'active eq true and (title eq "Admin")'
$encoded = [uri]::EscapeDataString($filter)
# Résultat automatique: active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%29
```

---

## Opérateurs Logiques

| Opérateur | Priorité | Exemple |
|-----------|----------|---------|
| `not` | 1 (premier) | `not (active eq false)` |
| `and` | 2 | `active eq true and title eq "Admin"` |
| `or` | 3 (dernier) | `title eq "Admin" or title eq "Manager"` |
| `()` | Force l'ordre | `(cond1 and cond2) or cond3` |

---

## Règles de Priorité

Sans parenthèses:
```
# Ceci:
active eq true and title eq "Admin" or title eq "Manager"

# Est évalué comme:
(active eq true and title eq "Admin") or (title eq "Manager")

# PAS comme:
active eq true and (title eq "Admin" or title eq "Manager")
```

Pour être sûr: **utilisez des parenthèses!**

---

## PowerShell Examples

```powershell
$token = "YOUR_TOKEN"
$baseUrl = "https://localhost:7001"

# Exemple 1: Simples AND/OR
$filter = 'active eq true and (title eq "Admin" or title eq "Manager")'
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $uri -Headers $headers

# Exemple 2: NOT
$filter = 'active eq true and not (userName sw "admin")'
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $uri -Headers $headers

# Exemple 3: Très imbriqué
$filter = '((active eq true and (title eq "Admin" or title eq "Manager")) or (active eq false and title eq "Director")) and departmentName eq "Engineering"'
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $uri -Headers $headers
```

---

## cURL Examples

```bash
# Simple AND/OR imbriqué
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22%29"

# Avec NOT
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20true%20and%20not%20%28userName%20sw%20%22admin%22%29"

# Très imbriqué
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=%28%28active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22%29%29%20or%20%28active%20eq%20false%20and%20title%20eq%20%22Director%22%29%29%20and%20departmentName%20eq%20%22Engineering%22"
```

---

## Niveaux d'Imbrication

| Niveau | Exemple | Complexité |
|--------|---------|-----------|
| 0 | `active eq true` | Très simple |
| 1 | `active eq true and title eq "Admin"` | Simple |
| 2 | `active eq true and (title eq "Admin" or title eq "Manager")` | Moyen |
| 3 | `((cond1 and cond2) or cond3) and cond4` | Complexe |
| 4+ | Multiple niveaux profonds | Très complexe |

---

## Tableau Encodage

| Caractère | Encodé | Utilisation |
|-----------|--------|-------------|
| Espace | `%20` | Entre opérateurs |
| `(` | `%28` | Parenthèse ouvrante |
| `)` | `%29` | Parenthèse fermante |
| `"` | `%22` | Guillemets |
| `@` | `%40` | Email |
| `-` | `%2D` | Tiret |
| `.` | `%2E` | Point |

---

## Bonnes Pratiques

✅ **À FAIRE:**
```
# Parenthèses claires
(active eq true) and (title eq "Admin" or title eq "Manager")

# Indentation logique
((condition1 and condition2) 
  or (condition3 and condition4))
```

❌ **À NE PAS FAIRE:**
```
# Confus, dépend de priorités implicites
active eq true and title eq "Admin" or title eq "Manager"

# Parenthèses inutiles
(((active eq true)))
```

---

## Résumé Complet

| Aspect | Détail |
|--------|--------|
| **AND/OR imbriqués** | ✅ Oui, complètement supporté |
| **Profondeur** | ✅ Illimitée (pratiquement) |
| **Parenthèses** | ✅ Optionnelles mais recommandées |
| **Caractères à encoder** | ✅ Spaces, parenthèses, guillemets, @ |
| **Outils** | ✅ PowerShell [uri]::EscapeDataString() |
| **RFC Spec** | ✅ RFC 7644 compliant |

---

## Documentation Fournie

### 📁 Fichiers Créés (2)

1. **NESTED-FILTERS-DOCUMENTATION.md** (1000+ lines)
   - ✅ Syntaxe complète
   - ✅ 30+ exemples
   - ✅ Implémentation
   - ✅ Priorité des opérateurs

2. **URL-ENCODING-GUIDE.md** (500+ lines)
   - ✅ Tableau complet
   - ✅ Outils et exemples
   - ✅ PowerShell, cURL, Python
   - ✅ Pièges communs

---

## Fichiers Totaux - Filtres

```
Documentation Filtres (Complète):
├── SCIM-FILTER-DOCUMENTATION.md (1000+ lines)
├── FILTER-IMPLEMENTATION-GUIDE.md (500+ lines)
├── FILTER-QUICK-EXAMPLES.md (400+ lines)
├── NESTED-FILTERS-DOCUMENTATION.md (1000+ lines) ← NOUVEAU
├── URL-ENCODING-GUIDE.md (500+ lines) ← NOUVEAU
└── FILTER-DOCUMENTATION-SUMMARY.md (200+ lines)

Total: 6 fichiers, 3600+ lignes!
```

---

## Status

🟢 **COMPLETE**

- ✅ Filtres simples documentés
- ✅ Filtres imbriqués documentés
- ✅ URL encoding documenté
- ✅ 100+ exemples fournis
- ✅ Prêt pour utilisation immédiate

---

## Quick Answer

**Oui, tu peux avoir des AND et OR imbriqués:**

```
filter=active eq true and (title eq "Admin" or title eq "Manager")
filter=((cond1 and cond2) or (cond3 and cond4)) and cond5
```

Voir **NESTED-FILTERS-DOCUMENTATION.md** pour tous les détails!
