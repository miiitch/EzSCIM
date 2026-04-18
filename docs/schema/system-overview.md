# SCIM Schema Auto-Generation System

This document describes the automatic SCIM schema generation system based on C# attributes. Schemas are built from annotated classes/properties and cached in thread-safe static initialization.

## Quick start

### 1) Define a SCIM model

```csharp
using ScimAPI.Attributes;

[ScimResource(
    "urn:ietf:params:scim:schemas:core:2.0:User",
    "User",
    "User Account")]
public class ScimUser
{
    // System property (not in schema because no attribute)
    public string Id { get; set; }

    // Simple SCIM attribute
    [ScimProperty("userName", "string", Required = true, Uniqueness = "server")]
    public string UserName { get; set; }

    // Complex attribute
    [ScimProperty("name", "complex")]
    public ScimName Name { get; set; }

    // Multi-valued attribute
    [ScimProperty("emails", "complex", MultiValued = true)]
    public List<ScimEmail> Emails { get; set; }
}

public class ScimName
{
    [ScimProperty("givenName", "string")]
    public string? GivenName { get; set; }

    [ScimProperty("familyName", "string")]
    public string? FamilyName { get; set; }
}
```

### 2) Access generated schemas

```csharp
using ScimAPI.Helpers;

var userSchema = ScimSchemaGenerator.UserSchema;
var groupSchema = ScimSchemaGenerator.GroupSchema;

Console.WriteLine($"User schema: {userSchema.Attributes.Count} attributes");
```

### 3) Return schemas in a controller

```csharp
[HttpGet("Schemas")]
public IActionResult GetSchemas()
{
    return Ok(new[]
    {
        ScimSchemaGenerator.UserSchema,
        ScimSchemaGenerator.GroupSchema
    });
}
```

## Core concepts

### Opt-in design

Only properties with `[ScimProperty]` are included in generated schemas.

```csharp
public class ScimUser
{
    // Not included (no attribute)
    public string Id { get; set; }
    public ScimMeta Meta { get; set; }

    // Included
    [ScimProperty("userName", "string")]
    public string UserName { get; set; }
}
```

### Inheritance support

Annotated properties from base classes are included automatically in derived schemas.

```csharp
public class ScimUser
{
    [ScimProperty("userName", "string")]
    public string UserName { get; set; }
}

public class EnterpriseUser : ScimUser
{
    [ScimProperty("employeeNumber", "string")]
    public string EmployeeNumber { get; set; }
}

var schema = ScimSchemaGenerator.GetSchema<EnterpriseUser>();
// Includes userName and employeeNumber
```

### Complex type discovery

Sub-attributes are discovered recursively.

```csharp
[ScimProperty("name", "complex")]
public ScimName Name { get; set; }

public class ScimName
{
    [ScimProperty("givenName", "string")]
    public string? GivenName { get; set; }

    [ScimProperty("familyName", "string")]
    public string? FamilyName { get; set; }
}
```

### Thread-safe caching

`ScimSchemaGenerator` static constructor guarantees one-time initialization.

```csharp
public static class ScimSchemaGenerator
{
    public static ScimSchema UserSchema { get; }
    public static ScimSchema GroupSchema { get; }

    static ScimSchemaGenerator()
    {
        UserSchema = GenerateSchema<ScimUser>();
        GroupSchema = GenerateSchema<ScimGroup>();
    }
}
```

## Available attributes

### `[ScimResource]` (class-level)

```csharp
[ScimResource(
    "urn:ietf:params:scim:schemas:core:2.0:User",  // schema URN
    "User",                                        // resource name
    "User Account"                                 // description
)]
```

### `[ScimProperty]` (property-level)

```csharp
[ScimProperty(
    "userName",                // attribute name (required)
    "string",                  // SCIM type (required)
    Required = true,
    MultiValued = false,
    Description = "...",
    Uniqueness = "server",     // none, server, global
    Mutability = "readWrite",  // readOnly, readWrite, immutable, writeOnly
    Returned = "default",      // always, never, default, request
    CaseExact = false
)]
```

Supported SCIM primitive/complex types:
- `string`
- `boolean`
- `integer`
- `decimal`
- `dateTime`
- `reference`
- `complex`
- `binary`

## Extend the system

### Define a custom resource

```csharp
[ScimResource(
    "urn:enterprise:params:scim:schemas:extension:MyCompany:2.0",
    "EnterpriseUser",
    "Enterprise User with custom attributes")]
public class EnterpriseUser : ScimUser
{
    [ScimProperty("employeeNumber", "string", Required = true)]
    public string EmployeeNumber { get; set; }

    [ScimProperty("department", "string")]
    public string? Department { get; set; }

    [ScimProperty("badge", "complex")]
    public BadgeInfo? Badge { get; set; }
}

public class BadgeInfo
{
    [ScimProperty("badgeNumber", "string")]
    public string? BadgeNumber { get; set; }

    [ScimProperty("accessLevel", "integer")]
    public int AccessLevel { get; set; }
}
```

### Generate custom schema

```csharp
// Recommended: static helper cache
public static class EnterpriseSchemaGenerator
{
    public static ScimSchema EnterpriseUserSchema { get; }

    static EnterpriseSchemaGenerator()
    {
        EnterpriseUserSchema = ScimSchemaGenerator.GetSchema<EnterpriseUser>();
    }
}

// Alternative: on-demand generation
var schema = ScimSchemaGenerator.GetSchema<EnterpriseUser>();
```

## Testing commands

```bash
dotnet build
dotnet test
```

If the API is running, verify schema endpoints:

```bash
curl http://localhost:5000/scim/Schemas -H "Authorization: Bearer <token>"
curl "http://localhost:5000/scim/Schemas/urn:ietf:params:scim:schemas:core:2.0:User" -H "Authorization: Bearer <token>"
```

## Architecture summary

```text
Annotated models (ScimUser / ScimGroup / custom types)
            |
            v
ScimSchemaGenerator (reflection + recursive discovery)
            |
            v
Static cached schemas (UserSchema / GroupSchema / custom schema)
            |
            v
SCIM controllers (/scim/Schemas)
```

## Benefits

- Declarative schema definition on models
- Type-safe and refactor-friendly
- High performance through static cache
- Thread-safe initialization
- Easy extension for custom resources
- Reduced data exposure through opt-in design

## Related docs

- [`extension-guide.md`](./extension-guide.md)
- [`models-required-optional.md`](./models-required-optional.md)
- [`expected-actual-pattern.md`](./expected-actual-pattern.md)
- [`testing-scim-schema-validation.md`](./testing-scim-schema-validation.md)
