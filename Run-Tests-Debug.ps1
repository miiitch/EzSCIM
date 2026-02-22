$ErrorActionPreference = "Continue"

Write-Host "=== Running Tests ===" -ForegroundColor Cyan
Write-Host ""

cd C:\Users\MichelPerfetti\src\private\scimwork

# Build first
Write-Host "Building..." -ForegroundColor Yellow
dotnet build --no-restore > build.log 2>&1
$buildExitCode = $LASTEXITCODE

if ($buildExitCode -ne 0) {
    Write-Host "Build FAILED!" -ForegroundColor Red
    Get-Content build.log
    exit 1
}

Write-Host "Build succeeded" -ForegroundColor Green
Write-Host ""

# Run specific tests that might be failing
Write-Host "Running ScimPatchApplier tests..." -ForegroundColor Yellow

$testFilter = "FullyQualifiedName~ScimPatchApplier"
$testOutput = dotnet test --no-build --filter $testFilter --logger "console;verbosity=detailed" 2>&1 | Out-String

Write-Host $testOutput

# Save to file
$testOutput | Out-File -FilePath "patch-test-results.txt" -Encoding UTF8

# Check for failures
if ($testOutput -match "Failed!") {
    Write-Host "`n=== TESTS FAILED ===" -ForegroundColor Red
    
    # Extract failure details
    $lines = $testOutput -split "`n"
    $inFailure = $false
    foreach ($line in $lines) {
        if ($line -match "Failed" -or $line -match "Error Message" -or $line -match "Stack Trace") {
            $inFailure = $true
        }
        if ($inFailure) {
            Write-Host $line -ForegroundColor Red
        }
    }
} else {
    Write-Host "`n=== TESTS PASSED ===" -ForegroundColor Green
}

Write-Host "`nTest output saved to: patch-test-results.txt" -ForegroundColor Cyan

