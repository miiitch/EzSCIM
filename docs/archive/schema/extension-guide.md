# SCIM Schema Extension Guide

This guide explains how to extend the SCIM schema system by creating custom types based on `ScimUser` and `ScimGroup`.

## Table of Contents

1. [Attribute system overview](#attribute-system-overview)
2. [Create a custom user type](#create-a-custom-user-type)
3. [Configure dependency injection](#configure-dependency-injection)
4. [Generate custom schemas](#generate-custom-schemas)
5. [Best practices](#best-practices)

---

## Attribute system overview

The SCIM schema system uses a declarative **opt-in** approach based on two attributes.

### `[ScimResource]` - class level

Defines SCIM schema metadata for a resource.

```csharp
[ScimResource(
    "urn:ietf:params:scim:schemas:core:2.0:User",
    "User",
    "User Account"
)]
public class ScimUser { ... }
```

**Properties:**
- `Schema` (required): SCIM schema URN
- `Name` (required): Resource name
- `Description` (required): Resource description

### `[ScimProperty]` - property level

Marks a property as part of the SCIM schema. **Only annotated properties are included in generated schemas.**

```csharp
[ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
public string UserName { get; set; }
```

**Properties:**
- `Name` (required): SCIM attribute name
- `Type` (required): SCIM data type (`string`, `boolean`, `complex`, `reference`, `dateTime`, `integer`, `decimal`, `binary`)
- `Required` (default: `false`): Whether the attribute is required
- `MultiValued` (default: `false`): Whether multiple values are allowed
- `Description`: Attribute description
- `Uniqueness` (default: `none`): Uniqueness constraint (`none`, `server`, `global`)
- `Mutability` (default: `readWrite`): Mutability (`readOnly`, `readWrite`, `immutable`, `writeOnly`)
- `Returned` (default: `default`): Return behavior (`always`, `never`, `default`, `request`)
- `CaseExact` (default: `false`): Case-sensitive comparison

---

## Create a custom user type

### Example: enterprise user with custom attributes

```csharp
using ScimAPI.Attributes;
using ScimAPI.Models;

namespace MyCompany.Scim.Models
{
    /// <summary>
    /// Extends ScimUser with enterprise-specific attributes.
    /// </summary>
    [ScimResource(
        "urn:enterprise:params:scim:schemas:extension:MyCompany:2.0",
        "EnterpriseUser",
        "Enterprise User with custom attributes"
    )]
    public class EnterpriseUser : ScimUser
    {
        /// <summary>
        /// Unique employee number.
        /// </summary>
        [ScimProperty("employeeNumber", "string", Required = true, Uniqueness = "server",
            Description = "Unique employee number")]
        public string EmployeeNumber { get; set; } = string.Empty;

        /// <summary>
        /// Department code.
        /// </summary>
        [ScimProperty("department", "string", Description = "Department code")]
        public string? Department { get; set; }

        /// <summary>
        /// Manager reference.
        /// </summary>
        [ScimProperty("manager", "reference", Description = "Reference to manager")]
        public string? ManagerId { get; set; }

        /// <summary>
        /// Employee hire date.
        /// </summary>
        [ScimProperty("hireDate", "dateTime", Description = "Employee hire date")]
        public DateTime? HireDate { get; set; }

        /// <summary>
        /// Cost centers (multi-valued).
        /// </summary>
        [ScimProperty("costCenters", "string", MultiValued = true,
            Description = "Cost centers")]
        public List<string> CostCenters { get; set; } = new();

        /// <summary>
        /// Badge information (complex type).
        /// </summary>
        [ScimProperty("badge", "complex", Description = "Badge information")]
        public BadgeInfo? Badge { get; set; }
    }

    /// <summary>
    /// Employee badge data.
    /// </summary>
    public class BadgeInfo
    {
        [ScimProperty("badgeNumber", "string", Description = "Badge number")]
        public string? BadgeNumber { get; set; }

        [ScimProperty("issueDate", "dateTime", Description = "Badge issue date")]
        public DateTime? IssueDate { get; set; }

        [ScimProperty("expiryDate", "dateTime", Description = "Badge expiry date")]
        public DateTime? ExpiryDate { get; set; }

        [ScimProperty("accessLevel", "integer", Description = "Access level")]
        public int AccessLevel { get; set; }
    }
}
```

### Important notes

1. **Inherited properties**: all annotated properties in `ScimUser` are automatically included in the derived schema.
2. **Custom URN**: use the format `urn:enterprise:params:scim:schemas:extension:{OrganizationName}:{Version}` for extensions.
3. **Complex types**: nested complex types (for example `BadgeInfo`) should also use `[ScimProperty]` annotations.

---

## Configure dependency injection

### Option 1: dedicated repository for the custom type

```csharp
using ScimAPI.Repositories;
using MyCompany.Scim.Models;

namespace MyCompany.Scim.Repositories
{
    public class EnterpriseUserRepository : IScimUserOnlyRepository<EnterpriseUser>
    {
        private readonly Dictionary<string, EnterpriseUser> _users = new();

        public Task<EnterpriseUser?> GetUserAsync(string id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        // ... other methods ...
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddSingleton<IScimUserOnlyRepository<EnterpriseUser>, EnterpriseUserRepository>();
```

### Option 2: combined repository (users + groups)

```csharp
public class EnterpriseScimRepository :
    IScimUserGroupRepository<EnterpriseUser, ScimGroup>
{
    // Implements both user and group operations
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddSingleton<EnterpriseScimRepository>();
builder.Services.AddSingleton<IScimUserOnlyRepository<EnterpriseUser>>(sp =>
    sp.GetRequiredService<EnterpriseScimRepository>());
builder.Services.AddSingleton<IScimUserGroupRepository<EnterpriseUser, ScimGroup>>(sp =>
    sp.GetRequiredService<EnterpriseScimRepository>());
```

---

## Generate custom schemas

### Method 1: static helper class (recommended)

Create a helper similar to `ScimSchemaGenerator`:

```csharp
using ScimAPI.Helpers;
using ScimAPI.Models;
using MyCompany.Scim.Models;

namespace MyCompany.Scim.Helpers
{
    public static class EnterpriseSchemaGenerator
    {
        /// <summary>
        /// Pre-generated schema for EnterpriseUser.
        /// Initialized once in the static constructor.
        /// </summary>
        public static ScimSchema EnterpriseUserSchema { get; }

        static EnterpriseSchemaGenerator()
        {
            EnterpriseUserSchema = ScimSchemaGenerator.GetSchema<EnterpriseUser>();
            Console.WriteLine($"[EnterpriseSchemaGenerator] EnterpriseUser schema initialized: {EnterpriseUserSchema.Attributes.Count} attributes");
        }
    }
}
```

Use in a controller:

```csharp
[HttpGet("Schemas")]
public IActionResult GetSchemas()
{
    var schemas = new List<ScimSchema>
    {
        ScimSchemaGenerator.UserSchema,
        ScimSchemaGenerator.GroupSchema,
        EnterpriseSchemaGenerator.EnterpriseUserSchema
    };
    return Ok(schemas);
}
```

### Method 2: on-demand generation

```csharp
var schema = ScimSchemaGenerator.GetSchema<EnterpriseUser>();
```

> This uses reflection every call. Prefer a static cached helper in production.

---

## Best practices

### 1) URN naming convention

For custom extension schemas:

```text
urn:enterprise:params:scim:schemas:extension:{OrganizationName}:{Version}
```

Examples:
- `urn:enterprise:params:scim:schemas:extension:Contoso:2.0`
- `urn:enterprise:params:scim:schemas:extension:AcmeCorp:1.0`

Do not modify SCIM core URNs:
- `urn:ietf:params:scim:schemas:core:2.0:User`
- `urn:ietf:params:scim:schemas:core:2.0:Group`

### 2) Opt-in schema design

Only properties with `[ScimProperty]` are exposed in generated schemas. This helps:
- Exclude internal properties (`Id`, `Schemas`, `Meta`, `CustomAttributes`)
- Precisely control SCIM exposure
- Reduce accidental data exposure

### 3) Annotation inheritance

Annotated properties in base classes (`ScimUser`, `ScimGroup`) are inherited by derived classes.

```csharp
public class ScimUser
{
    [ScimProperty("userName", "string", Required = true)]
    public string UserName { get; set; }
}

public class EnterpriseUser : ScimUser
{
    [ScimProperty("employeeNumber", "string")]
    public string EmployeeNumber { get; set; }
}
```

The generated `EnterpriseUser` schema contains both `userName` and `employeeNumber`.

### 4) Nested complex types

Annotate nested properties too:

```csharp
[ScimProperty("address", "complex")]
public CustomAddress Address { get; set; }

public class CustomAddress
{
    [ScimProperty("street", "string")]
    public string Street { get; set; }

    [ScimProperty("city", "string")]
    public string City { get; set; }
}
```

### 5) Multi-valued complex types

```csharp
[ScimProperty("certifications", "complex", MultiValued = true)]
public List<Certification> Certifications { get; set; } = new();

public class Certification
{
    [ScimProperty("name", "string")]
    public string Name { get; set; }

    [ScimProperty("issueDate", "dateTime")]
    public DateTime IssueDate { get; set; }
}
```

### 6) Startup validation

Validate schema generation at startup:

```csharp
var schema = EnterpriseSchemaGenerator.EnterpriseUserSchema;
if (schema.Attributes.Count == 0)
{
    app.Logger.LogWarning("EnterpriseUser schema has no attributes.");
}
else
{
    app.Logger.LogInformation("EnterpriseUser schema loaded with {Count} attributes", schema.Attributes.Count);
}
```

---

## Summary

1. Annotate the class with `[ScimResource]`.
2. Annotate exposed properties with `[ScimProperty]`.
3. Inherited annotated properties are included automatically.
4. Use a static schema helper for thread-safe caching and performance.
5. Use extension URNs in the format `urn:enterprise:params:scim:schemas:extension:{Org}:{Version}`.
6. Register custom repositories through dependency injection.

The schema generation system is automatic, type-safe, and efficient when combined with static pre-generation.
