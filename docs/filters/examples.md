## 🚀 Quick Filter Examples - Copy & Paste Ready

Use these examples to test filters immediately:

---

## User Filters

### Test with cURL

```bash
# Get active users
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20true"

# Get user by username
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=userName%20eq%20%22john.doe%22"

# Get users whose name contains "John"
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=displayName%20co%20%22John%22"

# Get users whose username starts with "admin"
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=userName%20sw%20%22admin%22"

# Get users whose email ends with "@company.com"
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=emails.value%20ew%20%22%40company.com%22"

# Get active users with "John" in their name
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20true%20and%20displayName%20co%20%22John%22"

# Get all inactive users
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20false"
```

### Test with PowerShell

```powershell
$token = "YOUR_TOKEN_HERE"
$baseUrl = "https://localhost:7001"
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/scim+json"
}

# Get active users
$filter = "active eq true"
$url = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
$response.Resources | ConvertTo-Json

# Get user by username
$filter = 'userName eq "john.doe"'
$url = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
$response.Resources | ConvertTo-Json

# Get users whose name contains "John"
$filter = 'displayName co "John"'
$url = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
$response.Resources | ConvertTo-Json

# Get users by email domain
$filter = 'emails.value ew "@company.com"'
$url = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
$response.Resources | ConvertTo-Json

# Complex filter: active users who are admins
$filter = 'active eq true and userName sw "admin"'
$url = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
$response.Resources | ConvertTo-Json
```

### Test from C#

```csharp
var repository = new InMemoryScimRepository(...);

// Get active users
var activeUsers = await repository.GetUsersAsync("active eq true");
Console.WriteLine($"Found {activeUsers.TotalResults} active users");

// Get user by username
var user = await repository.GetUsersAsync("userName eq \"john.doe\"");
if (user.TotalResults > 0)
{
    Console.WriteLine($"User: {user.Resources[0].DisplayName}");
}

// Get users with email from specific domain
var domainUsers = await repository.GetUsersAsync("emails.value ew \"@company.com\"");
Console.WriteLine($"Found {domainUsers.TotalResults} users from company domain");

// Complex: active admins
var admins = await repository.GetUsersAsync("active eq true and userName sw \"admin\"");
Console.WriteLine($"Found {admins.TotalResults} active admins");
```

---

## Group Filters

### Test with cURL

```bash
# Get group by name
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Groups?filter=displayName%20eq%20%22Engineering%20Team%22"

# Get groups that contain "Engineering"
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Groups?filter=displayName%20co%20%22Engineering%22"

# Get groups that start with "Team"
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Groups?filter=displayName%20sw%20%22Team%22"

# Get groups that end with "Committee"
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Groups?filter=displayName%20ew%20%22Committee%22"
```

### Test with PowerShell

```powershell
$token = "YOUR_TOKEN_HERE"
$baseUrl = "https://localhost:7001"
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/scim+json"
}

# Get group by exact name
$filter = 'displayName eq "Engineering Team"'
$url = "$baseUrl/scim/Groups?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
$response.Resources | ConvertTo-Json

# Get all engineering groups
$filter = 'displayName co "Engineering"'
$url = "$baseUrl/scim/Groups?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
$response.Resources | ConvertTo-Json

# Get all team groups
$filter = 'displayName sw "Team"'
$url = "$baseUrl/scim/Groups?filter=$([uri]::EscapeDataString($filter))"
$response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
$response.Resources | ConvertTo-Json
```

### Test from C#

```csharp
var repository = new InMemoryScimRepository(...);

// Get specific group
var group = await repository.GetGroupsAsync("displayName eq \"Engineering Team\"");
if (group.TotalResults > 0)
{
    Console.WriteLine($"Group: {group.Resources[0].DisplayName}");
}

// Get all engineering groups
var engGroups = await repository.GetGroupsAsync("displayName co \"Engineering\"");
Console.WriteLine($"Found {engGroups.TotalResults} engineering groups");

// Get team groups
var teamGroups = await repository.GetGroupsAsync("displayName sw \"Team\"");
Console.WriteLine($"Found {teamGroups.TotalResults} team groups");
```

---

## Full Request/Response Examples

### Request: Get Active Users

```http
GET /scim/Users?filter=active%20eq%20true&startIndex=1&count=50 HTTP/1.1
Host: localhost:7001
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGc...
Content-Type: application/scim+json
```

### Response: Active Users

```json
{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:ListResponse"],
  "totalResults": 15,
  "itemsPerPage": 50,
  "startIndex": 1,
  "Resources": [
    {
      "id": "user1",
      "userName": "john.doe",
      "displayName": "John Doe",
      "active": true,
      "emails": [
        {
          "value": "john.doe@company.com",
          "type": "work",
          "primary": true
        }
      ],
      "meta": {
        "resourceType": "User",
        "created": "2024-01-01T00:00:00Z",
        "lastModified": "2024-01-25T10:00:00Z",
        "location": "/Users/user1"
      }
    },
    {
      "id": "user2",
      "userName": "jane.smith",
      "displayName": "Jane Smith",
      "active": true,
      "emails": [
        {
          "value": "jane.smith@company.com",
          "type": "work",
          "primary": true
        }
      ],
      "meta": {
        "resourceType": "User",
        "created": "2024-01-02T00:00:00Z",
        "lastModified": "2024-01-24T15:00:00Z",
        "location": "/Users/user2"
      }
    }
  ]
}
```

---

## Quick Reference

### Most Used Filters

| Use Case | Filter | URL |
|----------|--------|-----|
| Get active users | `active eq true` | `?filter=active%20eq%20true` |
| Get inactive users | `active eq false` | `?filter=active%20eq%20false` |
| Get user by username | `userName eq "john"` | `?filter=userName%20eq%20%22john%22` |
| Get users from domain | `emails.value ew "@company.com"` | `?filter=emails.value%20ew%20%22%40company.com%22` |
| Find name | `displayName co "John"` | `?filter=displayName%20co%20%22John%22` |
| Admin users | `userName sw "admin"` | `?filter=userName%20sw%20%22admin%22` |
| Get group | `displayName eq "Team A"` | `?filter=displayName%20eq%20%22Team%20A%22` |

---

## Common Mistakes to Avoid

### ❌ NOT URL Encoded
```bash
# DON'T DO THIS - Will fail
curl "https://localhost:7001/scim/Users?filter=active eq true"
```

### ✅ Properly URL Encoded
```bash
# DO THIS - Will work
curl "https://localhost:7001/scim/Users?filter=active%20eq%20true"
```

### ❌ Missing Quotes Around Values
```bash
# DON'T DO THIS
filter=userName eq john.doe
```

### ✅ Proper Quotes
```bash
# DO THIS
filter=userName eq "john.doe"
```

### ❌ Wrong Operator
```bash
# DON'T DO THIS - "contains" is not valid
filter=displayName contains "John"
```

### ✅ Correct Operator
```bash
# DO THIS - "co" is the correct operator
filter=displayName co "John"
```

---

## Test All at Once

### PowerShell Script to Test All Filters

```powershell
# test-all-filters.ps1
$token = "YOUR_TOKEN_HERE"
$baseUrl = "https://localhost:7001"
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/scim+json"
}

$filters = @(
    "active eq true",
    'userName eq "john.doe"',
    'displayName co "John"',
    'emails.value ew "@company.com"',
    'displayName eq "Engineering Team"'
)

foreach ($filter in $filters) {
    Write-Host "`n=== Testing filter: $filter ===" -ForegroundColor Green
    $url = "$baseUrl/scim/Users?filter=$([uri]::EscapeDataString($filter))"
    try {
        $response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
        Write-Host "Total Results: $($response.totalResults)"
        Write-Host "Items: $($response.itemsPerPage)"
        if ($response.Resources.Count -gt 0) {
            Write-Host "First item: $($response.Resources[0].userName ?? $response.Resources[0].displayName)"
        }
    }
    catch {
        Write-Host "Error: $_" -ForegroundColor Red
    }
}
```

---

## Notes

- Filters are **case-sensitive** for attribute names
- Values comparison is **case-insensitive** (default behavior)
- Use **pagination** with filters for large result sets
- Always **URL encode** special characters in the filter

---

See **SCIM-FILTER-DOCUMENTATION.md** for complete reference.
See **FILTER-IMPLEMENTATION-GUIDE.md** for implementation details.
