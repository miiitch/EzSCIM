# SCIM Validator Run 06 - PATCH User Error Analysis

**Date**: February 22, 2026  
**Validator**: https://scimvalidator.microsoft.com/  
**Correlation ID**: 79a7b13c-08a6-4619-9444-955bcafa30bf  
**Test File**: `docs/scim-test-results/scim-results-06.json`

## Executive Summary

**Failed Test**: "Patch User - Replace Attributes"  
**Status**: FAILED (HasTestPassed: false)  
**Test ID**: 72  
**Category**: Core SCIM Compliance  
**Severity**: Critical

The SCIM validator test "Patch User - Replace Attributes" fails because PATCH operations with filtered paths on multi-valued attributes (e.g., `emails[primary eq true].value`) are not being persisted when combined with a scalar replace operation in the same request.

---

## Test Scenario

### Initial User Creation

**Request**: POST /scim/Users
```json
{
  "externalId": "e9e8e7a1-a02b-42d5-ba02-b4627c1bd8b5",
  "name": {
    "formatted": "Pearline",
    "familyName": "Dorris",
    "givenName": "Shaun",
    "middleName": "Dewayne",
    "honorificPrefix": "Iva",
    "honorificSuffix": "Harrison"
  },
  "displayName": "XLQZKLTAHXBZ",
  "nickName": "NJPFXFGCVXRL",
  "userName": "roberto@monahan.uk",
  "emails": [{"primary": "true", "value": "chandler@kub.co.uk"}],
  "phoneNumbers": [{"primary": "true", "value": "57-391-0986"}],
  "addresses": [{
    "primary": "true",
    "formatted": "VQCPEAPKVFCK",
    "streetAddress": "9391 Lavonne Islands",
    "locality": "NILKMYVQUFWG",
    "region": "OBNJPCOUJNYD",
    "postalCode": "cp2 0wf",
    "country": "Mongolia"
  }]
}
```

**Response**: 201 Created  
**User ID**: 54cf16cc-a037-449f-9609-6bbe19b1e82d

---

### PATCH Request (9 Operations)

**Request**: PATCH /scim/Users/54cf16cc-a037-449f-9609-6bbe19b1e82d

```json
{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    {"op": "replace", "path": "emails[primary eq true].value", "value": "carolina_wiegand@walsh.com"},
    {"op": "replace", "path": "phoneNumbers[primary eq true].value", "value": "1-836-2162"},
    {"op": "replace", "path": "addresses[primary eq true].formatted", "value": "SBQHSNKIAZEB"},
    {"op": "replace", "path": "addresses[primary eq true].streetAddress", "value": "09606 Hoeger Mill"},
    {"op": "replace", "path": "addresses[primary eq true].locality", "value": "ZILTLPHYLUYX"},
    {"op": "replace", "path": "addresses[primary eq true].region", "value": "EVKCHDQVNMNS"},
    {"op": "replace", "path": "addresses[primary eq true].postalCode", "value": "dv8 5kk"},
    {"op": "replace", "path": "addresses[primary eq true].country", "value": "Puerto Rico"},
    {
      "op": "replace",
      "value": {
        "externalId": "b4204d31-dc20-4a03-9019-4dc1b65ed729",
        "name.formatted": "Lorena",
        "name.familyName": "Cody",
        "name.givenName": "Jerel",
        "name.middleName": "Tyrique",
        "name.honorificPrefix": "Roma",
        "name.honorificSuffix": "Gordon",
        "displayName": "EPUSKQJNVTUS",
        "nickName": "AVYBNQNLAWHW",
        "profileUrl": "QMSAFFAITIKM",
        "title": "QMKXHPPULDXQ",
        "userType": "MQWLOAYJZKCW",
        "preferredLanguage": "lkt-US",
        "locale": "SWIGNCIQKUVE",
        "timezone": "Africa/El_Aaiun",
        "active": true
      }
    }
  ]
}
```

**Response**: 200 OK  
**Response Body**: Shows all values present immediately after PATCH

---

### Verification GET Request

**Request**: GET /scim/Users/54cf16cc-a037-449f-9609-6bbe19b1e82d

**Expected**: All 8 filtered multi-valued attribute changes should be persisted

**Actual**: The validator reports that the following values are MISSING:

1. ❌ `emails[primary eq true].value` - Expected: "carolina_wiegand@walsh.com"
2. ❌ `phoneNumbers[primary eq true].value` - Expected: "1-836-2162"
3. ❌ `addresses[primary eq true].formatted` - Expected: "SBQHSNKIAZEB"
4. ❌ `addresses[primary eq true].streetAddress` - Expected: "09606 Hoeger Mill"
5. ❌ `addresses[primary eq true].locality` - Expected: "ZILTLPHYLUYX"
6. ❌ `addresses[primary eq true].region` - Expected: "EVKCHDQVNMNS"
7. ❌ `addresses[primary eq true].postalCode` - Expected: "dv8 5kk"
8. ❌ `addresses[primary eq true].country` - Expected: "Puerto Rico"

---

## Error Messages from Validator

```json
{
  "Results": [
    {
      "Message": "The value of emails[primary eq true].value is Missing from the fetched Resource",
      "Outcome": 1
    },
    {
      "Message": "The value of phoneNumbers[primary eq true].value is Missing from the fetched Resource",
      "Outcome": 1
    },
    {
      "Message": "The value of addresses[primary eq true].formatted is Missing from the fetched Resource",
      "Outcome": 1
    },
    {
      "Message": "The value of addresses[primary eq true].streetAddress is Missing from the fetched Resource",
      "Outcome": 1
    },
    {
      "Message": "The value of addresses[primary eq true].locality is Missing from the fetched Resource",
      "Outcome": 1
    },
    {
      "Message": "The value of addresses[primary eq true].region is Missing from the fetched Resource",
      "Outcome": 1
    },
    {
      "Message": "The value of addresses[primary eq true].postalCode is Missing from the fetched Resource",
      "Outcome": 1
    },
    {
      "Message": "The value of addresses[primary eq true].country is Missing from the fetched Resource",
      "Outcome": 1
    }
  ]
}
```

---

## Root Cause Analysis

### Problem Location

**File**: `EzSCIM.IntegrationTests/ScimPatchApplier.cs`  
**Method**: `ApplyOperation` (lines 111-145)

### Issue Description

The `ScimPatchApplier` class handles PATCH operations for Entity Framework entities using reflection and `ScimProperty` attribute mappings. The problem occurs when processing PATCH operations with **filtered paths** on multi-valued attributes.

### Flow Analysis

1. **Operation 1-8**: Replace operations with filtered paths
   - Path: `emails[primary eq true].value`
   - Normalized to: `emails[0].value`
   - Property mapping lookup: **FAILS** or **NOT APPLIED**
   - Result: Changes NOT persisted to database

2. **Operation 9**: Replace without path (bulk replace)
   - Contains scalar attributes only (externalId, name.*, displayName, etc.)
   - Does NOT include emails, phoneNumbers, or addresses
   - Applied successfully via `ApplyBulkReplace`

3. **GET Request**: Fetches user from database
   - Returns original values for emails, phoneNumbers, addresses
   - Scalar attributes show updated values from operation 9

### Why Filtered Paths Fail

The `ScimPatchApplier.ApplyOperation` method:

```csharp
if (!mappings.TryGetValue(path, out var mapping))
{
    var normalizedPath = NormalizePath(path);
    if (!mappings.TryGetValue(normalizedPath, out mapping))
    {
        return false; // ← SILENTLY FAILS
    }
}
```

The `NormalizePath` method transforms:
- `emails[primary eq true].value` → `emails[0].value`
- `phoneNumbers[primary eq true].value` → `phoneNumbers[0].value`
- `addresses[primary eq true].formatted` → `addresses[0].formatted`

**However**, the property mapping lookup is case-insensitive but requires EXACT matches. The normalized path should match the `ScimProperty` attribute on `UserEntity`, but something in the matching logic prevents it from working.

### Suspected Issues

1. **Path normalization might not handle all filter expression variants**
2. **Property mappings might not include all normalized variations**
3. **Silent failure on missing mapping** - operations return `false` without logging or throwing

---

## Impact

### SCIM Compliance

- **RFC 7644 Section 3.5.2**: PATCH operations with filtered paths MUST be supported
- **Interoperability**: Microsoft Entra ID and other identity providers rely on filtered path syntax
- **Data Loss**: User attribute updates are silently ignored

### Affected Operations

- ✅ Scalar attribute updates (displayName, title, etc.) - **WORKING**
- ✅ Bulk replace without path - **WORKING**
- ❌ Filtered path on multi-valued attributes - **FAILING**
- ❌ Mixed operations (filtered + scalar) - **PARTIALLY FAILING**

---

## Proposed Solution

### Option 1: Fix ScimPatchApplier (Recommended)

Enhance `ScimPatchApplier.ApplyOperation` to properly handle filtered paths:

1. **Parse filter expressions** (e.g., `primary eq true`)
2. **Locate matching array element** in the target entity
3. **Apply value to correct sub-attribute**
4. **Log warnings** for unmapped paths instead of silent failure

### Option 2: Implement Full PATCH Handler

Create a dedicated PATCH handler similar to `InMemoryScimRepository.ApplyUserPatchOperation` that:
- Parses filtered paths using existing `AttributeFilterHelper`
- Handles multi-valued attribute operations correctly
- Integrates with Entity Framework for persistence

### Option 3: Use InMemoryScimRepository Logic

Extract the PATCH logic from `InMemoryScimRepository` into a shared service that works with both in-memory and EF Core repositories.

---

## Test Coverage Required

### Regression Tests

1. **Single filtered path operation**
   - `emails[primary eq true].value` replacement
   - Verify persistence via GET

2. **Multiple filtered path operations**
   - All address fields replacement
   - Verify all fields persisted

3. **Mixed operations (Run 06 scenario)**
   - 8 filtered path operations + 1 bulk scalar replace
   - Verify ALL changes persisted

4. **Different filter expressions**
   - `[primary eq true]`
   - `[type eq "work"]`
   - `[value eq "specific@email.com"]`

### Test Files

- `ScimValidatorComplianceTests.cs`: Add Run 06 specific test
- New test class for PATCH filtered path scenarios

---

## References

- **SCIM RFC 7644**: https://datatracker.ietf.org/doc/html/rfc7644
  - Section 3.5.2: Modifying with PATCH
  - Section 3.5.2.2: Removing Attributes and Values
  - Figure 5: Examples of PATCH operations with filters

- **Validator Results**: `docs/scim-test-results/scim-results-06.json`
- **Integration Tests**: `EzSCIM.IntegrationTests/ScimValidatorComplianceTests.cs`
- **PATCH Applier**: `EzSCIM.IntegrationTests/ScimPatchApplier.cs`

---

## Next Steps

1. ✅ Document the issue (this file)
2. ⏳ Create regression test that reproduces the bug
3. ⏳ Verify test fails with current implementation
4. ⏳ Implement fix in `ScimPatchApplier`
5. ⏳ Verify all tests pass
6. ⏳ Re-run SCIM validator and verify compliance

---

**Last Updated**: February 22, 2026  
**Status**: Analysis Complete - Ready for Implementation

