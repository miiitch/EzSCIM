# 🎯 Mapping Repository → SCIM - Guide rapide

## Qu'est-ce que c'est ?

Un système complet permettant de connecter **votre repository utilisateur existant** à SCIM avec **filtrage server-side automatique**.

```
Votre base SQL → IQueryable<TUser> → Filtre SCIM → SQL optimisé → Résultats SCIM
```

---

## ⚡ Démarrage rapide (3 étapes)

### 1. Annotez votre modèle

```csharp
public class MyUser
{
    [ScimProperty("userName", "string", Required = true)]
    public string Email { get; set; } = string.Empty;

    [ScimProperty("active", "boolean")]
    public bool IsActive { get; set; } = true;
}
```

### 2. Implémentez IUserDataRepository

```csharp
public class MyUserRepo : IUserDataRepository<MyUser>
{
    public IQueryable<MyUser> Query() => _context.Users;
    // + 4 autres méthodes (Get/Create/Update/Delete)
}
```

### 3. Configurez DI

```csharp
services.AddScoped<IUserDataRepository<MyUser>, MyUserRepo>();
services.AddScoped<IScimFilterTranslator<MyUser>, GenericScimFilterTranslator<MyUser>>();
services.AddScoped<IScimUserRepository<ScimUser>>(sp => 
    new ScimUserRepositoryAdapter<MyUser>(
        sp.GetRequiredService<IUserDataRepository<MyUser>>(),
        sp.GetRequiredService<IScimFilterTranslator<MyUser>>()));
```

**C'est tout !** 🎉

---

## 🚀 Exemple d'utilisation

```http
GET /scim/Users?filter=active eq true and userName sw "john"
```

**Traduit automatiquement en :**
```csharp
context.Users.Where(u => u.IsActive == true && u.Email.StartsWith("john"))
```

**Exécuté en SQL :**
```sql
SELECT * FROM Users WHERE IsActive = 1 AND Email LIKE 'john%'
```

---

## 📦 Composants fournis

| Composant | Fichier | Rôle |
|-----------|---------|------|
| Interface repository | `IUserDataRepository.cs` | Contrat pour votre source de données |
| Traducteur générique | `GenericScimFilterTranslator.cs` | AST → IQueryable (via attributs) |
| Traducteur ScimUser | `ScimUserFilterTranslator.cs` | AST → IQueryable (ScimUser direct) |
| Adaptateur | `ScimUserRepositoryAdapter.cs` | Pont Repository ↔ SCIM |
| Exemple | `CustomUser.cs`, `CustomUserRepository.cs` | Implémentation de référence |

---

## ✅ Tests

```
✅ 26/26 tests passed (100%)
   - ScimUserFilterTranslator: 13/13
   - GenericScimFilterTranslator: 13/13
```

**Opérateurs testés :**
- Comparaison: `eq`, `ne`, `co`, `sw`, `ew`, `gt`, `lt`
- Logique: `and`, `or`, `not`
- Présence: `pr`
- Propriétés imbriquées: `name.givenName`

---

## 📚 Documentation complète

| Document | Description |
|----------|-------------|
| **QUICK-START-REPOSITORY-INTEGRATION.md** | Guide rapide 15 min |
| **REPOSITORY-ADAPTER-GUIDE.md** | Guide complet avec exemples |
| **REPOSITORY-MAPPING-IMPLEMENTATION-COMPLETE.md** | Détails d'implémentation |

---

## 🎯 Avantages

✅ **Performance** : Filtrage SQL server-side, pas de chargement mémoire  
✅ **Simplicité** : 3 étapes, ~15 minutes d'intégration  
✅ **Flexibilité** : Fonctionne avec n'importe quel modèle annoté  
✅ **Type-safe** : Mapping par attributs, erreurs à la compilation  
✅ **Compatible** : EF Core, Dapper, SQL direct, etc.

---

## 🔧 Architecture

```
┌─────────────────┐
│  SCIM Client    │
└────────┬────────┘
         │ GET /scim/Users?filter=...
         v
┌─────────────────────────────┐
│   UsersController            │
│   Parse → FilterExpression   │
└────────┬────────────────────┘
         │
         v
┌──────────────────────────────────┐
│ ScimUserRepositoryAdapter<TUser> │
└────────┬─────────────────────────┘
         │
    ┌────┴────┐
    v         v
┌────────┐  ┌──────────────────────┐
│ Repo   │  │ FilterTranslator     │
│ Query()│  │ AST → Expression     │
└───┬────┘  └──────────┬───────────┘
    │                  │
    └────────┬─────────┘
             │ IQueryable.Where(predicate)
             v
    ┌────────────────┐
    │   Database     │
    │   (SQL)        │
    └────────────────┘
```

---

## 💡 Cas d'usage

- ✅ Intégration Azure AD / Entra ID
- ✅ Provisioning Okta
- ✅ Synchronisation multi-systèmes
- ✅ API SCIM sur base existante

---

## 🚀 Commencer

**Lire en premier :** [QUICK-START-REPOSITORY-INTEGRATION.md](QUICK-START-REPOSITORY-INTEGRATION.md)

**Approfondir :** [REPOSITORY-ADAPTER-GUIDE.md](REPOSITORY-ADAPTER-GUIDE.md)

**Détails techniques :** [REPOSITORY-MAPPING-IMPLEMENTATION-COMPLETE.md](REPOSITORY-MAPPING-IMPLEMENTATION-COMPLETE.md)

