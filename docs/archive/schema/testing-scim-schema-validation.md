# SCIM Schema Validation Testing Guide

## Quick Test: Verify Schema JSON Structure

### Option 1: Using PowerShell (Windows)

```powershell
# Set your API URL and token
$apiUrl = "https://your-api.com"
$token = "your-bearer-token"

# Test single schema endpoint
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/scim+json"
}

$response = Invoke-WebRequest -Uri "$apiUrl/scim/Schemas/urn:ietf:params:scim:schemas:core:2.0:User" `
    -Headers $headers -Method GET

Write-Host "Status Code: $($response.StatusCode)"
Write-Host "Response:"
$response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10

# Test schemas list endpoint
$response = Invoke-WebRequest -Uri "$apiUrl/scim/Schemas" `
    -Headers $headers -Method GET

Write-Host "`nList Response:"
$response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 5
```

### Option 2: Using cURL

```bash
# Set variables
API_URL="https://your-api.com"
TOKEN="your-bearer-token"

# Test single schema
curl -X GET "$API_URL/scim/Schemas/urn:ietf:params:scim:schemas:core:2.0:User" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/scim+json" | jq .

# Test schemas list
curl -X GET "$API_URL/scim/Schemas" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/scim+json" | jq .
```

### Option 3: Using Postman

1. Create a new GET request to `{API_URL}/scim/Schemas/urn:ietf:params:scim:schemas:core:2.0:User`
2. Add header: `Authorization: Bearer {your-token}`
3. Add header: `Content-Type: application/scim+json`
4. Send request
5. Verify response is a valid JSON object (not an array)

## Validation Checklist

### Single Schema Response (`/scim/Schemas/{id}`)

✅ Response is a **JSON object** (not an array)
✅ Has `id` property (lowercase)
✅ Has `name` property
✅ Has `description` property
✅ Has `schemas` array
✅ Has `attributes` array
✅ Has `meta` object with:
  - `resourceType: "Schema"`
  - `location: "https://..."`

Example:
```json
{
  "id": "urn:ietf:params:scim:schemas:core:2.0:User",
  "name": "User",
  "description": "...",
  "schemas": ["urn:ietf:params:scim:schemas:core:2.0:Schema"],
  "attributes": [...],
  "meta": {
    "resourceType": "Schema",
    "location": "https://your-api.com/scim/Schemas/urn%3Aietf%3Aparams%3Ascim%3Aschemas%3Acore%3A2.0%3AUser"
  }
}
```

### Schemas List Response (`/scim/Schemas`)

✅ Response is a **JSON object** (not an array)
✅ Has `schemas` array with `["urn:ietf:params:scim:api:messages:2.0:ListResponse"]`
✅ Has `totalResults` integer
✅ Has `startIndex` integer
✅ Has `itemsPerPage` integer
✅ Has `resources` array with schema objects
✅ Each resource in `resources` has `meta` object

Example:
```json
{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:ListResponse"],
  "totalResults": 2,
  "startIndex": 1,
  "itemsPerPage": 2,
  "resources": [
    {
      "id": "urn:ietf:params:scim:schemas:core:2.0:User",
      "name": "User",
      "schemas": [...],
      "attributes": [...],
      "meta": {...}
    },
    {
      "id": "urn:ietf:params:scim:schemas:core:2.0:Group",
      "name": "Group",
      "schemas": [...],
      "attributes": [...],
      "meta": {...}
    }
  ]
}
```

## Testing with Microsoft SCIM Validator

1. Go to https://scimvalidator.microsoft.com/
2. Enter your API endpoint (e.g., `https://your-api.com/scim`)
3. Configure authentication:
   - Authentication Type: Bearer Token
   - Token: Your valid JWT token
4. Click "Start Validation"
5. The validator should now successfully process schema endpoints without errors

### Expected Success

The validator should show:
- ✅ User Schema validation passed
- ✅ Group Schema validation passed
- ✅ ServiceProviderConfig validation passed

### If Still Getting Errors

1. Verify all endpoints return proper JSON objects (not arrays)
2. Verify `meta` property is included in schema responses
3. Check that camelCase naming is applied consistently
4. Ensure no null values are being serialized when they should be omitted

## Running Unit Tests

```powershell
# Run schema serialization tests
dotnet test EzSCIM.UnitTests/EzSCIM.UnitTests.csproj -k SchemaJsonSerializationTests -v normal

# Run all tests
dotnet test --logger:trx --results-directory TestResults
```

## Files Modified

- `EzSCIM/Models/ScimSchema.cs` — Added Meta property and JSON attributes
- `EzSCIM/Models/ScimSchemaAttribute.cs` — Added JSON serialization attributes  
- `EzSCIM/Models/ScimMeta.cs` — Added JSON ignore conditions
- `EzSCIM/Controllers/ScimConfigController.cs` — Updated endpoints to return SCIM-compliant responses
- `EzSCIM.UnitTests/SchemaJsonSerializationTests.cs` — New unit tests

## References

- **SCIM 2.0 Specification**: https://tools.ietf.org/html/rfc7643
- **Microsoft SCIM Validator**: https://scimvalidator.microsoft.com/
- **SCIM 2.0 Resources**: https://scim.cloud/

