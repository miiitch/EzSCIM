## 🔗 SCIM Nested Filters - Logical Operators Imbriqués

Les filtres SCIM supportent complètement les opérateurs logiques imbriqués avec des parenthèses.

---

## Syntaxe des Filtres Imbriqués

### Format Simple
```
condition1 and condition2
```

### Format Imbriqué
```
(condition1 and condition2) or (condition3 and condition4)
```

### Format Très Imbriqué
```
((condition1 and condition2) or condition3) and (condition4 or condition5)
```

---

## Opérateurs Logiques

| Opérateur | Utilisation | Priorité |
|-----------|-------------|----------|
| `and` | ET logique | Plus haute (évalué d'abord) |
| `or` | OU logique | Plus basse (évalué après) |
| `not` | Négation | Plus haute (évalué d'abord) |
| `()` | Parenthèses | Force l'ordre d'évaluation |

---

## Exemples de Filtres Imbriqués - Users

### Exemple 1: AND simple vs imbriqué

```bash
# Simple AND
filter=active eq true and userName sw "admin"

# Équivalent avec parenthèses (clarté)
filter=(active eq true) and (userName sw "admin")
```

### Exemple 2: AND + OR simples

```bash
# Utilisateurs actifs qui sont admins OU managers
filter=active eq true and (title eq "Admin" or title eq "Manager")

# URL encodé
filter=active%20eq%20true%20and%20(title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22)
```

### Exemple 3: Deux groupes de conditions avec AND

```bash
# (Active ET (Admin OU Manager)) ET (Email de la company)
filter=(active eq true and (title eq "Admin" or title eq "Manager")) and emails.value ew "@company.com"

# URL encodé
filter=(active%20eq%20true%20and%20(title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22))%20and%20emails.value%20ew%20%22%40company.com%22
```

### Exemple 4: Imbrication profonde

```bash
# Utilisateurs: ((Active ET (Admin OU Manager)) OU (Inactive ET Director)) ET Department=Engineering
filter=((active eq true and (title eq "Admin" or title eq "Manager")) or (active eq false and title eq "Director")) and departmentName eq "Engineering"

# URL encodé
filter=((active%20eq%20true%20and%20(title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22))%20or%20(active%20eq%20false%20and%20title%20eq%20%22Director%22))%20and%20departmentName%20eq%20%22Engineering%22
```

### Exemple 5: Utiliser NOT avec imbrication

```bash
# Utilisateurs actifs qui ne sont PAS admins
filter=active eq true and not (userName sw "admin")

# URL encodé
filter=active%20eq%20true%20and%20not%20(userName%20sw%20%22admin%22)
```

### Exemple 6: NOT imbriqué complexe

```bash
# Utilisateurs: Active ET (NOT (Admin OU Manager))
filter=active eq true and not (title eq "Admin" or title eq "Manager")

# URL encodé
filter=active%20eq%20true%20and%20not%20(title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22)
```

### Exemple 7: Plusieurs domaines OR

```bash
# Utilisateurs avec email de l'une de ces companies
filter=emails.value ew "@acme.com" or emails.value ew "@bigcorp.com" or emails.value ew "@startup.io"

# URL encodé
filter=emails.value%20ew%20%22%40acme.com%22%20or%20emails.value%20ew%20%22%40bigcorp.com%22%20or%20emails.value%20ew%20%22%40startup.io%22
```

### Exemple 8: Combinaison complexe

```bash
# ((Email Company OR Email Domain) AND (Active OR Manager)) AND (NOT Admin)
filter=((emails.value ew "@company.com" or emails.value co "@company-") and (active eq true or title eq "Manager")) and not (userName sw "admin")

# URL encodé
filter=((emails.value%20ew%20%22%40company.com%22%20or%20emails.value%20co%20%22%40company-%22)%20and%20(active%20eq%20true%20or%20title%20eq%20%22Manager%22))%20and%20not%20(userName%20sw%20%22admin%22)
```

---

## Exemples de Filtres Imbriqués - Groups

### Exemple 1: Groupes engineering OU infrastructure

```bash
# Groupes dont le nom contient "Engineering" OU "Infrastructure"
filter=displayName co "Engineering" or displayName co "Infrastructure"

# URL encodé
filter=displayName%20co%20%22Engineering%22%20or%20displayName%20co%20%22Infrastructure%22
```

### Exemple 2: Groupes team avec imbrication

```bash
# (Team OR Department) ET (Engineering OU Architecture)
filter=(displayName sw "Team" or displayName sw "Department") and (displayName co "Engineering" or displayName co "Architecture")

# URL encodé
filter=(displayName%20sw%20%22Team%22%20or%20displayName%20sw%20%22Department%22)%20and%20(displayName%20co%20%22Engineering%22%20or%20displayName%20co%20%22Architecture%22)
```

### Exemple 3: Groupes NOT admin

```bash
# Tous les groupes sauf ceux qui commencent par "Admin"
filter=not (displayName sw "Admin")

# URL encodé
filter=not%20(displayName%20sw%20%22Admin%22)
```

### Exemple 4: Groupes imbriqués complexes

```bash
# ((Team ET Engineering) OU (Department ET Architecture)) ET (NOT Technical)
filter=((displayName sw "Team" and displayName co "Engineering") or (displayName sw "Department" and displayName co "Architecture")) and not (displayName co "Technical")

# URL encodé
filter=((displayName%20sw%20%22Team%22%20and%20displayName%20co%20%22Engineering%22)%20or%20(displayName%20sw%20%22Department%22%20and%20displayName%20co%20%22Architecture%22))%20and%20not%20(displayName%20co%20%22Technical%22)
```

---

## Règles de Priorité (Precedence)

Sans parenthèses, l'ordre de évaluation est:

1. `not` (Priorité 1 - Évalué en premier)
2. `and` (Priorité 2)
3. `or` (Priorité 3 - Évalué en dernier)

### Exemples d'Ordre de Évaluation

```
# Cas 1: Sans parenthèses
active eq true and title eq "Admin" or title eq "Manager"

# Est évalué comme:
(active eq true and title eq "Admin") or (title eq "Manager")

# Cas 2: Avec parenthèses explicites
active eq true and (title eq "Admin" or title eq "Manager")

# Est évalué comme:
(active eq true) and (title eq "Admin" or title eq "Manager")
```

---

## Exemples PowerShell - Filtres Imbriqués

```powershell
$token = "YOUR_TOKEN"
$baseUrl = "https://localhost:7001"
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/scim+json"
}

# Exemple 1: Utilisateurs actifs qui sont admins OU managers
$filter = 'active eq true and (title eq "Admin" or title eq "Manager")'
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $uri -Headers $headers -Method Get
Write-Host "Found $($response.totalResults) active admins/managers"

# Exemple 2: Utilisateurs actifs avec email company, sauf admins
$filter = 'active eq true and emails.value ew "@company.com" and not (userName sw "admin")'
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $uri -Headers $headers -Method Get
Write-Host "Found $($response.totalResults) active non-admin company employees"

# Exemple 3: Imbrication complexe
$filter = '((active eq true and (title eq "Admin" or title eq "Manager")) or (active eq false and title eq "Director")) and departmentName eq "Engineering"'
$uri = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $uri -Headers $headers -Method Get
Write-Host "Found $($response.totalResults) matching complex filter"

# Exemple 4: Groupes avec imbrication
$filter = '(displayName sw "Team" or displayName sw "Department") and (displayName co "Engineering" or displayName co "Architecture")'
$uri = "$baseUrl/scim/Groups?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $uri -Headers $headers -Method Get
Write-Host "Found $($response.totalResults) engineering/architecture groups"
```

---

## Exemples cURL - Filtres Imbriqués

```bash
# Exemple 1: Active users who are admin or manager
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20true%20and%20(title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22)"

# Exemple 2: Active users from company excluding admins
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20true%20and%20emails.value%20ew%20%22%40company.com%22%20and%20not%20(userName%20sw%20%22admin%22)"

# Exemple 3: Complex nested filter
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=((active%20eq%20true%20and%20(title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22))%20or%20(active%20eq%20false%20and%20title%20eq%20%22Director%22))%20and%20departmentName%20eq%20%22Engineering%22"

# Exemple 4: Groups with nested conditions
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Groups?filter=(displayName%20sw%20%22Team%22%20or%20displayName%20sw%20%22Department%22)%20and%20(displayName%20co%20%22Engineering%22%20or%20displayName%20co%20%22Architecture%22)"
```

---

## Exemples C# - Filtres Imbriqués

```csharp
var repository = new InMemoryScimRepository(...);

// Exemple 1: Active users who are admin or manager
var filter = "active eq true and (title eq \"Admin\" or title eq \"Manager\")";
var result = await repository.GetUsersAsync(filter);
Console.WriteLine($"Found {result.TotalResults} active admins/managers");

// Exemple 2: Active users from company, excluding admins
var filter = "active eq true and emails.value ew \"@company.com\" and not (userName sw \"admin\")";
var result = await repository.GetUsersAsync(filter);
Console.WriteLine($"Found {result.TotalResults} non-admin company users");

// Exemple 3: Complex nested condition
var filter = "((active eq true and (title eq \"Admin\" or title eq \"Manager\")) or (active eq false and title eq \"Director\")) and departmentName eq \"Engineering\"";
var result = await repository.GetUsersAsync(filter);
Console.WriteLine($"Found {result.TotalResults} matching complex filter");

// Exemple 4: Groups with multiple nested conditions
var filter = "(displayName sw \"Team\" or displayName sw \"Department\") and (displayName co \"Engineering\" or displayName co \"Architecture\")";
var result = await repository.GetGroupsAsync(filter);
Console.WriteLine($"Found {result.TotalResults} engineering/architecture groups");
```

---

## Implémentation - Gestion des Filtres Imbriqués

Pour supporter les filtres imbriqués, vous devez:

### Option 1: Parsing Simple (Actuel)
```csharp
// Parsing simple des parenthèses
public List<ScimUser> ApplyNestedFilter(List<ScimUser> users, string filter)
{
    if (string.IsNullOrEmpty(filter))
        return users;

    // Gérer les parenthèses de base
    if (filter.Contains("(") && filter.Contains(")"))
    {
        // Extraire ce qui est entre parenthèses
        var insideParens = ExtractInsideParentheses(filter);
        var outside = RemoveParentheses(filter);
        
        // Appliquer récursivement
        var insideResult = ApplyNestedFilter(users, insideParens);
        return ApplyNestedFilter(insideResult, outside);
    }

    // Appliquer les conditions simples
    return ApplySimpleConditions(users, filter);
}

private string ExtractInsideParentheses(string filter)
{
    int start = filter.IndexOf("(") + 1;
    int end = filter.LastIndexOf(")");
    return filter.Substring(start, end - start);
}
```

### Option 2: Parsing Avancé (Recommandé)
```csharp
// Parser qui gère and/or/not et parenthèses
public List<ScimUser> ApplyAdvancedFilter(List<ScimUser> users, string filter)
{
    // 1. Tokenizer le filtre
    var tokens = TokenizeFilter(filter);
    
    // 2. Parser en arbre d'expression
    var expressionTree = ParseFilterExpression(tokens);
    
    // 3. Évaluer l'arbre pour chaque utilisateur
    return users.Where(u => EvaluateExpression(expressionTree, u)).ToList();
}

private List<string> TokenizeFilter(string filter)
{
    // Diviser en tokens: conditions, opérateurs, parenthèses
    var tokens = new List<string>();
    var current = "";
    
    foreach (char c in filter)
    {
        if (c == '(' || c == ')')
        {
            if (!string.IsNullOrWhiteSpace(current))
                tokens.Add(current.Trim());
            tokens.Add(c.ToString());
            current = "";
        }
        else
        {
            current += c;
        }
    }
    
    if (!string.IsNullOrWhiteSpace(current))
        tokens.Add(current.Trim());
    
    return tokens;
}
```

---

## Table de Réference - Imbrication

| Niveau | Exemple | Complexité |
|--------|---------|-----------|
| **0** | `active eq true` | Simple |
| **1** | `active eq true and title eq "Admin"` | Simple AND |
| **2** | `active eq true and (title eq "Admin" or title eq "Manager")` | AND + OR |
| **3** | `((active eq true and title eq "Admin") or active eq false) and department eq "Sales"` | Imbrication profonde |
| **4+** | Multiple niveaux de parenthèses | Très complexe |

---

## Bonnes Pratiques

### ✅ À FAIRE

```
# Clair avec parenthèses
(active eq true) and (title eq "Admin" or title eq "Manager")

# Ordre logique
((condition1 and condition2) or (condition3 and condition4))
```

### ❌ À NE PAS FAIRE

```
# Confus sans parenthèses
active eq true and title eq "Admin" or title eq "Manager"

# Parenthèses inutiles
(((active eq true)))
```

---

## Notes Importantes

✅ **Les parenthèses sont optionnelles** mais recommandées pour la clarté  
✅ **L'ordre de priorité** est: `not` > `and` > `or`  
✅ **URL Encoding** requis pour les parenthèses: `(` = `%28`, `)` = `%29`  
✅ **Récursivité**: Les filtres imbriqués peuvent être appliqués récursivement  
✅ **Performance**: Moins de parenthèses = plus rapide (moins de parsing)  

---

## Références SCIM

- RFC 7644 Section 3.4.2: https://tools.ietf.org/html/rfc7644#section-3.4.2
- Filter expressions: https://tools.ietf.org/html/rfc7644#section-3.4.2.2
- Logical operators: https://tools.ietf.org/html/rfc7644#section-3.4.2.2.1
