# TODO - JWT Authentication Next Steps

## Before production deployment

### 1. Integration testing
- [ ] Run `dotnet test` and verify all tests pass
- [ ] Validate authentication script flow (PowerShell/shell)
- [ ] Generate a development token and inspect it on [jwt.io](https://jwt.io)
- [ ] Test API calls with Postman or Insomnia

### 2. Azure Key Vault configuration
- [ ] Create or reuse an Azure Key Vault instance
- [ ] Create `Jwt-SecretKey` (32+ characters)
- [ ] Configure Managed Identity for the application
- [ ] Validate Key Vault access (`az keyvault secret show --name Jwt-SecretKey`)
- [ ] Confirm logs show secret loading from Key Vault

### 3. Entra ID configuration
- [ ] Generate a valid JWT signed with the configured secret
- [ ] Verify Entra **Test Connection** succeeds
- [ ] Verify endpoints return 401 without token
- [ ] Verify endpoints return 200 with valid token

### 4. Security
- [ ] Ensure tokens are never logged
- [ ] Ensure secret key is not stored in plaintext (except local dev)
- [ ] Configure appropriate token expiration
- [ ] Add monitoring for authentication failures
- [ ] Enforce HTTPS in production

### 5. Documentation
- [ ] Document token generation process for operations teams
- [ ] Document secret rotation process
- [ ] Document authentication troubleshooting steps
- [ ] Update deployment runbook

## Future optimizations

### 1. Token improvements
- [ ] Add additional claims (`aud`, `iss`)
- [ ] Implement refresh tokens
- [ ] Add token revocation list (blacklist)
- [ ] Add token-level rate limiting

### 2. Scalability
- [ ] Implement token validation caching
- [ ] Add token metrics (Prometheus/Application Insights)
- [ ] Add circuit breaker behavior for Key Vault access

### 3. Multi-tenant authentication
- [ ] Support separate secrets per tenant
- [ ] Add tenant identifier claims
- [ ] Enforce tenant-level data isolation

### 4. Advanced integrations
- [ ] Support direct Entra-issued tokens
- [ ] Implement full OAuth2 flow
- [ ] Support multiple identity providers

## Troubleshooting - common issues

### Issue: token invalid in production

```text
Symptom: HTTP 401 for requests that should be valid
Cause: Secret mismatch between token generation and token validation
Action:
  1. Verify the active secret in Key Vault
  2. Regenerate JWT with the correct secret
  3. Validate the token on jwt.io using that same secret
```

### Issue: Key Vault not accessible

```text
Symptom: Application startup fails, Key Vault access errors in logs
Cause: Managed Identity not configured or missing permissions
Action:
  1. Verify Managed Identity is assigned to the app
  2. Verify secret exists in Key Vault
  3. Verify Key Vault access policy (`get`, `list`)
  4. Verify Key Vault URI in configuration
```

### Issue: `/scim/auth/token` returns 403

```text
Symptom: Production request returns Forbidden
Cause: Expected behavior; endpoint is development-only
Action: Generate token from secure tooling (CLI/script/service)
```

## Security checklist

Before production rollout:

- [ ] JWT uses HS256 with 32+ character secret
- [ ] Secret key is never committed to Git
- [ ] Secret key is stored in Azure Key Vault
- [ ] Managed Identity is configured
- [ ] Tokens expire in a reasonable window (60 min recommended)
- [ ] HTTPS redirection is enabled
- [ ] Authentication is enabled on all SCIM endpoints
- [ ] `/scim/auth/token` returns 403 in production
- [ ] Authentication logs are enabled for audit
- [ ] Authentication errors are logged without leaking sensitive details
- [ ] Rate limiting is configured or planned
- [ ] TLS certificates are valid and up to date

## Monitoring

Track at least:

1. **Authentication failures**
   - Number of 401 responses
   - Failure reasons (invalid, expired, missing token)

2. **Token generation**
   - Number of tokens issued per day
   - Usage of `/scim/auth/token` in development

3. **Performance**
   - Token validation latency
   - Key Vault access latency

4. **Security**
   - Alerts for abnormal auth patterns (for example spikes in 401)
   - Tracking of repeated invalid token attempts

## Implementation references

- `EzSCIM/Services/JwtTokenService.cs`
- `EzSCIM/Authentication/JwtBearerTokenAuthenticationHandler.cs`
- `EzSCIM/Controllers/ScimConfigController.cs` (`/scim/auth/token`)
- `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`
- `docs/auth/setup.md`
