## 📋 SCIM Filter Documentation - Complete Guide

The `filter` parameter in GetUsersAsync() and GetGroupsAsync() allows clients to query resources using SCIM filter expressions as defined in RFC 7644.

---

## Filter Syntax

### Basic Format
```
attributeName OPERATOR value
```

### Supported Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `eq` | Equal | `userName eq "john.doe"` |
| `ne` | Not equal | `userName ne "admin"` |
| `co` | Contains | `displayName co "John"` |
| `sw` | Starts with | `userName sw "john"` |
| `ew` | Ends with | `userName ew ".com"` |
| `gt` | Greater than | `id gt "5"` |
| `ge` | Greater than or equal | `id ge "5"` |
| `lt` | Less than | `id lt "10"` |
| `le` | Less than or equal | `id le "10"` |
| `pr` | Present (has value) | `email pr` |

### Logical Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `and` | AND condition | `active eq true and userName sw "admin"` |
| `or` | OR condition | `userName eq "john" or userName eq "jane"` |
| `not` | NOT condition | `not (active eq false)` |

---

## User Filter Examples

### Simple Equality Filters

```bash
# Get user with specific username
filter=userName eq "john.doe"

# Get active users only
filter=active eq true

# Get users with specific email
filter=emails.value eq "john@example.com"
```

### String Matching Filters

```bash
# Users whose name contains "John"
filter=displayName co "John"

# Users whose username starts with "admin"
filter=userName sw "admin"

# Users whose email ends with "@example.com"
filter=emails.value ew "@example.com"
```

### Presence Filters

```bash
# Users with a phone number
filter=phoneNumbers pr

# Users without a phone number
filter=not (phoneNumbers pr)

# Users with given name
filter=name.givenName pr
```

### Compound Filters

```bash
# Active users named John
filter=active eq true and displayName co "John"

# Admins or managers
filter=title eq "Admin" or title eq "Manager"

# Users with email at company domain who are active
filter=emails.value ew "@company.com" and active eq true

# Users not disabled
filter=not (active eq false)
```

### Advanced Examples

```bash
# Users with specific department AND role
filter=departmentName eq "Engineering" and jobTitle eq "Senior Developer"

# Users with multiple conditions
filter=(active eq true) and (department eq "Sales" or department eq "Marketing")

# Users with email from specific domain who are not disabled
filter=emails.value ew "@acme.com" and not (active eq false)

# All active users except admins
filter=active eq true and not (userName sw "admin")
```

---

## Group Filter Examples

### Simple Filters

```bash
# Get group with specific display name
filter=displayName eq "Engineering Team"

# Get group with specific ID
filter=id eq "group123"
```

### String Matching

```bash
# Groups whose name contains "Engineering"
filter=displayName co "Engineering"

# Groups that start with "Team"
filter=displayName sw "Team"

# Groups that end with "Committee"
filter=displayName ew "Committee"
```

### Compound Filters

```bash
# Groups in specific department with specific name pattern
filter=departmentName eq "Engineering" and displayName co "Team"

# Groups matching multiple name patterns
filter=displayName co "Engineering" or displayName co "Architecture"

# Groups with description that is present and matches pattern
filter=description pr and description co "sales"
```

---

## Implementation Examples

### C# - Using the Filter Parameter

```csharp
// Example 1: Get active users
var response = await repository.GetUsersAsync(
    filter: "active eq true",
    startIndex: 1,
    count: 50
);

// Example 2: Get users by email domain
var response = await repository.GetUsersAsync(
    filter: "emails.value ew \"@company.com\"",
    startIndex: 1,
    count: 50
);

// Example 3: Complex filter with multiple conditions
var response = await repository.GetUsersAsync(
    filter: "active eq true and (userName sw \"admin\" or userName sw \"root\")",
    startIndex: 1,
    count: 50
);
```

### HTTP Request Examples

#### Get Active Users
```http
GET /scim/Users?filter=active%20eq%20true HTTP/1.1
Authorization: Bearer <token>
```

#### Get Users by Email Domain
```http
GET /scim/Users?filter=emails.value%20ew%20%22%40example.com%22 HTTP/1.1
Authorization: Bearer <token>
```

#### Get Users with Complex Filter
```http
GET /scim/Users?filter=active%20eq%20true%20and%20displayName%20co%20%22John%22&startIndex=1&count=100 HTTP/1.1
Authorization: Bearer <token>
```

#### Get Groups by Name Pattern
```http
GET /scim/Groups?filter=displayName%20co%20%22Engineering%22 HTTP/1.1
Authorization: Bearer <token>
```

---

## PowerShell Examples

### Basic Filters

```powershell
# Get active users
$filter = 'active eq true'
$users = Invoke-RestMethod -Uri "https://localhost:7001/scim/Users?filter=$([uri]::EscapeDataString($filter))" -Headers $headers -Method Get

# Get users by email
$filter = 'emails.value ew "@company.com"'
$users = Invoke-RestMethod -Uri "https://localhost:7001/scim/Users?filter=$([uri]::EscapeDataString($filter))" -Headers $headers -Method Get

# Get active admins
$filter = 'active eq true and userName sw "admin"'
$users = Invoke-RestMethod -Uri "https://localhost:7001/scim/Users?filter=$([uri]::EscapeDataString($filter))" -Headers $headers -Method Get
```

### Groups

```powershell
# Get engineering groups
$filter = 'displayName co "Engineering"'
$groups = Invoke-RestMethod -Uri "https://localhost:7001/scim/Groups?filter=$([uri]::EscapeDataString($filter))" -Headers $headers -Method Get

# Get specific group
$filter = 'displayName eq "Development Team"'
$groups = Invoke-RestMethod -Uri "https://localhost:7001/scim/Groups?filter=$([uri]::EscapeDataString($filter))" -Headers $headers -Method Get
```

---

## cURL Examples

### URL Encoded Filters

```bash
# Get active users
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20true"

# Get users by email domain
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=emails.value%20ew%20%22%40company.com%22"

# Get users with multiple conditions
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=active%20eq%20true%20and%20displayName%20co%20%22John%22"

# Get engineering groups
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Groups?filter=displayName%20co%20%22Engineering%22"
```

---

## Common Attributes by Resource Type

### User Attributes

| Attribute | Type | Example Filter |
|-----------|------|-----------------|
| `id` | string | `id eq "user123"` |
| `userName` | string | `userName eq "john.doe"` |
| `displayName` | string | `displayName co "John"` |
| `active` | boolean | `active eq true` |
| `emails.value` | string | `emails.value ew "@company.com"` |
| `phoneNumbers.value` | string | `phoneNumbers.value eq "+1234567890"` |
| `name.givenName` | string | `name.givenName eq "John"` |
| `name.familyName` | string | `name.familyName eq "Doe"` |
| `userType` | string | `userType eq "Employee"` |

### Group Attributes

| Attribute | Type | Example Filter |
|-----------|------|-----------------|
| `id` | string | `id eq "group123"` |
| `displayName` | string | `displayName eq "Engineering"` |
| `description` | string | `description co "sales"` |

---

## Error Handling

### Invalid Filter Syntax

```http
HTTP/1.1 400 Bad Request

{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:Error"],
  "detail": "Invalid filter syntax: unexpected character",
  "status": 400
}
```

### Filter Complexity Limit

```http
HTTP/1.1 413 Request Entity Too Large

{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:Error"],
  "detail": "Filter expression too complex",
  "status": 413
}
```

---

## Best Practices

### 1. **Use Specific Attributes**
```bash
# ✅ GOOD - Specific
filter=userName eq "john.doe"

# ❌ BAD - Too broad
filter=displayName co "john"
```

### 2. **Combine with Pagination**
```bash
# Get page 1 of active users
filter=active%20eq%20true&startIndex=1&count=50

# Get page 2 of active users
filter=active%20eq%20true&startIndex=51&count=50
```

### 3. **URL Encode Special Characters**
```bash
# ✅ CORRECT - URL encoded
filter=emails.value%20ew%20%22%40company.com%22

# ❌ WRONG - Not encoded
filter=emails.value ew "@company.com"
```

### 4. **Logical Grouping**
```bash
# ✅ GOOD - Clear precedence
filter=(active%20eq%20true)%20and%20(title%20eq%20%22Manager%22%20or%20title%20eq%20%22Lead%22)

# ❌ AMBIGUOUS - Unclear precedence
filter=active%20eq%20true%20and%20title%20eq%20%22Manager%22%20or%20title%20eq%20%22Lead%22
```

---

## Implementation Notes

### Case Sensitivity
- Attribute names are **case-sensitive** per SCIM spec
- Values comparison is typically **case-insensitive** (implementation dependent)

### Null Values
- Use `pr` operator to check if attribute is present
- Use `not (attributeName pr)` to check if absent

### Pagination with Filters
- `startIndex` is 1-based
- Default count: 100
- Maximum recommended: 1000

---

## Response Format with Filters

```json
{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:ListResponse"],
  "totalResults": 150,
  "itemsPerPage": 50,
  "startIndex": 1,
  "Resources": [
    {
      "id": "user123",
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
        "created": "2024-01-15T10:00:00Z",
        "lastModified": "2024-01-25T14:30:00Z",
        "location": "/Users/user123"
      }
    }
  ]
}
```

---

## References

- [RFC 7644 - SCIM Filtering](https://tools.ietf.org/html/rfc7644#section-3.4.2)
- [SCIM Filter Specification](https://tools.ietf.org/html/rfc7644#section-3.4.2.2)

