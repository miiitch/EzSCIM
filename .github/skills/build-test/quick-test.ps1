# Quick-Test.ps1
# Simplified test script for quick validation

param(
    [string]$TestPattern = "Patch"
)

$project = "C:\Users\MichelPerfetti\src\private\scimwork\EzSCIM.IntegrationTests\EzSCIM.IntegrationTests.csproj"

Write-Host "Building..." -ForegroundColor Yellow
dotnet build $project --configuration Release --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build FAILED" -ForegroundColor Red
    exit 1
}

Write-Host "`nRunning tests matching: $TestPattern" -ForegroundColor Yellow
dotnet test $project `
    --filter "FullyQualifiedName~$TestPattern" `
    --no-build `
    --configuration Release `
    --logger "console;verbosity=normal"

Write-Host "`nDone. Exit code: $LASTEXITCODE" -ForegroundColor Cyan
exit $LASTEXITCODE

