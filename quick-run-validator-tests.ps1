#!/usr/bin/env pwsh
# Quick script to run and capture test results

$ErrorActionPreference = "Continue"

Write-Host "Building..." -ForegroundColor Cyan
dotnet build EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj -c Debug

Write-Host "`nRunning tests..." -ForegroundColor Cyan
dotnet test EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj `
    --filter "FullyQualifiedName~ScimValidatorComplianceTests" `
    --logger "console;verbosity=normal" `
    --no-build `
    -c Debug `
    > scim-validator-results.log 2>&1

Write-Host "`nTests complete. Results saved to scim-validator-results.log" -ForegroundColor Green
Get-Content scim-validator-results.log

