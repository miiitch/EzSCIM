## URL Encoding for SCIM Filters

When you pass filters in URLs, special characters must be encoded.

---

## Characters to encode

| Character | Encoded | Typical usage |
|---|---|---|
| ` ` (space) | `%20` | Separation between tokens |
| `(` | `%28` | Open parenthesis |
| `)` | `%29` | Close parenthesis |
| `"` | `%22` | Quoted values |
| `@` | `%40` | Email values |

---

## Encoding examples

### Example 1: simple filter

Without encoding:

```text
active eq true
```

Encoded:

```text
active%20eq%20true
```

### Example 2: quoted value

Without encoding:

```text
userName eq "john.doe"
```

Encoded:

```text
userName%20eq%20%22john.doe%22
```

### Example 3: email domain

Without encoding:

```text
emails.value ew "@company.com"
```

Encoded:

```text
emails.value%20ew%20%22%40company.com%22
```

### Example 4: grouped expression

Without encoding:

```text
active eq true and (title eq "Admin" or title eq "Manager")
```

Encoded:

```text
active%20eq%20true%20and%20%28title%20eq%20%22Admin%22%20or%20title%20eq%20%22Manager%22%29
```

---

## Tools for encoding

### Online
- https://www.urlencoder.org/
- https://www.urldecoder.org/

### PowerShell

```powershell
[uri]::EscapeDataString("active eq true")
[uri]::EscapeDataString('userName eq "john.doe"')
```

### cURL (automatic)

```bash
curl --get --data-urlencode 'filter=active eq true' \
  "https://localhost:7001/scim/Users" \
  -H "Authorization: Bearer $TOKEN"
```

### Python

```python
from urllib.parse import quote
quote('active eq true')
quote('userName eq "john.doe"')
```

### JavaScript

```javascript
encodeURIComponent('active eq true');
encodeURIComponent('userName eq "john.doe"');
```

---

## Best practices

- Prefer `--data-urlencode` with `curl`.
- Keep raw filter strings readable in tests; encode at request time.
- Log decoded and encoded forms during troubleshooting.

---

## Related docs

- [`nested-filters.md`](./nested-filters.md)
- [`value-types.md`](./value-types.md)
- [`overview.md`](./overview.md)
