# Test.ps1
# Generic test script for .NET projects with flexible filtering
#
# Usage:
#   .\scripts\Test.ps1 -Filter "Patch"
#   .\scripts\Test.ps1 -Filter "PatchUser_ReplaceAllScalarAttributes" -Verbose
#   .\scripts\Test.ps1 -Project "EzSCIM.IntegrationTests" -Filter "ScimValidatorComplianceTests"

param(
    [Parameter(Mandatory=$false)]
    [string]$Project = "EzSCIM.IntegrationTests",
    
    [Parameter(Mandatory=$false)]
    [string]$Filter = "",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("quiet", "minimal", "normal", "detailed", "diagnostic")]
    [string]$Verbosity = "normal",
    
    [switch]$NoBuild,
    [switch]$Rebuild
)

$ErrorActionPreference = "Continue"
$rootPath = Split-Path -Parent $PSScriptRoot

# Find project file
$projectFile = Get-ChildItem -Path $rootPath -Recurse -Filter "$Project.csproj" | Select-Object -First 1

if (-not $projectFile) {
    Write-Host "❌ Project '$Project.csproj' not found" -ForegroundColor Red
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Running Tests: $($projectFile.Name)" -ForegroundColor Cyan
if ($Filter) {
    Write-Host "Filter: $Filter" -ForegroundColor Cyan
}
Write-Host "========================================" -ForegroundColor Cyan

# Rebuild if requested
if ($Rebuild) {
    Write-Host "`nRebuilding project..." -ForegroundColor Yellow
    & "$PSScriptRoot\Build.ps1" -Project $Project -Clean -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed, aborting tests" -ForegroundColor Red
        exit 1
    }
}

# Build if needed
if (-not $NoBuild -and -not $Rebuild) {
    Write-Host "`nBuilding project..." -ForegroundColor Yellow
    & "$PSScriptRoot\Build.ps1" -Project $Project -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed, aborting tests" -ForegroundColor Red
        exit 1
    }
}

# Run tests
Write-Host "`nRunning tests..." -ForegroundColor Yellow

$testArgs = @(
    "test",
    $projectFile.FullName,
    "--no-build",
    "--configuration", $Configuration,
    "--logger", "console;verbosity=$Verbosity"
)

if ($Filter) {
    $testArgs += "--filter"
    $testArgs += "FullyQualifiedName~$Filter"
}

$output = & dotnet @testArgs 2>&1 | Tee-Object -Variable testOutput

$exitCode = $LASTEXITCODE

# Parse results
$passedMatch = $testOutput | Select-String -Pattern "Passed!\s+.*?(\d+)\s+passed"
$failedMatch = $testOutput | Select-String -Pattern "Failed!\s+.*?(\d+)\s+failed"
$totalMatch = $testOutput | Select-String -Pattern "Total tests:\s+(\d+)"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Results Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($totalMatch) {
    $total = $totalMatch.Matches.Groups[1].Value
    Write-Host "Total tests: $total" -ForegroundColor White
}

if ($passedMatch) {
    $passed = $passedMatch.Matches.Groups[1].Value
    Write-Host "Passed:      $passed" -ForegroundColor Green
}

if ($failedMatch) {
    $failed = $failedMatch.Matches.Groups[1].Value
    Write-Host "Failed:      $failed" -ForegroundColor Red
}

if ($exitCode -eq 0) {
    Write-Host "`n✅ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "`n❌ Some tests failed" -ForegroundColor Red
}

Write-Host "========================================" -ForegroundColor Cyan

exit $exitCode

