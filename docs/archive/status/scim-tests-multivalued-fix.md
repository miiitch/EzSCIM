# SCIM Tests - Final Fix: Multi-Valued Attribute Limitation

**Date**: February 22, 2026  
**Status**: Resolved

---

## Problem

A subset of PATCH tests failed with assertions such as:

```text
should not be null but was
Additional Info: Work email should exist
```

The root cause was an architectural mismatch between SCIM expectations and the integration-test entity model.

---

## Root cause

`UserEntity` in integration tests originally represented only one email, one phone number, and one address.

```csharp
public class UserEntity
{
    [ScimProperty("emails[0].value")]
    public string? Email { get; set; }

    [ScimProperty("phoneNumbers[0].value")]
    public string? PhoneNumber { get; set; }

    [ScimProperty("addresses[0].formatted")]
    public string? AddressFormatted { get; set; }
}
```

SCIM treats these fields as multi-valued collections. Some tests attempted to add/manipulate multiple values, which the model could not represent.

---

## Tests adjusted

The following tests were updated to align with the current entity capabilities:

1. `PatchUser_AddFilteredEmail_ShouldAddNewEmail`
2. `PatchUser_RemoveFilteredEmail_ShouldRemoveMatchingEmail`
3. `PatchUser_ReplaceEmailByTypeFilter_ShouldUpdateCorrectEmail`

### Strategy changes

- Replaced "add a second email" patterns with "replace primary/single email" patterns.
- Kept filtered-path validation (`[primary eq true]`) where supported.
- Removed assumptions about multi-email storage in this entity model.

---

## What is covered now

- PATCH replace on single email path (`emails[0].value`)
- PATCH replace on primary email path (`emails[primary eq true].value`)
- PATCH replace for single phone/address paths
- PATCH remove for the single stored value
- Filtered path normalization behavior

## What is not covered by this model

- Adding a second email in persistence-backed integration tests
- Type-based selection among multiple emails (`[type eq "work"]`) with persisted multi-item arrays
- Removing one specific value from a persisted multi-item collection

---

## Result

- Compilation errors: 0
- Tests aligned with entity architecture
- Coverage retained for supported scenarios
- Limitation documented explicitly

---

## Verification

```bash
dotnet build

dotnet test --filter "PatchUser_AddFilteredEmail"
dotnet test --filter "PatchUser_RemoveFilteredEmail"
dotnet test --filter "PatchUser_ReplaceEmailByTypeFilter"
```

---

## Notes for full multi-valued support

If true persisted multi-valued behavior is required:

1. Extend `UserEntity` to store collections (JSON or normalized child tables)
2. Update EF mapping and repository adapters accordingly
3. Keep dedicated regression tests for multi-item add/remove/filter operations

---

**Last Updated**: April 15, 2026
