﻿# Schema System Documentation

Comprehensive documentation for the SCIM schema system, including system overview, extensions, and models.

## 📖 Quick Navigation

### Getting Started
- **[System Overview](./system-overview.md)** - Schema system architecture
- **[Extension Guide](./extension-guide.md)** - Creating custom schema extensions
- **[Models Reference](./models-required-optional.md)** - Required and optional attributes

### Troubleshooting & Validation
- **[SCIM Validator Fix](./scim-validator-fix.md)** - Fix for Microsoft SCIM Validator JSON parsing errors
- **[Expected/Actual Pattern](./expected-actual-pattern.md)** - Data pattern explanation

---

## 🎯 Key Concepts

### Standard Schemas
- `urn:ietf:params:scim:schemas:core:2.0:User` - Core user schema
- `urn:ietf:params:scim:schemas:core:2.0:Group` - Core group schema

### Extension Schemas
- `urn:ietf:params:scim:schemas:extension:enterprise:2.0:User` - Enterprise user extension
- Custom enterprise-specific extensions

### Custom Schemas
- Application-specific schema extensions
- Type-safe custom attributes
- Full validation support

---

## 📦 Schema Components

### User Schema
```
Core Attributes:
- id (unique identifier)
- externalId (external system reference)
- userName (login identifier)
- name (given, family, formatted)
- displayName (human-readable name)
- emails (list with type)
- phoneNumbers (list with type)
- userType (standard or custom)
- active (enabled/disabled status)

Extensions:
- Enterprise User (department, manager, cost center)
- Custom attributes (application-specific)
```

### Group Schema
```
Core Attributes:
- id (unique identifier)
- displayName (group name)
- members (user references)
- externalId (external system reference)

Extensions:
- Custom attributes (application-specific)
```

---

## 💡 Common Tasks

### Adding a Custom Attribute
1. Define in schema configuration
2. Add to model class
3. Implement in repository
4. Test with filter examples

### Creating a Schema Extension
See [Extension Guide](./extension-guide.md)

### Validating Attributes
See [Models Reference](./models-required-optional.md)

---

## 📊 Attribute Classifications

### Required Attributes
- `id` - Unique identifier (auto-generated)
- `schemas` - List of applicable schemas
- `meta` - Metadata (created, modified, location)

### Recommended Attributes
- `externalId` - Reference to external system
- `displayName` - Human-readable name
- `active` - Enable/disable status

### Optional Attributes
- Application-specific attributes
- Extended attributes via extensions
- Complex type attributes

---

## 📖 Documentation Structure

- **system-overview.md** - Schema system architecture
- **extension-guide.md** - Creating custom extensions
- **models-required-optional.md** - Attribute requirements
- **expected-actual-pattern.md** - Data pattern explanation

---

## 🧪 Testing Schemas

```bash
# Test schema endpoints
GET /scim/Schemas
GET /scim/Schemas/urn:ietf:params:scim:schemas:core:2.0:User
POST /scim/Schemas (custom schema)
```

---

## 🔗 Related Documentation

- [User Model Details](./models-required-optional.md)
- [Repository Integration](../migration/quick-start-repository.md)
- [Filter System](../filters/overview.md)
- [API Reference](../../README.md)

---

## 📚 SCIM 2.0 Specification

- [RFC 7644 - User Model](https://tools.ietf.org/html/rfc7644#section-4.1)
- [RFC 7644 - Group Model](https://tools.ietf.org/html/rfc7644#section-4.2)
- [RFC 7643 - Schemas](https://tools.ietf.org/html/rfc7643)

---

**Status**: ✅ Complete  
**Last Updated**: February 21, 2026  
**SCIM Version**: 2.0

