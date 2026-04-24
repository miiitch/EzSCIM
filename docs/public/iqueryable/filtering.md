# SCIM Filter Syntax

SCIM filters allow clients to query resources with attribute-based conditions
as defined in [RFC 7644 §3.4.2](https://tools.ietf.org/html/rfc7644#section-3.4.2).

---

## Basic syntax

```
GET /scim/Users?filter=<expression>
GET /scim/Groups?filter=<expression>&startIndex=1&count=50
```

```
attributeName OPERATOR value
```

Values must be **URL-encoded** when using HTTP. String values use double quotes.

---

## Comparison operators

| Operator | Meaning | Example |
|---|---|---|
| `eq` | Equal | `userName eq "john.doe"` |
| `ne` | Not equal | `userName ne "admin"` |
| `co` | Contains | `displayName co "John"` |
| `sw` | Starts with | `userName sw "john"` |
| `ew` | Ends with | `emails.value ew "@acme.com"` |
| `gt` | Greater than | `id gt "5"` |
| `ge` | Greater than or equal | `id ge "5"` |
| `lt` | Less than | `id lt "10"` |
| `le` | Less than or equal | `id le "10"` |
| `pr` | Present (has value) | `phoneNumbers pr` |

## Logical operators

| Operator | Example |
|---|---|
| `and` | `active eq true and userName sw "admin"` |
| `or` | `userName eq "john" or userName eq "jane"` |
| `not` | `not (active eq false)` |

---

## Examples

### User filters

```bash
# Exact match on userName (Entra ID lookup pattern)
filter=userName eq "jane.doe@acme.com"

# Active users only
filter=active eq true

# Users whose email ends with domain
filter=emails.value ew "@acme.com"

# Active users in engineering
filter=active eq true and title eq "Engineer"

# Users with a work email who are active
filter=emails.value ew "@company.com" and active eq true

# Users with given name present
filter=name.givenName pr

# Complex with grouping
filter=(active eq true) and (title eq "Manager" or title eq "Lead")

# All active users except service accounts
filter=active eq true and not (userName sw "svc-")
```

### Group filters

```bash
# Exact group name (Entra ID lookup pattern)
filter=displayName eq "Engineering Team"

# Groups containing "Admin" in the name
filter=displayName co "Admin"

# Groups starting with "Team"
filter=displayName sw "Team"
```

---

## Pagination

```bash
# First page, 50 items
GET /scim/Users?filter=active%20eq%20true&startIndex=1&count=50

# Second page
GET /scim/Users?filter=active%20eq%20true&startIndex=51&count=50
```

- `startIndex` is **1-based** (not 0-based)
- Default `count`: 100
- Maximum recommended: 1000

---

## URL encoding

Filters must be URL-encoded in HTTP requests. Common encodings:

| Character | Encoded |
|---|---|
| space | `%20` |
| `"` | `%22` |
| `@` | `%40` |
| `(` | `%28` |
| `)` | `%29` |

```bash
# Original:  filter=emails.value ew "@acme.com" and active eq true
# Encoded:   filter=emails.value%20ew%20%22%40acme.com%22%20and%20active%20eq%20true

curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:7001/scim/Users?filter=emails.value%20ew%20%22%40acme.com%22%20and%20active%20eq%20true"
```

PowerShell automatic encoding:

```powershell
$filter = 'active eq true and userName sw "admin"'
$uri = "/scim/Users?filter=$([uri]::EscapeDataString($filter))"
Invoke-RestMethod -Uri "https://localhost:7001$uri" -Headers $headers
```

---

## Response format

```json
{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:ListResponse"],
  "totalResults": 150,
  "itemsPerPage": 50,
  "startIndex": 1,
  "Resources": [
    {
      "id": "user-001",
      "userName": "jane.doe@acme.com",
      "displayName": "Jane Doe",
      "active": true,
      "emails": [
        { "value": "jane.doe@acme.com", "type": "work", "primary": true }
      ],
      "meta": {
        "resourceType": "User",
        "created": "2024-01-15T10:00:00Z",
        "lastModified": "2024-03-01T09:00:00Z",
        "location": "/scim/Users/user-001"
      }
    }
  ]
}
```

---

## Error responses

### Invalid filter syntax — `400 Bad Request`

```json
{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:Error"],
  "detail": "Invalid filter syntax: unexpected character at position 12",
  "status": 400
}
```

---

## How filters map to your entity

EzSCIM uses `[ScimProperty]` annotations to resolve SCIM attribute names to C# property names.

```csharp
public class Employee
{
    [ScimProperty("userName", "string")]
    public string Email { get; set; }      // SCIM: userName → C#: Email

    [ScimProperty("active", "boolean")]
    public bool IsEnabled { get; set; }    // SCIM: active → C#: IsEnabled
}
```

```
SCIM filter:  active eq true and userName co "@acme.com"
LINQ:         e => e.IsEnabled == true && e.Email.Contains("@acme.com")
SQL:          WHERE IsEnabled = 1 AND Email LIKE '%@acme.com%'
```

---

## Implementation notes

- String comparisons via `co`, `sw`, `ew` are **case-insensitive** by default (SQL `LIKE`)
- Attribute names are **case-sensitive** per SCIM spec (`userName` ≠ `UserName`)
- `pr` generates `.IsNotNull()` / `.IsNotEmpty()` in LINQ
- Nested attributes (e.g. `name.givenName`, `emails.value`) require the property's
  `[ScimProperty]` name to include the dot notation (`"name.givenName"`)

---

**Next**: [SCIM 2.0 attribute reference →](./scim-attributes.md) | [Schema extensions →](./schema-extensions.md)

