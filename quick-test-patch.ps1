#!/usr/bin/env pwsh
# Quick compilation check

Write-Host "Compiling test project..." -ForegroundColor Cyan
$result = dotnet build EzSCIM.IntegrationTests -c Debug --nologo 2>&1 | Out-String

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Compilation successful" -ForegroundColor Green
    Write-Host "`nRunning single test..." -ForegroundColor Cyan
    dotnet test EzSCIM.IntegrationTests `
        --no-build `
        -c Debug `
        --filter "PatchGroup_ReplaceMembers_ShouldUpdateMembersList" `
        --logger "console;verbosity=detailed" 2>&1
} else {
    Write-Host "❌ Compilation failed:" -ForegroundColor Red
    Write-Host $result
}

