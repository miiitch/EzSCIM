# SCIM Validator

## Overview

Microsoft provides a SCIM validator at https://scimvalidator.microsoft.com/
that runs a standardized suite of tests against a SCIM endpoint.
Validation results are stored in `docs/scim-test-results/` as `scim-results-XX.json`.

---

## Running the validator

### Prerequisites

1. A publicly accessible SCIM endpoint (use DevTunnel — see [development-setup.md](./development-setup.md))
2. A valid JWT token

### Steps

1. Start the Demo API with Aspire and DevTunnel exposure
2. Copy the public DevTunnel URL (e.g. `https://abc123.devtunnels.ms`)
3. Generate a JWT token: `GET https://abc123.devtunnels.ms/scim/auth/token`
4. Go to https://scimvalidator.microsoft.com
5. Enter:
   - **Tenant URL**: `https://abc123.devtunnels.ms/scim`
   - **Secret Token**: the JWT token (without `Bearer ` prefix)
6. Click **Test** and download the JSON results
7. Save as `docs/scim-test-results/scim-results-XX.json` (increment XX)

---

## Storing results

```
docs/scim-test-results/
├── scim-results-01.json
├── scim-results-02.json
├── ...
└── scim-results-XX.json    ← latest
```

Each file is the raw JSON export from the validator.

---

## Compliance process

For every **failure** reported by the validator:

1. Identify the failing test name in the JSON (e.g. `"Patch User - Replace Attributes"`)
2. Create a regression test in `EzSCIM.IntegrationTests/ScimValidatorComplianceTests.cs`
3. The test must **fail before** the fix is applied
4. Fix the root cause
5. Verify the test passes

---

## Known issues

### Patch User — Replace Attributes (runs 05, 06, 07)

**Validator test**: `Patch User - Replace Attributes`  
**Status**: Known validator bug — not an EzSCIM defect  
**Detail**: See [issues/scim-validator-bug-patch-replace-attributes.md](./issues/scim-validator-bug-patch-replace-attributes.md)

The validator incorrectly computes the expected state when a PATCH request mixes:
- Filtered-path operations: `"path": "emails[type eq \"work\"].value"`
- No-path operations: `"path": "emails"`

The validator's expected state diverges from RFC 7644 §3.5.2 behavior. EzSCIM
follows the RFC. This failure appears consistently in runs 05, 06, and 07 and is
not actionable.

---

## Interpreting results

Key fields in the result JSON:

```json
{
  "testResults": [
    {
      "testName": "Get Users",
      "status": "Passed",
      "runs": [...]
    },
    {
      "testName": "Patch User - Replace Attributes",
      "status": "Failed",
      "runs": [
        {
          "runId": "06",
          "status": "Failed",
          "errorMessage": "Expected emails[0].value to be ...",
          "request": { ... },
          "response": { ... }
        }
      ]
    }
  ]
}
```

- `status: "Passed"` — all runs passed
- `status: "Failed"` — one or more runs failed
- For each failing run: check `errorMessage`, `request`, and `response`

---

**See also**: [testing.md](./testing.md) | [issues/](./issues/)

