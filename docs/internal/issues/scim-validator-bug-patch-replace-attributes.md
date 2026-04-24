# Bug Report: "Patch User - Replace Attributes" Incorrect Expected State Computation

## Summary

The test **"Patch User - Replace Attributes"** on [scimvalidator.microsoft.com](https://scimvalidator.microsoft.com/) incorrectly computes the expected resource state when a PATCH request combines **filtered-path operations** on multi-valued attributes with a **no-path replace** operation containing scalar attributes.

The validator **sends** `"timezone":"Africa/Lubumbashi"` in the PATCH request but **expects** the timezone to be an empty string after the operation. It also reports multi-valued attributes (emails, phoneNumbers, addresses) as "Missing from the fetched Resource" even though the server correctly applies all operations and returns them in the response.

**Validation run date:** 2026-02-23  
**Correlation ID:** `f11da8e0-85ed-4693-974a-925b7bdf8684`  
**Affected test:** "Patch User - Replace Attributes" (Category: Required)  
**Reproducibility:** Consistent across multiple runs (observed on runs 05, 06, and 07)

---

## Reported Errors (9 total)

| # | Error Message |
|---|---------------|
| 1 | `The value of timezone should be  instead of Africa/Lubumbashi` |
| 2 | `The value of emails[primary eq true].value is Missing from the fetched Resource` |
| 3 | `The value of phoneNumbers[primary eq true].value is Missing from the fetched Resource` |
| 4 | `The value of addresses[primary eq true].formatted is Missing from the fetched Resource` |
| 5 | `The value of addresses[primary eq true].streetAddress is Missing from the fetched Resource` |
| 6 | `The value of addresses[primary eq true].locality is Missing from the fetched Resource` |
| 7 | `The value of addresses[primary eq true].region is Missing from the fetched Resource` |
| 8 | `The value of addresses[primary eq true].postalCode is Missing from the fetched Resource` |
| 9 | `The value of addresses[primary eq true].country is Missing from the fetched Resource` |

---

## Step-by-Step Reproduction

### Step 1 — Validator creates a user (POST)

```http
POST /scim/Users HTTP/1.1
Content-Type: application/scim+json; charset=utf-8
```

```json
{
  "externalId": "847fb9f0-e272-451e-9bb5-9c4409eec1e9",
  "userName": "gunner@rempel.info",
  "name": {
    "formatted": "Prince",
    "familyName": "Madelynn",
    "givenName": "Felicity",
    "middleName": "Brock",
    "honorificPrefix": "Dianna",
    "honorificSuffix": "Elyse"
  },
  "displayName": "RYILLMBHZVMV",
  "nickName": "GIMFVFXLWNSK",
  "profileUrl": "AEUEHXXFPGHN",
  "title": "HDSUAYOJAJKA",
  "userType": "YDJZMFOYHKVB",
  "preferredLanguage": "en-GB",
  "locale": "UNYRWFJXPONP",
  "timezone": "Africa/Maputo",
  "active": true,
  "emails": [{ "primary": "true", "value": "justyn@jones.com" }],
  "phoneNumbers": [{ "primary": "true", "value": "31-226-3240" }],
  "addresses": [{
    "primary": "true",
    "formatted": "ZINGVLVFMQIB",
    "streetAddress": "13462 Ramiro Union",
    "locality": "YVKNYRNIKUTF",
    "region": "GPEHFTPMLEBA",
    "postalCode": "qw03 8ik",
    "country": "Colombia"
  }],
  "schemas": ["urn:ietf:params:scim:schemas:core:2.0:User"]
}
```

**Server response:** `201 Created` — user created with ID `87993052-0210-42bf-b003-85d221ddb143`, timezone = `Africa/Maputo`.

### Step 2 — Validator sends a PATCH with 9 operations

```http
PATCH /scim/Users/87993052-0210-42bf-b003-85d221ddb143 HTTP/1.1
Content-Type: application/scim+json; charset=utf-8
```

```json
{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    { "op": "replace", "path": "emails[primary eq true].value", "value": "jackeline.graham@thompson.co.uk" },
    { "op": "replace", "path": "phoneNumbers[primary eq true].value", "value": "15-532-3301" },
    { "op": "replace", "path": "addresses[primary eq true].formatted", "value": "HKHVBPXOTLPU" },
    { "op": "replace", "path": "addresses[primary eq true].streetAddress", "value": "5603 Gusikowski Overpass" },
    { "op": "replace", "path": "addresses[primary eq true].locality", "value": "QJQLYXFKNQES" },
    { "op": "replace", "path": "addresses[primary eq true].region", "value": "BHJLXPTIQJRP" },
    { "op": "replace", "path": "addresses[primary eq true].postalCode", "value": "ad39 0tr" },
    { "op": "replace", "path": "addresses[primary eq true].country", "value": "Bahrain" },
    {
      "op": "replace",
      "value": {
        "externalId": "c7145de2-8d09-4d3f-ad32-73f3ad2f7dc3",
        "name.formatted": "Theresa",
        "name.familyName": "Mateo",
        "name.givenName": "Leanne",
        "name.middleName": "Keyshawn",
        "name.honorificPrefix": "Petra",
        "name.honorificSuffix": "Donald",
        "displayName": "WPMGMAPZZZOG",
        "nickName": "XRVKUQQEWTWM",
        "profileUrl": "MVFHUEKNWNOZ",
        "title": "KDDASUUSXZHZ",
        "userType": "KHPEVIFITFOF",
        "preferredLanguage": "en-SC",
        "locale": "PHSYYLKOBPWW",
        "timezone": "Africa/Lubumbashi",
        "active": true
      }
    }
  ]
}
```

The structure is:
- **Operations 1–8**: filtered-path replace on multi-valued sub-attributes
- **Operation 9**: no-path replace with a value object containing **only scalar attributes** (no `emails`, `phoneNumbers`, or `addresses` keys)

### Step 3 — Server responds correctly (200 OK)

```json
{
  "externalId": "c7145de2-8d09-4d3f-ad32-73f3ad2f7dc3",
  "name": {
    "formatted": "Theresa",
    "familyName": "Mateo",
    "givenName": "Leanne",
    "middleName": "Keyshawn",
    "honorificPrefix": "Petra",
    "honorificSuffix": "Donald"
  },
  "displayName": "WPMGMAPZZZOG",
  "nickName": "XRVKUQQEWTWM",
  "profileUrl": "MVFHUEKNWNOZ",
  "title": "KDDASUUSXZHZ",
  "userType": "KHPEVIFITFOF",
  "preferredLanguage": "en-SC",
  "locale": "PHSYYLKOBPWW",
  "timezone": "Africa/Lubumbashi",
  "active": true,
  "emails": [{ "value": "jackeline.graham@thompson.co.uk", "primary": true }],
  "phoneNumbers": [{ "value": "15-532-3301", "primary": true }],
  "addresses": [{
    "formatted": "HKHVBPXOTLPU",
    "streetAddress": "5603 Gusikowski Overpass",
    "locality": "QJQLYXFKNQES",
    "region": "BHJLXPTIQJRP",
    "postalCode": "ad39 0tr",
    "country": "Bahrain",
    "primary": true
  }],
  "id": "87993052-0210-42bf-b003-85d221ddb143",
  "userName": "gunner@rempel.info",
  "schemas": ["urn:ietf:params:scim:schemas:core:2.0:User"],
  "meta": {
    "resourceType": "User",
    "created": "2026-02-23T08:57:51.9641351Z",
    "lastModified": "2026-02-23T08:57:52.1363639Z",
    "location": "/scim/Users/87993052-0210-42bf-b003-85d221ddb143"
  }
}
```

**All 9 operations are correctly reflected in the response:**
- ✅ `timezone` = `"Africa/Lubumbashi"` (from operation 9)
- ✅ `emails[0].value` = `"jackeline.graham@thompson.co.uk"` (from operation 1)
- ✅ `phoneNumbers[0].value` = `"15-532-3301"` (from operation 2)
- ✅ All 6 address sub-attributes updated (from operations 3–8)
- ✅ All other scalar attributes updated (from operation 9)

### Step 4 — Validator reports failure despite correct response

The validator reports the 9 errors listed in the table above.

---

## Analysis: Why This Is a Validator Bug

### Bug #1 — timezone: Validator contradicts its own PATCH request

The validator sends `"timezone": "Africa/Lubumbashi"` in operation 9, then reports:

> `The value of timezone should be  instead of Africa/Lubumbashi`

The validator expects timezone to be an **empty string**, but it explicitly set it to `"Africa/Lubumbashi"`. This directly contradicts the PATCH request the validator itself generated.

Per [RFC 7644 Section 3.5.2.2 (Replace)](https://datatracker.ietf.org/doc/html/rfc7644#section-3.5.2.2):

> If "path" is omitted, the target is assumed to be the resource itself.
> In this case, the "value" attribute SHALL contain a list of one or more
> attributes that are to be replaced.

Each attribute in the value object is treated as an individual replace operation. The value object contains `"timezone": "Africa/Lubumbashi"`, so timezone **must** be `"Africa/Lubumbashi"` after the operation.

### Bug #2 — Multi-valued attributes incorrectly reported as "Missing"

The validator reports emails, phoneNumbers, and all address sub-attributes as "Missing from the fetched Resource" even though:

1. **Operations 1–8** explicitly update them via filtered-path replace
2. **The PATCH response** contains all values correctly
3. **Operation 9** (the no-path replace) does **not** contain `emails`, `phoneNumbers`, or `addresses` keys

Per RFC 7644 §3.5.2.2, a no-path replace only modifies the attributes present in the value object. Since multi-valued attributes are absent from operation 9's value object, they must not be cleared. The server correctly preserves the values set by operations 1–8.

### Root Cause Hypothesis

The validator appears to incorrectly compute the expected resource state when a single PATCH request combines:
- **Filtered-path operations** (ops 1–8) targeting multi-valued attribute sub-properties
- **A no-path replace operation** (op 9) containing only scalar attributes

The validator likely treats operation 9's no-path replace as a **full resource replacement** rather than an attribute-level replacement. This would:
- Wipe its expected state for `emails`, `phoneNumbers`, `addresses` (absent from the value object) — causing the "Missing" errors
- Miscalculate the expected `timezone` value — causing the `"should be  instead of Africa/Lubumbashi"` error

---

## Evidence of Reproducibility

This is consistent across multiple validation runs against the same SCIM server:

| Run | Date | timezone Error | Multi-valued "Missing" Errors |
|-----|------|----------------|-------------------------------|
| 05  | 2026-02-22 | Not present | 8 errors (same pattern) |
| 06  | 2026-02-22 | Not present | 8 errors (same pattern) |
| 07  | 2026-02-23 | **Yes** — `"should be  instead of Africa/Lubumbashi"` | 8 errors (same pattern) |

The timezone error in run 07 is the **definitive proof**: the validator itself sent the value and then rejected it.

Note: all other tests pass (18 passed, 1 failed) — the SCIM server implementation is fully compliant for all other scenarios including preview tests.

---

## Expected Behavior

After applying the 9 PATCH operations per RFC 7644 §3.5.2.2, the validator should:

1. **Accept** `timezone = "Africa/Lubumbashi"` as correct (matching what was sent in operation 9)
2. **Accept** that `emails`, `phoneNumbers`, and `addresses` are present and updated (by operations 1–8, and not cleared by operation 9 which does not mention them)
3. **Pass** the "Patch User - Replace Attributes" test

---

## Environment

- **Validator URL:** https://scimvalidator.microsoft.com/
- **Correlation ID:** `f11da8e0-85ed-4693-974a-925b7bdf8684`
- **Date:** 2026-02-23T08:57:51Z
- **SCIM Server:** Custom .NET 8 implementation via dev tunnel
- **All other tests:** 18 passed, 8 preview tests passed, 0 warnings

