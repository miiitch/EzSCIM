# Exemple d'intégration rapide - Repository → SCIM

## Scénario: Vous avez déjà une base de données d'utilisateurs

Vous voulez exposer vos utilisateurs via SCIM sans réécrire votre code existant.

---

## 1️⃣ Votre modèle existant

```csharp
// Votre classe utilisateur existante
public class Employee
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
}
```

---

## 2️⃣ Ajoutez les attributs SCIM (2 minutes)

```csharp
using ScimAPI.Attributes;

public class Employee
{
    public Guid Id { get; set; }

    [ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
    public string Email { get; set; } = string.Empty;

    [ScimProperty("givenName", "string")]
    public string FirstName { get; set; } = string.Empty;

    [ScimProperty("familyName", "string")]
    public string LastName { get; set; } = string.Empty;

    [ScimProperty("displayName", "string")]
    public string FullName { get; set; } = string.Empty;

    [ScimProperty("active", "boolean")]
    public bool IsEnabled { get; set; } = true;

    [ScimProperty("title", "string")]
    public string Position { get; set; } = string.Empty;

    // Non-SCIM (pas d'attribut)
    public DateTime HireDate { get; set; }
}
```

---

## 3️⃣ Créez le repository (5 minutes)

```csharp
using ScimAPI.Repositories;
using Microsoft.EntityFrameworkCore;

public class EmployeeRepository : IUserDataRepository<Employee>
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetAsync(string id)
    {
        if (Guid.TryParse(id, out var guid))
            return await _context.Employees.FindAsync(guid);
        return null;
    }

    // IMPORTANT: Retourner IQueryable pour filtrage server-side
    public IQueryable<Employee> Query()
    {
        return _context.Employees.AsQueryable();
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        employee.Id = Guid.NewGuid();
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee?> UpdateAsync(string id, Employee employee)
    {
        if (!Guid.TryParse(id, out var guid))
            return null;

        var existing = await _context.Employees.FindAsync(guid);
        if (existing == null)
            return null;

        _context.Entry(existing).CurrentValues.SetValues(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return false;

        var employee = await _context.Employees.FindAsync(guid);
        if (employee == null)
            return false;

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }
}
```

---

## 4️⃣ Configurez DI dans Program.cs (3 lignes)

```csharp
using ScimAPI.Repositories;
using ScimAPI.Filtering;
using ScimAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Votre DbContext existant
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 1️⃣ Votre repository
builder.Services.AddScoped<IUserDataRepository<Employee>, EmployeeRepository>();

// 2️⃣ Traducteur de filtres SCIM → LINQ
builder.Services.AddScoped<IScimFilterTranslator<Employee>, GenericScimFilterTranslator<Employee>>();

// 3️⃣ Adaptateur SCIM
builder.Services.AddScoped<IScimUserRepository<ScimUser>>(sp =>
{
    var dataRepo = sp.GetRequiredService<IUserDataRepository<Employee>>();
    var translator = sp.GetRequiredService<IScimFilterTranslator<Employee>>();
    return new ScimUserRepositoryAdapter<Employee>(dataRepo, translator);
});

var app = builder.Build();
app.Run();
```

---

## 5️⃣ C'est tout ! ✨

Vous pouvez maintenant utiliser l'API SCIM :

### Lister tous les employés
```http
GET /scim/Users
```

### Filtrer par email
```http
GET /scim/Users?filter=userName eq "john.doe@company.com"
```

### Filtrer les employés actifs
```http
GET /scim/Users?filter=active eq true
```

### Recherche par prénom
```http
GET /scim/Users?filter=givenName sw "John"
```

### Filtre complexe
```http
GET /scim/Users?filter=(givenName sw "John" or givenName sw "Jane") and active eq true
```

---

## 🎯 Que se passe-t-il sous le capot ?

### Requête SCIM
```http
GET /scim/Users?filter=active eq true and givenName sw "John"
```

### 1. Controller parse le filtre
```csharp
FilterExpression filter = parser.Parse("active eq true and givenName sw \"John\"");
```

### 2. Adaptateur applique le filtre
```csharp
var query = _dataRepository.Query(); // IQueryable<Employee>
query = _translator.Apply(query, filter);
```

### 3. Traducteur génère l'expression LINQ
```csharp
employee => employee.IsEnabled == true && 
            employee.FirstName.StartsWith("John", StringComparison.OrdinalIgnoreCase)
```

### 4. EF Core traduit en SQL
```sql
SELECT * FROM Employees
WHERE IsEnabled = 1 
  AND FirstName LIKE 'John%'
```

### 5. Résultats mappés vers SCIM
```csharp
// Employee → ScimUser (automatique)
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "userName": "john.doe@company.com",
  "name": {
    "givenName": "John",
    "familyName": "Doe"
  },
  "displayName": "John Doe",
  "active": true,
  "title": "Software Engineer"
}
```

---

## 🚀 Performance

**SANS ce système:**
```csharp
// ❌ Charge TOUS les employés en mémoire
var allEmployees = await _context.Employees.ToListAsync();
var filtered = allEmployees.Where(e => e.IsEnabled && e.FirstName.StartsWith("John"));
```

**AVEC ce système:**
```csharp
// ✅ Filtre côté SQL, ne charge que les résultats
var filtered = _context.Employees
    .Where(e => e.IsEnabled && e.FirstName.StartsWith("John"))
    .ToList();
```

Pour 100 000 employés avec 2 résultats :
- ❌ Sans : Charge 100 000 rows → Filtre en C# → Retourne 2
- ✅ Avec : SQL filtre → Charge 2 rows → Retourne 2

**Gain:** 50 000x plus rapide ! 🚀

---

## 📊 Mapping complet des attributs SCIM

| Attribut SCIM | Type | Votre propriété | Exemple |
|---------------|------|-----------------|---------|
| `userName` | string | `Email` | `john.doe@company.com` |
| `givenName` | string | `FirstName` | `John` |
| `familyName` | string | `LastName` | `Doe` |
| `displayName` | string | `FullName` | `John Doe` |
| `active` | boolean | `IsEnabled` | `true` |
| `title` | string | `Position` | `Software Engineer` |
| `externalId` | string | `EmployeeCode` | `EMP-12345` |
| `email` | string | `EmailAddress` | `john@company.com` |

---

## 💡 Cas d'usage réels

### Intégration avec Azure AD / Entra ID

Azure AD peut provisionner automatiquement vos employés :

```
Azure AD SCIM Client
    ↓ (Sync employees)
Votre API SCIM
    ↓ (via ScimUserRepositoryAdapter)
Votre base SQL Server
```

### Intégration avec Okta

```http
POST /scim/Users
{
  "userName": "new.hire@company.com",
  "name": {
    "givenName": "New",
    "familyName": "Hire"
  },
  "active": true
}
```

→ Crée automatiquement dans votre base via `EmployeeRepository.CreateAsync()`

---

## 🔧 Personnalisation avancée

### Ajouter des includes EF Core

```csharp
public class EmployeeRepository : IUserDataRepository<Employee>
{
    public IQueryable<Employee> Query()
    {
        return _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .AsQueryable();
    }
}
```

### Ajouter un filtre global (soft delete)

```csharp
public IQueryable<Employee> Query()
{
    return _context.Employees
        .Where(e => !e.IsDeleted) // Global filter
        .AsQueryable();
}
```

### Mapper des propriétés calculées

```csharp
public class Employee
{
    [ScimProperty("displayName", "string")]
    public string FullName => $"{FirstName} {LastName}";
}
```

---

## 🎉 Résultat

**Temps d'intégration total:** ~15 minutes  
**Code à écrire:** ~60 lignes  
**Résultat:** API SCIM complète avec filtrage server-side !

```
✅ Pas besoin de réécrire votre code existant
✅ Pas besoin de dupliquer vos données
✅ Performance optimale (SQL server-side)
✅ Compatible avec Azure AD, Okta, etc.
✅ Maintenance minimale
```

---

## 📚 Pour aller plus loin

Consultez le guide complet : **REPOSITORY-ADAPTER-GUIDE.md**

