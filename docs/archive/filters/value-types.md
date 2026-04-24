## SCIM Filter Values - Supported Value Types

This document describes value types you can use on the right side of binary operators in SCIM filters.

---

## Supported value types

### 1) String

**Format:**

```text
attributeName OPERATOR "string value"
```

**Typical operators:** `eq`, `ne`, `co`, `sw`, `ew`

**Examples:**

```text
filter=userName eq "john.doe"
filter=displayName co "John"
filter=userName sw "admin"
filter=emails.value ew "@company.com"
filter=userType ne "Guest"
filter=displayName eq "O'Connor"
filter=displayName eq "John Doe"
```

**Notes:**
- Attribute names are matched by SCIM attribute name.
- String comparison behavior depends on translator implementation and target store.

---

### 2) Boolean

**Format:**

```text
attributeName OPERATOR true|false
```

**Typical operators:** `eq`, `ne`

**Examples:**

```text
filter=active eq true
filter=active eq false
filter=active ne true
filter=emailVerified eq true
```

**Notes:**
- Booleans are not quoted.
- Use lowercase `true` / `false`.

---

### 3) Numeric

**Format:**

```text
attributeName OPERATOR number
```

**Typical operators:** `eq`, `ne`, `gt`, `ge`, `lt`, `le`

**Examples:**

```text
filter=id eq 12345
filter=id gt 1000
filter=id le 9999
filter=salary gt 50000
filter=memberCount ge 5
```

**Notes:**
- Numbers are not quoted.
- Integer and decimal values are supported depending on attribute type.

---

### 4) DateTime

**Format:**

```text
attributeName OPERATOR "2024-01-15T10:00:00Z"
```

**Typical operators:** `eq`, `ne`, `gt`, `ge`, `lt`, `le`

**Examples:**

```text
filter=meta.created gt "2024-01-15T10:00:00Z"
filter=meta.lastModified lt "2024-06-01T00:00:00Z"
```

**Notes:**
- Use RFC 3339 / ISO-8601 timestamps.
- Keep values quoted.

---

### 5) Presence (`pr`)

**Format:**

```text
attributeName pr
```

**Examples:**

```text
filter=phoneNumbers pr
filter=description pr
filter=not (manager pr)
```

**Notes:**
- `pr` has no right-hand value.

---

## Summary table

| Type | Format | Typical operators | Example |
|---|---|---|---|
| String | `"value"` | `eq`, `ne`, `co`, `sw`, `ew` | `"john.doe"` |
| Boolean | `true` / `false` | `eq`, `ne` | `true` |
| Numeric | `number` | `eq`, `ne`, `gt`, `ge`, `lt`, `le` | `1000` |
| DateTime | `"2024-01-15T10:00:00Z"` | `eq`, `ne`, `gt`, `ge`, `lt`, `le` | `"2024-01-15T10:00:00Z"` |
| Presence | `(none)` | `pr` | `phoneNumbers pr` |

---

## Mixed expression examples

```text
filter=active eq true and userName sw "admin"
filter=(active eq true or title eq "Manager") and meta.created gt "2024-01-01T00:00:00Z"
filter=emails.value ew "@company.com" and not (phoneNumbers pr)
```

---

## Related docs

- [`value-types-quick-reference.md`](./value-types-quick-reference.md)
- [`nested-filters.md`](./nested-filters.md)
- [`url-encoding.md`](./url-encoding.md)
