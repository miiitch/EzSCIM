#!/usr/bin/env pwsh
# Quick test for PatchGroup_ReplaceMembers_ShouldUpdateMembersList

Write-Host "Building..." -ForegroundColor Cyan
dotnet build EzSCIM.IntegrationTests -c Debug --nologo

Write-Host "`nRunning test..." -ForegroundColor Cyan
dotnet test EzSCIM.IntegrationTests `
    --no-build `
    -c Debug `
    --filter "PatchGroup_ReplaceMembers_ShouldUpdateMembersList" `
    --logger "console;verbosity=normal"

