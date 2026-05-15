# Quick Guide - EntraID Demo + Aspire + SCIM

This guide explains how to expose `entraid.demo` via Aspire/DevTunnels, generate a token, and connect Microsoft Entra for SCIM testing.

## Prerequisites

- .NET SDK installed
- Solution opened in this repository
- Access to Microsoft Entra (test tenant)

## 1) Start Aspire (AppHost)

From the repository root, start the Aspire AppHost:

```powershell
dotnet run --project .\EzSCIM.EntraID.AppHost
```

When the AppHost starts, it creates an HTTPS DevTunnel for the `scimapi` project.

## 2) Get the DevTunnel URL

Retrieve the public HTTPS URL from:

- The Aspire dashboard (endpoint links), or
- The AppHost console output

Note the URL in the form:

```
https://<your-tunnel>.devtunnels.ms
```

The public SCIM URL will be:

```
https://<your-tunnel>.devtunnels.ms/scim
```

## 3) Generate a token (dev)

### Option A - Provided PowerShell script

```powershell
.\Generate-Token.ps1 -ApiBaseUrl "https://<your-tunnel>.devtunnels.ms" -CopyToClipboard
```

The script returns a JWT and can copy `Bearer <token>` to the clipboard.

### Option B - Call the endpoint directly

```powershell
Invoke-RestMethod -Uri "https://<your-tunnel>.devtunnels.ms/scim/auth/token" -Method Get
```

Read the `token` field, then use `Bearer <token>`.

> Note: the `/scim/auth/token` endpoint is available only in development.

## 4) Connect Microsoft Entra (SCIM tests)

In **Microsoft Entra ID > Enterprise applications > (your app) > Provisioning**:

- **Tenant URL**: `https://<your-tunnel>.devtunnels.ms/scim`
- **Secret Token**: `Bearer <token>`
- Click **Test Connection**

If everything is correct, Entra calls `ServiceProviderConfig`, `Schemas`, and `Users`.

## 5) Quick SCIM call test

```powershell
$headers = @{ "Authorization" = "Bearer <token>" }
Invoke-RestMethod -Uri "https://<your-tunnel>.devtunnels.ms/scim/Users" -Headers $headers -Method Get
```

## Quick troubleshooting

- **401 Unauthorized**: missing token, wrong format, or expired token.
- **403 on /scim/auth/token**: non-dev environment or endpoint disabled.
- **Invalid URL**: verify the DevTunnel URL and ensure the AppHost is still running.
