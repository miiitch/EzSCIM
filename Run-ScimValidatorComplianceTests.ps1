#!/usr/bin/env pwsh
# Script to run ScimValidatorComplianceTests and identify failing tests

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Running ScimValidatorComplianceTests" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n[1/3] Building project..." -ForegroundColor Yellow
$buildOutput = dotnet build EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj -c Debug --nologo 2>&1
$buildExitCode = $LASTEXITCODE

if ($buildExitCode -eq 0) {
    Write-Host "✅ Build succeeded" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    $buildOutput | ForEach-Object { Write-Host $_ }
    exit 1
}

Write-Host "`n[2/3] Running ScimValidatorComplianceTests (20 tests)..." -ForegroundColor Yellow
Write-Host "Note: This may take 2-5 minutes due to Docker container startup" -ForegroundColor Gray
Write-Host ""

$testOutput = dotnet test EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj `
    --no-build `
    -c Debug `
    --nologo `
    --logger "console;verbosity=detailed" `
    --filter "FullyQualifiedName~ScimValidatorComplianceTests" `
    2>&1

$testExitCode = $LASTEXITCODE

Write-Host "`n[3/3] Test Results:" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan

# Display full output
$testOutput | ForEach-Object { Write-Host $_ }

Write-Host "`n========================================" -ForegroundColor Cyan
if ($testExitCode -eq 0) {
    Write-Host "✅ All ScimValidatorComplianceTests passed!" -ForegroundColor Green
} else {
    Write-Host "❌ Some tests failed - see details above" -ForegroundColor Red
    Write-Host ""
    Write-Host "Failed tests summary:" -ForegroundColor Yellow
    $testOutput | Select-String "Failed.*ScimValidatorComplianceTests" | ForEach-Object { Write-Host $_ -ForegroundColor Red }
}
Write-Host "========================================" -ForegroundColor Cyan

exit $testExitCode

