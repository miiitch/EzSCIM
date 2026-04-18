## SCIM Nested Filters - Logical Grouping

SCIM filters fully support nested logical expressions with parentheses.

---

## Nested filter syntax

### Simple

```text
condition1 and condition2
```

### Grouped

```text
(condition1 and condition2) or (condition3 and condition4)
```

### Deeply nested

```text
((condition1 and condition2) or condition3) and (condition4 or condition5)
```

---

## Logical operators

| Operator | Meaning | Precedence |
|---|---|---|
| `and` | Logical AND | Higher |
| `or` | Logical OR | Lower |
| `not` | Negation | High |
| `()` | Grouping | Explicit precedence |

---

## User filter examples

### Example 1

```text
filter=active eq true and userName sw "admin"
filter=(active eq true) and (userName sw "admin")
```

### Example 2

```text
filter=active eq true and (title eq "Admin" or title eq "Manager")
```

### Example 3

```text
filter=(active eq true and (title eq "Admin" or title eq "Manager")) and emails.value ew "@company.com"
```

### Example 4

```text
filter=((active eq true and (title eq "Admin" or title eq "Manager")) or (active eq false and title eq "Director")) and departmentName eq "Engineering"
```

### Example 5

```text
filter=active eq true and not (userName sw "admin")
```

### Example 6

```text
filter=active eq true and not (title eq "Admin" or title eq "Manager")
```

### Example 7

```text
filter=emails.value ew "@acme.com" or emails.value ew "@bigcorp.com" or emails.value ew "@startup.io"
```

### Example 8

```text
filter=((emails.value ew "@company.com" or emails.value co "@company-") and (active eq true or title eq "Manager")) and not (userName sw "admin")
```

---

## URL-encoded examples

```text
filter=active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22%29
```

```text
filter=active%20eq%20true%20and%20not%20%28userName%20sw%20%22admin%22%29
```

```text
filter=%28%28active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22%29%29%20or%20%28active%20eq%20false%20and%20title%20eq%20%22Director%22%29%29%20and%20departmentName%20eq%20%22Engineering%22
```

---

## Validation tips

- Start with the non-encoded filter and validate it first.
- Add parentheses explicitly, even when precedence would already work.
- Encode only at request time.
- Keep complex filters in test fixtures for repeatability.

---

## Related docs

- [`overview.md`](./overview.md)
- [`url-encoding.md`](./url-encoding.md)
- [`value-types.md`](./value-types.md)
