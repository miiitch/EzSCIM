# SCIM 2.0 Attribute Reference

Complete reference of SCIM 2.0 standard attributes as defined in
[RFC 7643](https://tools.ietf.org/html/rfc7643).

---

## User attributes (`urn:ietf:params:scim:schemas:core:2.0:User`)

### Core attributes

| Attribute | Type | Multi | Required | Description |
|---|---|---|---|---|
| `id` | string | No | R/O | Server-assigned unique identifier |
| `externalId` | string | No | No | Identifier set by the provisioning client (e.g. Entra Object ID) |
| `userName` | string | No | **Yes** | Unique login name (e.g. `jane.doe@acme.com`) |
| `displayName` | string | No | No | Full display name |
| `nickName` | string | No | No | Informal name |
| `profileUrl` | string | No | No | URL to user's profile |
| `title` | string | No | No | Job title (e.g. `Senior Engineer`) |
| `userType` | string | No | No | User category (e.g. `Employee`, `Contractor`) |
| `preferredLanguage` | string | No | No | ISO 639-1 language code (e.g. `en-US`) |
| `locale` | string | No | No | Locale (e.g. `en-US`) |
| `timezone` | string | No | No | IANA timezone (e.g. `America/New_York`) |
| `active` | boolean | No | No | Account enabled/disabled |

### Name sub-attributes (`name`)

| Attribute | Type | Description |
|---|---|---|
| `name.formatted` | string | Full name, formatted |
| `name.familyName` | string | Last / family name |
| `name.givenName` | string | First / given name |
| `name.middleName` | string | Middle name |
| `name.honorificPrefix` | string | Prefix (e.g. `Dr.`, `Ms.`) |
| `name.honorificSuffix` | string | Suffix (e.g. `Jr.`, `III`) |

### Multi-valued attributes

#### `emails`

| Sub-attribute | Type | Description |
|---|---|---|
| `emails.value` | string | Email address |
| `emails.type` | string | `work`, `home`, `other` |
| `emails.primary` | boolean | `true` for the primary address |
| `emails.display` | string | Display label |

#### `phoneNumbers`

| Sub-attribute | Type | Description |
|---|---|---|
| `phoneNumbers.value` | string | Phone number |
| `phoneNumbers.type` | string | `work`, `home`, `mobile`, `fax`, `other` |
| `phoneNumbers.primary` | boolean | Primary phone |

#### `addresses`

| Sub-attribute | Type | Description |
|---|---|---|
| `addresses.formatted` | string | Full address formatted |
| `addresses.streetAddress` | string | Street address |
| `addresses.locality` | string | City |
| `addresses.region` | string | State / province |
| `addresses.postalCode` | string | Postal code |
| `addresses.country` | string | ISO 3166-1 alpha-2 country code |
| `addresses.type` | string | `work`, `home`, `other` |
| `addresses.primary` | boolean | Primary address |

#### `ims` (Instant messaging)

| Sub-attribute | Type | Description |
|---|---|---|
| `ims.value` | string | IM handle |
| `ims.type` | string | `aim`, `gtalk`, `icq`, `xmpp`, `msn`, `skype`, `qq`, `yahoo` |

#### `photos`

| Sub-attribute | Type | Description |
|---|---|---|
| `photos.value` | string | URL of the photo |
| `photos.type` | string | `photo`, `thumbnail` |

#### `groups` (read-only)

| Sub-attribute | Type | Description |
|---|---|---|
| `groups.value` | string | Group ID |
| `groups.display` | string | Group display name |
| `groups.type` | string | `direct`, `indirect` |

#### `roles`

| Sub-attribute | Type | Description |
|---|---|---|
| `roles.value` | string | Role value |
| `roles.display` | string | Role label |
| `roles.primary` | boolean | Primary role |

#### `entitlements`

| Sub-attribute | Type | Description |
|---|---|---|
| `entitlements.value` | string | Entitlement value |
| `entitlements.display` | string | Entitlement label |

#### `x509Certificates`

| Sub-attribute | Type | Description |
|---|---|---|
| `x509Certificates.value` | string | Base64-encoded DER certificate |

---

## Group attributes (`urn:ietf:params:scim:schemas:core:2.0:Group`)

| Attribute | Type | Multi | Required | Description |
|---|---|---|---|---|
| `id` | string | No | R/O | Server-assigned unique identifier |
| `externalId` | string | No | No | Identifier set by the provisioning client |
| `displayName` | string | No | **Yes** | Group name |

### `members`

| Sub-attribute | Type | Description |
|---|---|---|
| `members.value` | string | Member user ID |
| `members.display` | string | Member display name |
| `members.type` | string | `User` or `Group` |
| `members.$ref` | string | Resource URI of the member |

---

## Enterprise User extension (`urn:ietf:params:scim:schemas:extension:enterprise:2.0:User`)

Commonly used by Entra ID provisioning.

| Attribute | Type | Description |
|---|---|---|
| `employeeNumber` | string | Employee ID |
| `costCenter` | string | Cost center code |
| `organization` | string | Organization name |
| `division` | string | Division name |
| `department` | string | Department name |
| `manager.value` | string | Manager's user ID |
| `manager.displayName` | string | Manager's display name |
| `manager.$ref` | string | Manager's resource URI |

PATCH example from Entra ID:

```json
{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    {
      "op": "Replace",
      "path": "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:department",
      "value": "Engineering"
    },
    {
      "op": "Replace",
      "path": "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:employeeNumber",
      "value": "EMP-12345"
    }
  ]
}
```

---

## Microsoft Entra ID required SCIM fields

For Microsoft Entra ID provisioning, keep your SCIM schema aligned with RFC 7643 and
configure mappings so Entra can reliably create, match, and update resources.

### Validator-focused checklist

Before running the Microsoft SCIM Validator, verify these baseline conditions:

- `User.userName` is present and unique
- `User.active` is mapped for lifecycle operations
- `Group.displayName` is present
- Server-managed attributes (`id`, `schemas`, `meta`) are returned correctly
- Entra mappings are saved for both create and update flows

### User (`urn:ietf:params:scim:schemas:core:2.0:User`)

| Attribute | Typical Entra usage | Notes |
|---|---|---|
| `userName` | Required for matching/provisioning | Must be unique and stable over time |
| `active` | Required for enable/disable lifecycle | Used for soft deprovisioning (`false`) |
| `name.givenName` | Commonly mapped | Recommended for profile completeness |
| `name.familyName` | Commonly mapped | Recommended for profile completeness |
| `emails.value` | Commonly mapped | Strongly recommended for most enterprise mappings |
| `externalId` | Commonly used for correlation | Client-defined identifier for cross-system matching |

### Group (`urn:ietf:params:scim:schemas:core:2.0:Group`)

| Attribute | Typical Entra usage | Notes |
|---|---|---|
| `displayName` | Required for group provisioning | Group display name in target app |
| `externalId` | Commonly used for correlation | Stable link to source directory object |

### Example model classes for Entra provisioning

```csharp
using EzSCIM.Attributes;
using EzSCIM.Constants;

public class EntraScimUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ScimProperty(ScimAttributeNames.User.UserName, "string", Required = true, Uniqueness = "server")]
    public string UserName { get; set; } = string.Empty;

    [ScimProperty(ScimAttributeNames.User.Active, "boolean")]
    public bool Active { get; set; } = true;

    [ScimProperty(ScimAttributeNames.User.NameGivenName, "string")]
    public string GivenName { get; set; } = string.Empty;

    [ScimProperty(ScimAttributeNames.User.NameFamilyName, "string")]
    public string FamilyName { get; set; } = string.Empty;

    [ScimProperty(ScimAttributeNames.User.EmailsValue, "string")]
    public string Email { get; set; } = string.Empty;

    [ScimProperty(ScimAttributeNames.Common.ExternalId, "string")]
    public string? ExternalId { get; set; }
}

public class EntraScimGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ScimProperty(ScimAttributeNames.Group.DisplayName, "string", Required = true)]
    public string DisplayName { get; set; } = string.Empty;

    [ScimProperty(ScimAttributeNames.Common.ExternalId, "string")]
    public string? ExternalId { get; set; }
}
```

The example intentionally uses a flattened `Email` property for readability. In production,
map it to the SCIM `emails.value` contract used by your Entra attribute mappings.

### Quick links

- Microsoft Entra SCIM provisioning reference: [Tutorial: Develop a SCIM endpoint for user provisioning to apps from Microsoft Entra ID](https://learn.microsoft.com/entra/identity/app-provisioning/use-scim-to-provision-users-and-groups)
- Microsoft SCIM Validator: [https://scimvalidator.microsoft.com/](https://scimvalidator.microsoft.com/)
- SCIM Core Schema (RFC 7643): [https://www.rfc-editor.org/rfc/rfc7643](https://www.rfc-editor.org/rfc/rfc7643)
- SCIM Protocol (RFC 7644): [https://www.rfc-editor.org/rfc/rfc7644](https://www.rfc-editor.org/rfc/rfc7644)

---

## `meta` attributes (read-only)

Returned on every resource. Set automatically by EzSCIM.

| Attribute | Type | Description |
|---|---|---|
| `meta.resourceType` | string | `User` or `Group` |
| `meta.created` | dateTime | ISO 8601 creation timestamp |
| `meta.lastModified` | dateTime | ISO 8601 last modification timestamp |
| `meta.location` | string | Canonical URL of the resource |
| `meta.version` | string | ETag version (optional) |

---

## SCIM data types

| SCIM type | C# type | Notes |
|---|---|---|
| `string` | `string` | UTF-8 text |
| `boolean` | `bool` | `true` / `false` |
| `integer` | `int` / `long` | Whole number |
| `decimal` | `decimal` / `double` | Floating point |
| `dateTime` | `DateTime` / `DateTimeOffset` | ISO 8601 (`2024-01-15T10:00:00Z`) |
| `binary` | `byte[]` | Base64-encoded |
| `reference` | `string` | URI reference |
| `complex` | class | Nested object |

---

## Attribute mutability

| Mutability | Description |
|---|---|
| `readOnly` | Cannot be set by the client (e.g. `id`, `meta`) |
| `readWrite` | Can be read and written (default) |
| `immutable` | Can only be set on creation |
| `writeOnly` | Set by client, never returned |

---

## Filter-friendly attributes

The most commonly filtered attributes:

```bash
filter=userName eq "jane.doe@acme.com"       # Entra ID: lookup before create
filter=externalId eq "azure-object-id-abc"   # Entra ID: lookup by Azure Object ID
filter=active eq true                         # Active users only
filter=emails.value ew "@acme.com"           # By email domain
filter=displayName co "Engineering"          # Groups by name pattern
```

---

**Next**: [Schema extensions →](./schema-extensions.md) | [Filtering →](./filtering.md)

