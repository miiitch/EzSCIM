# SCIM Validator Errors Analysis

**Test Date**: February 22, 2026  
**Validator**: Microsoft SCIM Validator (https://scimvalidator.microsoft.com/)  
**Correlation ID**: 7a788491-24be-42e6-b7c3-69f70e4c5c71

## Summary

- **Total Tests**: 92 tests
- **Passed Tests**: 86 tests
- **Failed Tests**: 3 tests
- **Preview Tests with Issues**: 1 test
- **Overall Compliance**: 96.7% (3 failures)

## Critical Issues

### 1. PATCH User - Replace Attributes (FAILED)
**Test ID**: 61  
**Category**: Core SCIM Functionality  
**Severity**: High

#### Issue Description
When executing a PATCH request with multiple replace operations on different attributes, the API does not return the replaced attributes in the subsequent GET request.

#### Failed Validations
The following attributes are missing from the fetched resource after the PATCH operation:
- `emails[primary eq true].value` - Missing
- `phoneNumbers[primary eq true].value` - Missing
- `addresses[primary eq true].formatted` - Missing
- `addresses[primary eq true].streetAddress` - Missing
- `addresses[primary eq true].locality` - Missing
- `addresses[primary eq true].region` - Missing
- `addresses[primary eq true].postalCode` - Missing
- `addresses[primary eq true].country` - Missing

#### Request Details
```
PATCH /scim/Users/acc335d5-2100-41ba-b0c8-6119f24e0d49
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    {"op": "replace", "path": "emails[primary eq true].value", "value": "mary@dickens.co.uk"},
    {"op": "replace", "path": "phoneNumbers[primary eq true].value", "value": "39-949-9451"},
    {"op": "replace", "path": "addresses[primary eq true].formatted", "value": "EMZNFSDRVIIF"},
    {"op": "replace", "path": "addresses[primary eq true].streetAddress", "value": "2645 Renner Burg"},
    {"op": "replace", "path": "addresses[primary eq true].locality", "value": "ZNKYOYRSCLWA"},
    {"op": "replace", "path": "addresses[primary eq true].region", "value": "LTODLTJZFVNK"},
    {"op": "replace", "path": "addresses[primary eq true].postalCode", "value": "vn55 2ue"},
    {"op": "replace", "path": "addresses[primary eq true].country", "value": "Ukraine"},
    {"op": "replace", "value": {"externalId": "16bf3b76-243b-49e1-a939-5176753d02c8", ...}}
  ]
}
```

#### Root Cause Analysis
The PATCH implementation is not properly handling complex filter expressions like `[primary eq true]` on multi-valued attributes. The API appears to be ignoring replace operations on filtered paths and/or not persisting the changes correctly.

#### Resolution Required
- Implement proper SCIM filter expression parsing for PATCH operations
- Ensure that replace operations on multi-valued attributes with filter expressions are correctly applied
- Verify that all replaced attributes are persisted and returned in subsequent GET requests

---

### 2. GET Group by ID with excludedAttributes (FAILED)
**Test ID**: 70  
**Category**: Attribute Filtering  
**Severity**: High

#### Issue Description
When requesting a Group resource with `excludedAttributes=members` query parameter, the API returns the `members` attribute in the response, which violates the SCIM specification.

#### Failed Validation
- **Message**: "The response should exclude the attribute: members"
- **Outcome**: Failed (Outcome: 1)

#### Request Details
```
GET /scim/Groups/4ebace1d-a5a0-4749-b33e-0bf0139254cc?excludedAttributes=members
```

#### Response (Current - INCORRECT)
```json
{
  "externalId": "b517130d-96c0-4bc6-92fb-6a477d53d7ba",
  "members": [],                    ← SHOULD BE EXCLUDED
  "customAttributes": {},
  "id": "4ebace1d-a5a0-4749-b33e-0bf0139254cc",
  "displayName": "MIKMFSFZFILL",
  "schemas": ["urn:ietf:params:scim:schemas:core:2.0:Group"],
  "meta": {
    "resourceType": "Group",
    "created": "2026-02-22T14:26:50.9104345Z",
    "lastModified": "2026-02-22T14:26:50.9104346Z",
    "location": "/scim/Groups/4ebace1d-a5a0-4749-b33e-0bf0139254cc"
  }
}
```

#### Root Cause Analysis
The `excludedAttributes` query parameter is not being properly processed on the GET endpoint for Groups. The API is ignoring the exclusion list and returning all attributes.

#### Resolution Required
- Implement proper handling of the `excludedAttributes` query parameter
- Ensure that excluded attributes are filtered out before serialization
- Test with various attribute exclusion patterns (e.g., `members`, `meta.lastModified`, etc.)

---

### 3. Filter Groups with excludedAttributes (FAILED)
**Test ID**: 72  
**Category**: Attribute Filtering  
**Severity**: High

#### Issue Description
When filtering Groups with `excludedAttributes=members`, the API returns the `members` attribute in the list response, violating the SCIM specification.

#### Failed Validation
- **Message**: "The response should exclude the attribute: members"
- **Outcome**: Failed (Outcome: 1)

#### Request Details
```
GET /scim/Groups?excludedAttributes=members&filter=displayName eq "MIKMFSFZFILL"
```

#### Response (Current - INCORRECT)
The response includes `members` attribute which should be excluded based on the query parameter.

#### Root Cause Analysis
Similar to issue #2, the `excludedAttributes` parameter is not being processed in filtered list responses. The filtering logic only handles the filter expressions but ignores attribute exclusions.

#### Resolution Required
- Apply `excludedAttributes` filtering to list responses
- Ensure consistency between single-resource GET and list GET operations
- Validate that the same exclusion logic applies to both endpoints

---

## Non-Critical Issues

### Preview Test: PATCH User - Multiple Operations on Different Attributes (PARTIAL FAILURE)
**Test ID**: 82  
**Category**: Extended SCIM Functionality (Preview)  
**Severity**: Medium

#### Issues Found
1. **externalId Mismatch**: The value of externalId after the PATCH should be `8e25e04b-a001-4820-905c-7807436e33b1` but is `d4d1c35e-b442-452e-8aa5-a5276c339e87`
2. **nickName Not Removed**: The value of `nickName` should not be in the fetched resource after a remove operation

#### Context
This appears to be related to the primary PATCH issue (Issue #1) where multiple operations on different attributes are not being correctly applied.

---

## Error Messages Analysis

### French Error Messages in Responses
The API is returning error messages in French:
- Example: `"detail": "<legacy French duplicate-user message>"` (`User already exists`)
- Example: `"detail": "<legacy French group-not-found message>"` (`Group not found`)

**Note**: According to project coding standards, all error messages should be in English for API consistency and international compatibility.

---

## Recommendations

### Priority 1 (Critical - Block Production)
1. **Fix `excludedAttributes` Support**
   - Implement filtering logic in the response serialization
   - Apply to both single-resource and list endpoints
   - Test all commonly excluded attributes

2. **Fix PATCH Multi-Attribute Replace Operations**
   - Review PATCH request parsing for complex filter expressions
   - Ensure filter expressions like `[primary eq true]` are correctly evaluated
   - Verify data persistence after multi-operation PATCH requests

### Priority 2 (Important)
3. **Standardize Error Messages to English**
   - Replace all French error messages with English equivalents
   - Implement consistent error message format across all endpoints

### Priority 3 (Quality)
4. **Add Unit Tests for Edge Cases**
   - Test PATCH operations with complex filter expressions
   - Test `excludedAttributes` with various attribute combinations
   - Test list responses with both filters and attribute exclusions

---

## Test Categories

### Passed Categories
- ✅ Create Users and Groups
- ✅ Create duplicate detection
- ✅ Filter existing resources
- ✅ Filter non-existing resources
- ✅ Case-insensitive filtering
- ✅ Pagination
- ✅ Single resource GET
- ✅ Delete operations
- ✅ Basic PATCH operations

### Failed Categories
- ❌ PATCH with complex filter expressions
- ❌ GET with excludedAttributes parameter
- ❌ List filtering with excludedAttributes

---

## Next Steps

1. Review and fix `excludedAttributes` implementation
2. Debug PATCH operation with multi-valued attribute filters
3. Update error messages to English
4. Re-run validation test suite
5. Verify SFCompliance flag (currently: SFComplianceFailed: true)
