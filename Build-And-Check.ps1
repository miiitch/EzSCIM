$ErrorActionPreference = "Continue"

Write-Host "=== Building Project ===" -ForegroundColor Cyan
Write-Host ""

cd C:\Users\MichelPerfetti\src\private\scimwork

# Clean first
Write-Host "Cleaning..." -ForegroundColor Yellow
dotnet clean --verbosity quiet

# Build
Write-Host "Building..." -ForegroundColor Yellow
$output = dotnet build --no-incremental --verbosity normal 2>&1

# Display output
$output | Out-String | Write-Host

# Check for errors
$errors = $output | Select-String -Pattern "error CS"
$warnings = $output | Select-String -Pattern "warning CS"

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Errors: $($errors.Count)" -ForegroundColor $(if ($errors.Count -gt 0) { "Red" } else { "Green" })
Write-Host "Warnings: $($warnings.Count)" -ForegroundColor Yellow

if ($errors.Count -gt 0) {
    Write-Host ""
    Write-Host "=== Errors ===" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host $_.Line -ForegroundColor Red }
}

# Save to file
$output | Out-File -FilePath "build-result.txt" -Encoding UTF8
Write-Host ""
Write-Host "Full output saved to build-result.txt" -ForegroundColor Cyan

