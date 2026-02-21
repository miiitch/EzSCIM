# Microsoft Entra (Azure AD) Integration Guide

This guide explains how to configure automatic provisioning of users and groups from Microsoft Entra (formerly Azure Active Directory) to your application via SCIM 2.0.

## 📋 Prerequisites

- Application registered in Microsoft Entra
- SCIM API deployed and reachable (HTTPS required for production)
- Bearer authentication token (recommended)

## 🔧 Configuration in Microsoft Entra

### Step 1: Go to Enterprise Applications

1. Sign in to the [Azure Portal](https://portal.azure.com)
2. Go to **Microsoft Entra ID** (formerly Azure Active Directory)
3. Click **Enterprise Applications**
4. Create a new application or select an existing one

### Step 2: Configure Provisioning

1. In your application, go to **Provisioning**
2. Click **Get started**
3. Set Provisioning Mode to **Automatic**

### Step 3: Configure Tenant URL and Authentication

#### Tenant URL
```text
https://your-server.com/scim
```

For local development with ngrok:
```text
https://your-subdomain.ngrok.io/scim
```

#### Secret Token - JWT Bearer Token

The SCIM API uses JWT Bearer tokens to secure endpoints. Here is how to generate and use a token:

##### Generate a Token (Development Only)

In development, call the test endpoint to obtain a valid token:

```bash
curl -X GET "https://localhost:7001/scim/auth/token"
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": "60 minutes"
}
```

**⚠️ NOTE:** This endpoint is available **ONLY in development** and returns HTTP 403 in production.

##### Use the Token in Requests

Include the token in the `Authorization` header:

```bash
curl -X GET "https://your-server.com/scim/Users" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

##### Configure the Token in Entra

1. Generate a JWT token with the secret configured in production
2. Copy the full token (including the `Bearer ` prefix)
3. In Microsoft Entra, go to **Provisioning → Admin Credentials**
4. Paste the token into the **Secret Token** field
5. Click **Test Connection**

##### Security in Production

- JWT tokens are generated with an HS256 secret key stored in **Azure Key Vault**
- Tokens expire after 60 minutes by default
- Each integration should have its own unique secret key
- Secret keys must NEVER be committed to source control

##### JWT Format (Development)

Minimal payload:
```json
{
  "sub": "scim-client",
  "jti": "unique-id",
  "exp": 1234567890
}
```

No additional claims are required. Validation only checks the signature and expiration.

### Step 4: Test the Connection

1. Click **Test Connection**
2. Azure will call the following endpoints:
   - `GET /scim/ServiceProviderConfig`
   - `GET /scim/Schemas`
   - `GET /scim/Users?filter=userName eq "testuser@domain.com"`

If everything is correct, you will see a success message ✅

### Step 5: Configure Attribute Mappings

#### User Mappings

Recommended mappings:

| Azure AD Attribute | SCIM Attribute | Type |
|--------------------|----------------|------|
| userPrincipalName  | userName       | Direct |
| objectId           | externalId     | Direct |
| displayName        | displayName    | Direct |
| givenName          | name.givenName | Direct |
| surname            | name.familyName | Direct |
| mail               | emails[type eq "work"].value | Direct |
| accountEnabled     | active         | Direct |
| jobTitle           | title          | Direct |
| department         | urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:department | Direct |
| employeeId         | urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:employeeNumber | Direct |

#### Group Mappings

| Azure AD Attribute | SCIM Attribute | Type |
|--------------------|----------------|------|
| objectId           | externalId     | Direct |
| displayName        | displayName    | Direct |
| members            | members        | Direct |

### Step 6: Set Provisioning Scope

Choose who will be provisioned:

1. **Sync only assigned users and groups** (recommended)
   - You manually assign the users/groups to be synchronized
   
2. **Sync all users and groups**
   - All users in the directory will be synchronized

### Step 7: Start Provisioning

1. Save the configuration
2. Enable provisioning
3. Click **Start provisioning**

The initial sync cycle will start (can take 20–40 minutes).

## 🔄 Provisioning Cycles

### Initial Sync
- Duration: 20–40 minutes (depends on number of users)
- Microsoft Entra reads all users/groups in scope
- Compares with existing users in your SCIM API
- Creates/updates the necessary users

### Incremental Sync
- Frequency: Every 40 minutes by default
- Synchronizes only changes since the last sync

## 🔎 SCIM Operations Used by Microsoft Entra

### 1. User Existence Check
```http
GET /scim/Users?filter=userName eq "user@domain.com"
```
Microsoft Entra checks if the user exists before creating it.

### 2. User Creation
```http
POST /scim/Users
Content-Type: application/scim+json

{
  "schemas": [
    "urn:ietf:params:scim:schemas:core:2.0:User",
    "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User"
  ],
  "userName": "john.doe@domain.com",
  "externalId": "azure-object-id-123",
  "name": {
    "givenName": "John",
    "familyName": "Doe"
  },
  "active": true,
  "emails": [
    {
      "value": "john.doe@domain.com",
      "type": "work",
      "primary": true
    }
  ]
}
```

### 3. User Update (PATCH)
```http
PATCH /scim/Users/{id}
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    {
      "op": "replace",
      "path": "active",
      "value": false
    }
  ]
}
```

### 4. Group Existence Check
```http
GET /scim/Groups?filter=displayName eq "GroupName"
```

### 5. Group Creation
```http
POST /scim/Groups
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:schemas:core:2.0:Group"],
  "displayName": "Administrators",
  "externalId": "azure-group-id-456",
  "members": [
    {
      "value": "user-id-123",
      "display": "John Doe"
    }
  ]
}
```

### 6. Add Member to Group
```http
PATCH /scim/Groups/{groupId}
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    {
      "op": "add",
      "path": "members",
      "value": [
        {
          "value": "user-id-123"
        }
      ]
    }
  ]
}
```

### 7. Remove Member from Group
```http
PATCH /scim/Groups/{groupId}
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    {
      "op": "remove",
      "path": "members[value eq \"user-id-123\"]"
    }
  ]
}
```

### 8. Deactivate User
```http
PATCH /scim/Users/{id}
Content-Type: application/scim+json

{
  "schemas": ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
  "Operations": [
    {
      "op": "replace",
      "path": "active",
      "value": false
    }
  ]
}
```

### 9. Delete User
```http
DELETE /scim/Users/{id}
```

## 📊 Monitoring and Logs

### Provisioning Logs in Azure

1. Go to your enterprise application
2. Click on **Provisioning logs**
3. You will see all operations:
   - ✅ Success
   - ⚠️ Warnings
   - ❌ Failures

### Expected Response Codes

| Code | Meaning | Azure Action |
|------|---------|--------------|
| 200 | OK | Operation successful |
| 201 | Created | Resource created successfully |
| 204 | No Content | Deletion successful |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource already exists (duplicate userName/displayName) |
| 500 | Server Error | Internal error, Azure will retry |

## 🔐 Security

### Production
For production, implement:

1. **HTTPS mandatory**
2. **Bearer Token JWT Authentication**
3. **Rate limiting** to prevent abuse
4. **Strict input validation**
5. **Audit logging** of all operations

### Example of Bearer Authentication

```csharp
// In Program.cs
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://login.microsoftonline.com/{tenant-id}";
        options.Audience = "api://your-api-id";
    });

// Add [Authorize] to your controllers
[Authorize]
[ApiController]
[Route("scim/[controller]")]
public class UsersController : ControllerBase
```

## 🐛 Troubleshooting

### Error: "Test connection failed"

**Possible causes:**
1. Incorrect tenant URL
2. Invalid authentication token
3. API not reachable (firewall, HTTPS required)
4. Non-compliant SCIM endpoints

**Solutions:**
- Check the URL: must end with `/scim`
- Test manually with Postman/curl
- Check your API logs

### Error: "userName already exists" (409)

**Cause:** A user with the same `userName` already exists.

**Solution:** Microsoft Entra will use PATCH to update the existing user.

### Error: "Schema not found"

**Cause:** Custom schemas are not loaded correctly.

**Solution:** Check `appsettings.Scim.json` and the logs on startup.

### Users are not syncing

**Possible causes:**
1. Users not in the provisioning scope
2. Incorrect attribute mappings
3. Restrictive group/user filters

**Solutions:**
- Check user assignment to the application
- Review provisioning logs
- Check scope rules

## 📚 References

- [Microsoft SCIM Documentation](https://learn.microsoft.com/en-us/azure/active-directory/app-provisioning/use-scim-to-provision-users-and-groups)
- [SCIM 2.0 Specification (RFC 7644)](https://datatracker.ietf.org/doc/html/rfc7644)
- [SCIM Protocol (RFC 7644)](https://datatracker.ietf.org/doc/html/rfc7644)
- [SCIM Schema (RFC 7643)](https://datatracker.ietf.org/doc/html/rfc7643)

## 🎯 Production Checklist

Before deploying to production, ensure that:

- [ ] HTTPS is enabled with a valid certificate
- [ ] Bearer Token JWT Authentication is implemented
- [ ] Rate limiting is configured
- [ ] Audit logs are enabled
- [ ] Persistent database (replace InMemory)
- [ ] Load testing is completed
- [ ] Backup and restore are configured
- [ ] Monitoring and alerts are in place
- [ ] API documentation is updated
- [ ] End-to-end tests with Azure passed successfully
