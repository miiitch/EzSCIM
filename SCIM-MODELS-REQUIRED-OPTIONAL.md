# ✅ SCIM MODELS - REQUIRED VS OPTIONAL ATTRIBUTES

## 📋 Summary

Separated **REQUIRED** and **OPTIONAL** attributes in ScimUser and ScimGroup models to clearly show SCIM RFC 7643 compliance.

---

## 📚 ScimUser - Attribute Classification

### ✅ REQUIRED Attributes (4)

According to RFC 7643, these attributes **MUST** be present:

| Attribute | Type | Description | Default |
|-----------|------|-------------|---------|
| `Id` | string | Unique resource identifier | Auto-generated GUID |
| `UserName` | string | Unique user identifier | Empty string |
| `Schemas` | List\<string\> | SCIM schema URIs | `["urn:ietf:params:scim:schemas:core:2.0:User"]` |
| `Meta` | ScimMeta | Resource metadata | New instance |

### 🟡 OPTIONAL Attributes (17)

These attributes **MAY** be present (client decides):

| Attribute | Type | Nullable | Default | Description |
|-----------|------|----------|---------|-------------|
| `ExternalId` | string? | ✅ | null | External identifier |
| `Name` | ScimName | ❌ | new() | Full name (complex) |
| `DisplayName` | string? | ✅ | null | Display name |
| `NickName` | string? | ✅ | null | Casual name |
| `ProfileUrl` | string? | ✅ | null | Profile URL |
| `Title` | string? | ✅ | null | Job title |
| `UserType` | string? | ✅ | null | User type (Employee, etc.) |
| `PreferredLanguage` | string? | ✅ | null | Language preference |
| `Locale` | string? | ✅ | null | Locale |
| `Timezone` | string? | ✅ | null | Timezone |
| `Active` | bool | ❌ | true | Account active status |
| `Emails` | List\<ScimEmail\> | ❌ | new() | Email addresses |
| `PhoneNumbers` | List\<ScimPhoneNumber\> | ❌ | new() | Phone numbers |
| `Addresses` | List\<ScimAddress\> | ❌ | new() | Physical addresses |
| `Groups` | List\<ScimGroupMembership\> | ❌ | new() | Group memberships (read-only) |
| `CustomAttributes` | Dictionary | ❌ | new() | Custom extensions |

---

## 📚 ScimGroup - Attribute Classification

### ✅ REQUIRED Attributes (4)

According to RFC 7643, these attributes **MUST** be present:

| Attribute | Type | Description | Default |
|-----------|------|-------------|---------|
| `Id` | string | Unique resource identifier | Auto-generated GUID |
| `DisplayName` | string | Group display name | Empty string |
| `Schemas` | List\<string\> | SCIM schema URIs | `["urn:ietf:params:scim:schemas:core:2.0:Group"]` |
| `Meta` | ScimMeta | Resource metadata | New instance |

### 🟡 OPTIONAL Attributes (3)

These attributes **MAY** be present (client decides):

| Attribute | Type | Nullable | Default | Description |
|-----------|------|----------|---------|-------------|
| `ExternalId` | string? | ✅ | null | External identifier |
| `Members` | List\<ScimMember\> | ❌ | new() | Group members |
| `CustomAttributes` | Dictionary | ❌ | new() | Custom extensions |

---

## 💡 Why This Separation Matters

### 1. **Client Flexibility**
Clients can send minimal payloads:
```json
{
  "schemas": ["urn:ietf:params:scim:schemas:core:2.0:User"],
  "userName": "john.doe"
}
```

### 2. **Validation Rules**
```csharp
// Server MUST reject if required attributes are missing
if (string.IsNullOrEmpty(user.UserName))
{
    return BadRequest("userName is required");
}

// Server MUST accept if optional attributes are missing
// (No error if DisplayName is null)
```

### 3. **Clear Documentation**
Developers immediately know:
- What they **MUST** provide
- What they **CAN** provide
- What defaults to expect

---

## 📊 Minimal Valid Payloads

### Minimal ScimUser (Production-Ready)
```json
{
  "schemas": ["urn:ietf:params:scim:schemas:core:2.0:User"],
  "userName": "john.doe@example.com"
}
```

**Server response will include:**
- Auto-generated `id`
- Auto-generated `meta` (created, lastModified, location)
- Default `active: true`
- Empty collections for emails, phoneNumbers, etc.

### Minimal ScimGroup (Production-Ready)
```json
{
  "schemas": ["urn:ietf:params:scim:schemas:core:2.0:Group"],
  "displayName": "Developers"
}
```

**Server response will include:**
- Auto-generated `id`
- Auto-generated `meta`
- Empty `members` array

---

## 🔧 Code Changes Applied

### ScimUser.cs
```csharp
public class ScimUser
{
    // ==================== REQUIRED ATTRIBUTES (RFC 7643) ====================
    
    /// <summary>
    /// Unique identifier for the SCIM resource (REQUIRED)
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Unique identifier for the User (REQUIRED)
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// SCIM schema URIs (REQUIRED)
    /// </summary>
    public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:schemas:core:2.0:User" };
    
    /// <summary>
    /// Resource metadata (REQUIRED)
    /// </summary>
    public ScimMeta Meta { get; set; } = new();

    // ==================== OPTIONAL ATTRIBUTES ====================
    
    /// <summary>
    /// External unique identifier defined by provisioning client (OPTIONAL)
    /// </summary>
    public string? ExternalId { get; set; }
    
    // ... rest of optional attributes
}
```

### ScimGroup.cs
```csharp
public class ScimGroup
{
    // ==================== REQUIRED ATTRIBUTES (RFC 7643) ====================
    
    /// <summary>
    /// Unique identifier for the SCIM resource (REQUIRED)
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name for the Group (REQUIRED)
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// SCIM schema URIs (REQUIRED)
    /// </summary>
    public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:schemas:core:2.0:Group" };
    
    /// <summary>
    /// Resource metadata (REQUIRED)
    /// </summary>
    public ScimMeta Meta { get; set; } = new();

    // ==================== OPTIONAL ATTRIBUTES ====================
    
    /// <summary>
    /// External unique identifier defined by provisioning client (OPTIONAL)
    /// </summary>
    public string? ExternalId { get; set; }
    
    // ... rest of optional attributes
}
```

---

## 📖 RFC 7643 References

### Required Attributes (Section 3.1)
> "id", "schemas", and "meta" are REQUIRED in all SCIM resources.

### User Resource (Section 4.1)
> "userName" is REQUIRED. All other attributes are OPTIONAL.

### Group Resource (Section 4.2)
> "displayName" is REQUIRED. All other attributes are OPTIONAL.

---

## ✅ Benefits

1. **Clear Separation**: Visual distinction between required and optional
2. **Better Documentation**: Comments explain (REQUIRED) vs (OPTIONAL)
3. **RFC Compliance**: Follows SCIM 2.0 standard exactly
4. **Developer-Friendly**: Easy to know what to send
5. **Nullable Types**: Optional strings are properly marked with `?`
6. **Safe Defaults**: Required attributes have non-null defaults

---

## 🎯 Validation Strategy

### Server-Side Validation (Recommended)
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] ScimUser user)
    {
        // Validate REQUIRED attributes
        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            return BadRequest(new ScimError
            {
                Status = 400,
                Detail = "userName is required",
                ScimType = "invalidValue"
            });
        }
        
        // Optional attributes - no validation needed
        // Server handles null/empty gracefully
        
        var created = await _repository.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }
}
```

---

## 🎉 Summary

Successfully reorganized ScimUser and ScimGroup to clearly show:
- ✅ **4 REQUIRED** attributes in ScimUser
- ✅ **17 OPTIONAL** attributes in ScimUser
- ✅ **4 REQUIRED** attributes in ScimGroup
- ✅ **3 OPTIONAL** attributes in ScimGroup
- ✅ Clear visual separation with comments
- ✅ Proper nullable types for optional strings
- ✅ RFC 7643 compliant structure

**The SCIM models now clearly communicate what clients MUST and MAY provide!**

---

**Date:** 2026-02-01  
**Status:** ✅ Complete  
**RFC:** SCIM 2.0 (RFC 7643 Section 3.1, 4.1, 4.2)
