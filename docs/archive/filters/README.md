# SCIM Filtering System

Complete documentation for the SCIM filtering system including operators, examples, and implementation details.

## 📖 Quick Navigation

### Getting Started
- **[Overview](./overview.md)** - Introduction to SCIM filtering
- **[Examples](./examples.md)** - Practical filtering examples
- **[Quick Reference](./value-types-quick-reference.md)** - Quick lookup for common filters

### Detailed Reference
- **[Reference](./reference.md)** - Complete operator reference
- **[Value Types](./value-types.md)** - Supported data types and operations
- **[Parser Implementation](./parser.md)** - How the parser works internally

### Advanced Topics
- **[Implementation Guide](./implementation-guide.md)** - Adding new filters
- **[Nested Filters](./nested-filters.md)** - Complex filter expressions
- **[Error Handling](./error-handling.md)** - Filter validation and errors
- **[URL Encoding](./url-encoding.md)** - Proper filter URL encoding

---

## 🎯 Filter Operators Supported

### Comparison Operators
| Operator | Meaning | Example |
|----------|---------|---------|
| `eq` | Equals | `userName eq "john.doe"` |
| `ne` | Not equals | `active ne false` |
| `co` | Contains | `displayName co "admin"` |
| `sw` | Starts with | `userName sw "john"` |
| `ew` | Ends with | `userName ew "@example.com"` |
| `pr` | Present (exists) | `email pr` |
| `gt` | Greater than | `created gt "2026-01-01"` |
| `ge` | Greater or equal | `created ge "2026-01-01"` |
| `lt` | Less than | `created lt "2026-12-31"` |
| `le` | Less or equal | `created le "2026-12-31"` |

### Logical Operators
| Operator | Meaning |
|----------|---------|
| `and` | AND operation (higher precedence) |
| `or` | OR operation (lower precedence) |
| `not` | NOT operation |

### Grouping
| Symbol | Meaning |
|--------|---------|
| `(...)` | Parentheses for grouping |

---

## 💡 Common Filter Examples

### Simple Filters
```
userName eq "john.doe@example.com"
active eq true
```

### Complex Combinations
```
(userName sw "admin" or displayName co "Admin") and active eq true
```

### Attribute Path Filters
```
emails.value eq "john.doe@example.com"
name.givenName eq "John"
```

---

## 📋 Documentation Structure

- **overview.md** - Introduction and concepts
- **reference.md** - Full operator reference documentation
- **examples.md** - Practical usage examples
- **value-types.md** - Data type support
- **value-types-quick-reference.md** - Quick lookup
- **parser.md** - Parser implementation details
- **implementation-guide.md** - How to add new filters
- **nested-filters.md** - Complex filter expressions
- **error-handling.md** - Error messages and validation
- **url-encoding.md** - URL encoding requirements

---

## 🧪 Testing

All filter operators are covered by unit tests:

```bash
# Run filter tests
dotnet test --filter "FullyQualifiedName~FilterTranslator"

# Run specific filter tests
dotnet test --filter "ScimUserFilterTranslatorTests"
dotnet test --filter "ScimGroupFilterTranslatorTests"
```

---

## 🔗 Related Documentation

- [SCIM 2.0 Specification](https://tools.ietf.org/html/rfc7644)
- [Filter Expression RFC 3986](https://tools.ietf.org/html/rfc3986)
- [Quick Start Guide](../guides/quickstart.md)

---

**Status**: ✅ Production Ready  
**Last Updated**: February 21, 2026  
**Test Coverage**: 100%

