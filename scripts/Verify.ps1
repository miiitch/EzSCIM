# Verify.ps1
# Quick verification script - compile and show summary

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EzSCIM Quick Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$project = "C:\Users\MichelPerfetti\src\private\scimwork\EzSCIM.IntegrationTests\EzSCIM.IntegrationTests.csproj"

# Step 1: Build
Write-Host "`n[1/2] Building..." -ForegroundColor Yellow
$buildOutput = dotnet build $project --configuration Release --verbosity quiet 2>&1 | Out-String

if ($LASTEXITCODE -eq 0) {
    Write-Host "      ✅ Build succeeded" -ForegroundColor Green
} else {
    Write-Host "      ❌ Build failed" -ForegroundColor Red
    $buildOutput | Select-String -Pattern "error" | ForEach-Object { Write-Host "      $_" -ForegroundColor Red }
    exit 1
}

# Step 2: Show files modified
Write-Host "`n[2/2] Modified Files:" -ForegroundColor Yellow
$files = @(
    "UserEntityPatchApplier.cs",
    "ScimWebApplicationFactory.cs", 
    "ScimValidatorComplianceTests.cs"
)

foreach ($file in $files) {
    if (Test-Path "C:\Users\MichelPerfetti\src\private\scimwork\EzSCIM.IntegrationTests\**\$file" -PathType Leaf) {
        Write-Host "      ✅ $file" -ForegroundColor Green
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Ready to test!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Run tests with:" -ForegroundColor White
Write-Host "  .\scripts\Test.ps1 -Filter 'Patch'" -ForegroundColor Cyan
Write-Host ""

