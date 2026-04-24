# UserEntity Migration - Multi-Valued Attribute Support

**Date**: February 22, 2026  
**Status**: Migration complete  
**Type**: Major architecture update

---

## Goal

Evolve `UserEntity` to support multiple emails, phone numbers, and addresses, consistent with SCIM multi-valued semantics.

---

## Architecture change

### Before (single-valued fields)

```csharp
public class UserEntity
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AddressFormatted { get; set; }
    public string? AddressStreetAddress { get; set; }
}
```

### After (JSON-backed collections)

```csharp
public class UserEntity
{
    [Column(TypeName = "jsonb")]
    public string? EmailsJson { get; set; }

    [Column(TypeName = "jsonb")]
    public string? PhoneNumbersJson { get; set; }

    [Column(TypeName = "jsonb")]
    public string? AddressesJson { get; set; }
}
```

---

## New components

### 1) `MultiValuedAttributeHelper.cs`

Provides POCO models and JSON serialization helpers for:
- email entries
- phone entries
- address entries

### 2) `UserEntityExtensions.cs`

Extension methods:
- `ToScimUser()`
- `UpdateFromScimUser()`

Handles JSON <-> SCIM collection conversion.

### 3) `UserEntityPatchApplier.cs`

Specialized PATCH logic for JSON-backed multi-valued attributes.

Supports:
- filtered paths (for example `emails[primary eq true].value`)
- `add`, `replace`, `remove`
- nested sub-attributes (`value`, `type`, `primary`, address fields)

### 4) `JsonUserRepositoryAdapter.cs`

Adapter that relies on JSON-aware extension methods and PATCH applier.

### 5) `SeedData.cs` updates

Seed users now include JSON payloads for emails/phones/addresses.

---

## Updated files

- `EzSCIM.IntegrationTests/Data/Entities/UserEntity.cs`
- `EzSCIM.IntegrationTests/ScimWebApplicationFactory.cs`
- `EzSCIM.IntegrationTests/Data/SeedData.cs`

---

## Supported scenarios after migration

### Multiple emails

```json
{
  "op": "add",
  "path": "emails",
  "value": [
    { "value": "work@company.com", "type": "work", "primary": false }
  ]
}
```

```json
{
  "op": "replace",
  "path": "emails[type eq \"work\"].value",
  "value": "newwork@company.com"
}
```

### Multiple phone numbers

```json
{
  "op": "add",
  "path": "phoneNumbers",
  "value": [
    { "value": "555-1234", "type": "mobile", "primary": false }
  ]
}
```

### Multiple addresses

```json
{
  "op": "replace",
  "path": "addresses[type eq \"home\"].streetAddress",
  "value": "456 New Home St"
}
```

---

## Validation status

- Entity model migrated to JSON-based multi-valued storage
- Repository adapter and patch applier aligned with new shape
- Seed/test data updated
- Regression scenarios available for filtered-path PATCH operations

---

## Notes

This migration targets test infrastructure and adapter behavior. If production persistence needs normalized relational tables instead of JSON, you can keep the same SCIM mapping model and swap storage strategy at repository level.

---

**Last Updated**: April 15, 2026
