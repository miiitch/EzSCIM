#!/usr/bin/env pwsh
# Quick test script to verify PATCH fix

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing SCIM PATCH Fix" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n[1/3] Building project..." -ForegroundColor Yellow
$buildOutput = dotnet build EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj -c Release --nologo 2>&1
$buildExitCode = $LASTEXITCODE

if ($buildExitCode -eq 0) {
    Write-Host "✅ Build succeeded" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    $buildOutput | ForEach-Object { Write-Host $_ }
    exit 1
}

Write-Host "`n[2/3] Running PATCH compliance tests..." -ForegroundColor Yellow
$testOutput = dotnet test EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj `
    --no-build `
    -c Release `
    --nologo `
    --logger "console;verbosity=normal" `
    --filter "FullyQualifiedName~PatchUser_ReplaceFilteredEmailPrimaryValue" `
    2>&1

$testExitCode = $LASTEXITCODE

Write-Host "`n[3/3] Test Results:" -ForegroundColor Yellow
$testOutput | ForEach-Object { Write-Host $_ }

if ($testExitCode -eq 0) {
    Write-Host "`n✅ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "`n❌ Some tests failed" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Exit Code: $testExitCode" -ForegroundColor $(if ($testExitCode -eq 0) { "Green" } else { "Red" })
Write-Host "========================================" -ForegroundColor Cyan

exit $testExitCode

