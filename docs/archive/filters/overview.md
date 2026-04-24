# Supported SCIM Filters

This document describes SCIM operators and filter patterns supported by the API, including Microsoft Entra-compatible query patterns.

## Logical operators

### `and`
All conditions must match.

```text
userName eq "john.doe@example.com" and active eq true
name.givenName co "John" and name.familyName co "Doe"
```

### `or`
At least one condition must match.

```text
userName eq "john.doe@example.com" or userName eq "jane.doe@example.com"
displayName co "Admin" or displayName co "Manager"
```

### `not`
Negates a condition.

```text
not (active eq false)
not (userName sw "test")
```

### Complex expressions
Use parentheses to control grouping and precedence.

```text
(userName sw "john" or userName sw "jane") and active eq true
```

## Comparison operators

### `eq` (equal)
Strict equality.

```text
userName eq "john.doe@example.com"
externalId eq "12345"
active eq true
```

### `ne` (not equal)
Inequality.

### `co` (contains)
Substring match.

```text
userName co "doe"
displayName co "Admin"
name.givenName co "John"
```

### `sw` (starts with)
Prefix match.

```text
userName sw "john"
externalId sw "EXT-"
displayName sw "Test"
```

### `ew` (ends with)
Suffix match.

```text
userName ew "@example.com"
```

### `pr` (present)
Attribute exists and is not empty.

```text
userName pr
displayName pr
externalId pr
```

### `gt`, `ge`, `lt`, `le`
Numeric and date comparisons.

## Supported user attributes

### Core attributes
- `userName`
- `externalId`
- `displayName`
- `active`

### Name attributes (`name.*`)
- `name.givenName`
- `name.familyName`
- `name.formatted`
- `name.middleName`
- `name.honorificPrefix`
- `name.honorificSuffix`

### Complex/multi-valued attributes
- `emails[type eq "work"].value`
- `emails.value`
- `phoneNumbers.value`
- `addresses.streetAddress`

### Other common attributes
- `title`
- `userType`
- `preferredLanguage`
- `locale`
- `timezone`

## Supported group attributes

- `displayName`
- `externalId`
- `members.value`

## Notes and limits

- Attribute names follow SCIM naming conventions.
- Nested filters are supported using standard SCIM syntax.
- URL-encode filters when calling endpoints over HTTP.

## Related docs

- [`nested-filters.md`](./nested-filters.md)
- [`url-encoding.md`](./url-encoding.md)
- [`value-types.md`](./value-types.md)
- [`reference.md`](./reference.md)
