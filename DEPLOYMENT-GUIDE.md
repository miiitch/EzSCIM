# Deployment & Testing Guide - SCIM API Fixes

## Overview

This guide provides step-by-step instructions to build, test, and deploy the SCIM API with error fixes.

## Pre-Requisites

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or JetBrains Rider
- SCIM compliance test tool (e.g., Microsoft Entra ID SCIM tester)

---

## Build Instructions

### 1. Clean Build

```powershell
cd C:\Users\MichelPerfetti\src\private\scimwork

# Clean previous builds
dotnet clean EzSCIM/EzSCIM.csproj
dotnet clean EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj

# Restore dependencies
dotnet restore
```

### 2. Build the Core Library

```powershell
# Build release version
dotnet build EzSCIM/EzSCIM.csproj -c Release

# Verify build succeeded
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}
```

### 3. Build Demo Application

```powershell
# Build demo SCIM API
dotnet build EzSCIM.EntraID.Demo/EzSCIM.EntraID.Demo.csproj -c Release

# Verify
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Demo build successful" -ForegroundColor Green
}
```

---

## Unit & Integration Testing

### Run All Tests

```powershell
# Run all unit tests
dotnet test EzSCIM.UnitTests/EzSCIM.UnitTests.csproj -v minimal

# Run all integration tests
dotnet test EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj -v minimal
```

### Test Filter Operations

```powershell
# Test SCIM filter parsing
dotnet test EzSCIM.UnitTests/EzSCIM.UnitTests.csproj -k "Filter" -v normal
```

### Expected Test Results

```
Total Tests: 62+ (approximate)
Expected Results After Fixes:
✅ User API Tests: PASS
✅ Group API Tests: PASS
✅ Filter Tests: PASS
✅ Patch Operations: PASS
✅ ExcludedAttributes: PASS
✅ Error Messages: PASS (all English)
```

---

## Local Deployment

### 1. Run Demo Application Locally

```powershell
# Start the demo SCIM API (local development)
cd EzSCIM.EntraID.Demo
dotnet run

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
# Now listening on: https://localhost:5001
# Now listening on: http://localhost:5000
```

### 2. Verify API Health

```powershell
# In a new PowerShell window
$response = Invoke-WebRequest -Uri "https://localhost:5001/scim/.well-known/scim-configuration" `
    -SkipCertificateCheck
    
Write-Host "Status: $($response.StatusCode)"
```

### 3. Test Error Messages (English)

```powershell
# Try creating duplicate group (should show English error)
$headers = @{
    "Authorization" = "Bearer <token>"
    "Content-Type" = "application/scim+json"
}

$body = @{
    displayName = "TestGroup"
    schemas = @("urn:ietf:params:scim:schemas:core:2.0:Group")
} | ConvertTo-Json

# First creation - should succeed
$response1 = Invoke-WebRequest -Uri "https://localhost:5001/scim/Groups" `
    -Method POST `
    -Headers $headers `
    -Body $body `
    -SkipCertificateCheck

# Second creation - should fail with English message
try {
    $response2 = Invoke-WebRequest -Uri "https://localhost:5001/scim/Groups" `
        -Method POST `
        -Headers $headers `
        -Body $body `
        -SkipCertificateCheck
} catch {
    $error = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "Error Message: $($error.detail)"
    # Should show: "Group already exists" (not "Groupe existe déjà")
}
```

### 4. Test ExcludedAttributes

```powershell
# Create a group first
$groupBody = @{
    displayName = "TestGroup"
    schemas = @("urn:ietf:params:scim:schemas:core:2.0:Group")
} | ConvertTo-Json

$groupResponse = Invoke-WebRequest -Uri "https://localhost:5001/scim/Groups" `
    -Method POST `
    -Headers $headers `
    -Body $groupBody `
    -SkipCertificateCheck

$groupId = ($groupResponse.Content | ConvertFrom-Json).id

# GET with excludedAttributes
$response = Invoke-WebRequest `
    -Uri "https://localhost:5001/scim/Groups/$groupId`?excludedAttributes=members" `
    -Headers $headers `
    -SkipCertificateCheck

$group = $response.Content | ConvertFrom-Json

if ($null -eq $group.members) {
    Write-Host "✅ ExcludedAttributes works: members excluded"
} else {
    Write-Host "❌ ExcludedAttributes failed: members still present"
}
```

### 5. Test Flexible Boolean

```powershell
# Create user with string boolean (test flexible converter)
$userBody = @{
    userName = "testuser@example.com"
    displayName = "Test User"
    active = $true
    emails = @(
        @{
            value = "test@example.com"
            primary = "true"  # ← String instead of boolean
        }
    )
    schemas = @("urn:ietf:params:scim:schemas:core:2.0:User")
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "https://localhost:5001/scim/Users" `
        -Method POST `
        -Headers $headers `
        -Body $userBody `
        -SkipCertificateCheck
    
    Write-Host "✅ Flexible boolean converter works: String 'true' accepted"
} catch {
    Write-Host "❌ Failed: $($_.ErrorDetails.Message)"
}
```

### 6. Test PATCH Group Properties

```powershell
# Create a group
$groupId = "..." # Use ID from previous test

# Patch with new externalId
$patchBody = @{
    schemas = @("urn:ietf:params:scim:api:messages:2.0:PatchOp")
    Operations = @(
        @{
            op = "replace"
            value = @{
                externalId = "new-external-id-123"
            }
        }
    )
} | ConvertTo-Json

$response = Invoke-WebRequest `
    -Uri "https://localhost:5001/scim/Groups/$groupId" `
    -Method PATCH `
    -Headers $headers `
    -Body $patchBody `
    -SkipCertificateCheck

$patched = $response.Content | ConvertFrom-Json

if ($patched.externalId -eq "new-external-id-123") {
    Write-Host "✅ PATCH replace works: externalId updated"
} else {
    Write-Host "❌ PATCH replace failed: externalId not updated"
}
```

---

## Production Deployment

### 1. Publish Release Build

```powershell
# Create release package
dotnet publish EzSCIM.EntraID.Demo/EzSCIM.EntraID.Demo.csproj `
    -c Release `
    -o ./publish `
    --no-self-contained

# Output location: ./publish/
```

### 2. Deploy to Azure App Service

```powershell
# Install Azure CLI if not present
# https://aka.ms/installazurecliwindows

# Login to Azure
az login

# Deploy to App Service
az webapp deployment source config-zip `
    --resource-group <resource-group> `
    --name <app-service-name> `
    --src ./publish.zip

# Verify deployment
az webapp show `
    --resource-group <resource-group> `
    --name <app-service-name> `
    --query "state"
```

### 3. Configure Environment Variables

```powershell
# Set required configuration
az webapp config appsettings set `
    --resource-group <resource-group> `
    --name <app-service-name> `
    --settings `
        "ASPNETCORE_ENVIRONMENT=Production" `
        "JWT_SECRET=<your-jwt-secret>" `
        "CORS_ORIGINS=https://yourdomain.com"

# Restart the app
az webapp restart `
    --resource-group <resource-group> `
    --name <app-service-name>
```

---

## SCIM Compliance Testing

### Using Microsoft Entra ID Test Tool

1. **Configuration:**
   - Base URL: `https://<deployed-url>/scim`
   - Authentication: Bearer Token
   - Token: Use JWT token from your auth endpoint

2. **Run Full Test Suite:**
   - Navigate to: https://scim-test-tool.microsoft.com
   - Enter your SCIM API endpoint
   - Select "Test All"

3. **Expected Results:**
   ```
   Total Tests: 62+
   ✅ Passed: 62+ (all should pass)
   ❌ Failed: 0
   ⚠️ Skipped: 0
   
   SFComplianceFailed: false ← CRITICAL
   ```

### Manual Test Checklist

- [ ] Create User - Returns 201 with boolean emails accepted
- [ ] Create Group - Returns 201 with English error on duplicate
- [ ] Get User with excludedAttributes=emails - Members excluded
- [ ] Get Group with excludedAttributes=members - Members excluded
- [ ] PATCH Group externalId - Value updated
- [ ] PATCH Group displayName - Value updated
- [ ] Delete non-existent resource - Returns 404 with English error
- [ ] All error messages in English - No French messages

---

## Rollback Procedure

If issues are encountered:

```powershell
# Stop the application
Stop-WebAppIISExpress -Name "EzSCIM.EntraID.Demo"

# Revert to previous build
git checkout HEAD~1

# Rebuild
dotnet build EzSCIM/EzSCIM.csproj -c Release

# Restart
dotnet run
```

---

## Monitoring & Logging

### Enable Debug Logging

```csharp
// In Program.cs or appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "EzSCIM": "Debug"  // ← Add this for detailed SCIM logs
    }
  }
}
```

### Check Logs

```powershell
# Docker logs (if containerized)
docker logs <container-id> --tail 100

# File logs (if file logging configured)
Get-Content -Path "./logs/scim-api.log" -Tail 50
```

---

## Troubleshooting

### Issue: "Boolean deserialization error"

**Solution:** Verify `FlexibleBooleanJsonConverter.cs` is in `EzSCIM/Helpers/` and imported correctly.

### Issue: "excludedAttributes not working"

**Solution:** Ensure query parameter is added to controller methods and `FilterUserAttributes`/`FilterGroupAttributes` methods exist.

### Issue: "PATCH not updating properties"

**Solution:** Verify `ApplyGroupPatchOperation` has the new "replace" operation handling.

### Issue: "French error messages still appearing"

**Solution:** Search codebase for remaining French strings:
```powershell
grep -r "Utilisateur\|Groupe\|Erreur" EzSCIM/ --include="*.cs"
```

---

## Performance Benchmarks

Expected performance impact: **Negligible**

- Boolean conversion: < 1ms per request
- Attribute filtering: < 2ms for 10-item list
- Error message translation: No runtime impact (compile-time)
- PATCH operations: Same as before

---

## Support & Questions

For issues or questions:
1. Check application logs
2. Review error messages (should be in English)
3. Verify SCIM request format matches RFC 7643
4. Consult [../docs/guides/](../docs/guides/) for usage examples


