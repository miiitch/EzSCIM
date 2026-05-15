# Schema Extensions

EzSCIM uses a declarative, opt-in attribute system. You annotate your C# classes with
`[ScimProperty]` to control exactly which fields are exposed via SCIM and how they are
described in the auto-generated `/scim/Schemas` endpoint.

---

## Annotations

### `[ScimResource]` — class level

Defines the SCIM schema URN and metadata for a resource class.

```csharp
using EzSCIM.Attributes;

[ScimResource(
    "urn:ietf:params:scim:schemas:core:2.0:User",
    "User",
    "User Account"
)]
public class ScimUser { ... }
```

| Parameter | Description |
|---|---|
| `Schema` | SCIM schema URN (required) |
| `Name` | Resource name (required) |
| `Description` | Resource description (required) |

### `[ScimProperty]` — property level

Marks a property as part of the SCIM schema. **Only annotated properties
are included in generated schemas and mapped by filter translators.**

```csharp
[ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
public string UserName { get; set; }
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Name` | string | — | SCIM attribute name (required) |
| `Type` | string | — | SCIM data type (required) — see below |
| `Required` | bool | `false` | Attribute is required |
| `MultiValued` | bool | `false` | Attribute allows multiple values |
| `Description` | string | `""` | Attribute description |
| `Uniqueness` | string | `"none"` | `none`, `server`, `global` |
| `Mutability` | string | `"readWrite"` | `readOnly`, `readWrite`, `immutable`, `writeOnly` |
| `Returned` | string | `"default"` | `always`, `never`, `default`, `request` |
| `CaseExact` | bool | `false` | Case-sensitive value comparison |

!!! info "SCIM data types"
    `string`, `boolean`, `complex`, `reference`, `dateTime`, `integer`, `decimal`, `binary`

---

## Extend ScimUser with custom attributes

Inherit `ScimUser` and annotate custom properties:

```csharp
using EzSCIM.Attributes;
using EzSCIM.Models;

[ScimResource(
    "urn:enterprise:params:scim:schemas:extension:Acme:2.0",
    "AcmeUser",
    "Acme Corporation user with enterprise attributes"
)]
public class AcmeUser : ScimUser
{
    [ScimProperty("employeeNumber", "string",
        Required = true,
        Uniqueness = "server",
        Description = "Unique employee ID")]
    public string EmployeeNumber { get; set; } = string.Empty;

    [ScimProperty("department", "string",
        Description = "Department code")]
    public string? Department { get; set; }

    [ScimProperty("hireDate", "dateTime",
        Description = "Date of hire")]
    public DateTime? HireDate { get; set; }

    [ScimProperty("costCenters", "string",
        MultiValued = true,
        Description = "Assigned cost centers")]
    public List<string> CostCenters { get; set; } = new();
}
```

!!! tip "Inheritance"
    All `[ScimProperty]` annotations from `ScimUser` are automatically inherited.
    The generated schema contains both base and extension attributes.

---

## Extend ScimGroup

```csharp
[ScimResource(
    "urn:enterprise:params:scim:schemas:extension:Acme:2.0:Group",
    "AcmeGroup",
    "Acme Corporation group"
)]
public class AcmeGroup : ScimGroup
{
    [ScimProperty("description", "string",
        Description = "Group description")]
    public string? Description { get; set; }

    [ScimProperty("ownerEmail", "string",
        Description = "Group owner email")]
    public string? OwnerEmail { get; set; }
}
```

---

## Complex nested types

??? example "Complex attribute with sub-attributes"

    Nested complex attributes (sub-objects) also use `[ScimProperty]`:

    ```csharp
    [ScimProperty("badge", "complex", Description = "Badge information")]
    public BadgeInfo? Badge { get; set; }

    public class BadgeInfo
    {
        [ScimProperty("badgeNumber", "string", Description = "Badge ID")]
        public string? BadgeNumber { get; set; }

        [ScimProperty("accessLevel", "integer", Description = "Access level 1-5")]
        public int AccessLevel { get; set; }

        [ScimProperty("expiryDate", "dateTime", Description = "Expiry date")]
        public DateTime? ExpiryDate { get; set; }
    }
    ```

---

## URN naming convention

!!! tip "Use the correct URN format for custom schemas"
    ```
    urn:enterprise:params:scim:schemas:extension:{OrganizationName}:{Version}
    ```

    Examples:

    - `urn:enterprise:params:scim:schemas:extension:Contoso:2.0`
    - `urn:enterprise:params:scim:schemas:extension:AcmeCorp:1.0`

!!! warning "Do not reuse SCIM core URNs"
    - `urn:ietf:params:scim:schemas:core:2.0:User` ← official only
    - `urn:ietf:params:scim:schemas:core:2.0:Group` ← official only

---

## Schema generation

EzSCIM auto-generates the `/scim/Schemas` response from your annotated classes via reflection.
The generator is called once at startup and cached.

```csharp
// Access the generated schema (e.g. to add it to a custom schemas response)
var userSchema = ScimSchemaGenerator.GetSchema<AcmeUser>();
var groupSchema = ScimSchemaGenerator.GetSchema<AcmeGroup>();
```

??? tip "Validate schema loading at startup"

    ```csharp
    var schema = ScimSchemaGenerator.GetSchema<AcmeUser>();
    if (schema.Attributes.Count == 0)
        app.Logger.LogWarning("AcmeUser schema has no attributes — check [ScimProperty] annotations");
    else
        app.Logger.LogInformation("AcmeUser schema: {Count} attributes", schema.Attributes.Count);
    ```

---

## Register custom types in DI

When using `AcmeUser` and `AcmeGroup` instead of the base `ScimUser` / `ScimGroup`:

```csharp
// Data repository (IQueryable path)
builder.Services.AddScoped<IUserGroupDataRepository<AcmeUser, AcmeGroup>, AcmeRepository>();
builder.Services.AddScoped<IScimFilterTranslator<AcmeUser>, GenericScimFilterTranslator<AcmeUser>>();
builder.Services.AddScoped<IScimFilterTranslator<AcmeGroup>, GenericScimFilterTranslator<AcmeGroup>>();

// SCIM repository for the custom types
builder.Services.AddScoped<IScimUserGroupRepository<AcmeUser, AcmeGroup>, AcmeScimRepository>();
```

---

**Next**: [SCIM 2.0 attribute reference →](./scim-attributes.md) | [EF Core getting started →](../efcore/getting-started.md)


