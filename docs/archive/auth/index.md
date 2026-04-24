# JWT Bearer Authentication - Index

## Overview

This implementation secures the SCIM API with JWT Bearer authentication and supports Azure Key Vault in production.

**Status:** Production ready

---

## Start here

### For developers
- [`setup.md`](./setup.md) - setup and runtime configuration
- [`pre-production-checklist.md`](./pre-production-checklist.md) - operational checklist

### For operators / DevOps
- [`setup.md`](./setup.md) - key vault and production settings
- [`pre-production-checklist.md`](./pre-production-checklist.md) - rollout checklist

---

## Documentation map

| Document | Audience | Purpose |
|---|---|---|
| `setup.md` | Developers / Ops | JWT and Key Vault setup |
| `index.md` | Everyone | Entry point and architecture |
| `pre-production-checklist.md` | Ops / Security | Deployment and hardening checklist |

---

## Scripts and verification

### Authentication tests

```bash
dotnet test
```

### Manual token usage (development)

```bash
curl https://localhost:7001/scim/auth/token
curl -H "Authorization: Bearer <token>" https://localhost:7001/scim/Users
```

---

## Architecture

```text
Client
  -> Authorization: Bearer <token>
  -> JwtBearer authentication handler
  -> Authorized SCIM controllers (/scim/Users, /scim/Groups, /scim/Schemas)
```

### Main components

- `EzSCIM/Services/JwtTokenService.cs`
- `EzSCIM/Authentication/JwtBearerTokenAuthenticationHandler.cs`
- `EzSCIM/Controllers/ScimConfigController.cs` (`/scim/auth/token` for development use)

---

## Capabilities

- Minimal JWT claims support
- HS256 signing
- Azure Key Vault secret loading in production
- Dev token endpoint (`/scim/auth/token`) for local testing
- Protected SCIM endpoints
- Authentication-friendly test infrastructure

---

## Security profile

### Development
- Local secret from development config
- Token endpoint available for local validation

### Production
- Secret loaded from Azure Key Vault
- Managed Identity support
- `/scim/auth/token` disabled (returns 403)
- HTTPS enforced

---

## Dependencies (high-level)

- `System.IdentityModel.Tokens.Jwt`
- `Microsoft.IdentityModel.Tokens`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Azure.Identity`
- `Azure.Security.KeyVault.Secrets`

---

## FAQ

### How do I generate tokens in production?
Use secure tooling (service/CLI) that reads the active secret from Key Vault and signs JWTs with the expected claims and lifetime.

### Why protect `ServiceProviderConfig`?
This repository uses a strict security posture and applies authentication broadly across SCIM endpoints.

### Can token lifetime be changed?
Yes. Update token generation settings and keep validator/client expectations aligned.

### Can Entra-issued JWTs be used directly?
Possible, depending on your trust model and token validation configuration.

---

## Related documentation

- [`setup.md`](./setup.md)
- [`pre-production-checklist.md`](./pre-production-checklist.md)
- [`../tests/entra-integration.md`](../tests/entra-integration.md)
- [`../README.md`](../README.md)
