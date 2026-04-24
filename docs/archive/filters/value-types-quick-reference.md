## Quick Reference - Filter Value Types

**Q: What value types can be used on the right side of SCIM operators?**

**A: Five common categories**

---

## Value type overview

### 1) String

```text
filter=userName eq "john.doe"
filter=displayName co "John"
filter=emails.value ew "@company.com"
```

- Quoted: `"..."`
- Common operators: `eq`, `ne`, `co`, `sw`, `ew`

---

### 2) Boolean

```text
filter=active eq true
filter=active eq false
filter=emailVerified eq true
```

- Not quoted
- Use lowercase `true` / `false`
- Common operators: `eq`, `ne`

---

### 3) Numeric

```text
filter=id eq 12345
filter=id gt 1000
filter=salary le 50000
```

- Not quoted
- Integer or decimal
- Common operators: `eq`, `ne`, `gt`, `ge`, `lt`, `le`

---

### 4) DateTime

```text
filter=meta.created gt "2024-01-15T10:00:00Z"
filter=meta.lastModified lt "2024-06-01T00:00:00Z"
```

- Quoted
- ISO-8601 format recommended
- Common operators: `eq`, `ne`, `gt`, `ge`, `lt`, `le`

---

### 5) Presence

```text
filter=phoneNumbers pr
filter=not (manager pr)
filter=description pr
```

- No explicit value
- Operator: `pr`

---

## Summary table

| Type | Format | Operators | Example |
|---|---|---|---|
| String | `"value"` | `eq`, `ne`, `co`, `sw`, `ew` | `"john.doe"` |
| Boolean | `true` / `false` | `eq`, `ne` | `true` |
| Numeric | `number` | `eq`, `ne`, `gt`, `ge`, `lt`, `le` | `1000` |
| DateTime | `"2024-01-15T10:00:00Z"` | `eq`, `ne`, `gt`, `ge`, `lt`, `le` | `"2024-01-15T10:00:00Z"` |
| Presence | `(none)` | `pr` | `phoneNumbers pr` |

---

## Operator examples by type

### String

```text
eq: userName eq "john.doe"
ne: userName ne "admin"
co: displayName co "John"
sw: userName sw "admin"
ew: emails.value ew "@company.com"
```

### Boolean

```text
eq: active eq true
ne: active ne false
```

### Numeric

```text
eq: id eq 12345
ne: id ne 0
gt: id gt 1000
ge: id ge 1000
lt: id lt 9999
le: salary le 50000
```

### DateTime

```text
eq: meta.created eq "2024-01-15T10:00:00Z"
gt: meta.created gt "2024-01-01T00:00:00Z"
lt: meta.lastModified lt "2024-12-31T23:59:59Z"
```

### Presence

```text
pr: phoneNumbers pr
not pr: not (manager pr)
```

---

## Related docs

- [`value-types.md`](./value-types.md)
- [`url-encoding.md`](./url-encoding.md)
- [`nested-filters.md`](./nested-filters.md)
