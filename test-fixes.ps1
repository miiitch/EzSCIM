#!/usr/bin/env pwsh
# Test the fixes made to SCIM integration tests

Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nRunning integration tests..." -ForegroundColor Cyan
dotnet test EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj --no-build --verbosity normal

Write-Host "`nTest run complete." -ForegroundColor Green

