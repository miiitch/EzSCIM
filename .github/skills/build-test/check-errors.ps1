# Check-Errors.ps1
# Check for compilation errors in specific files or entire project
#
# Usage:
#   .\scripts\Check-Errors.ps1
#   .\scripts\Check-Errors.ps1 -Project "EzSCIM.IntegrationTests"
#   .\scripts\Check-Errors.ps1 -Files "UserEntityPatchApplier.cs", "ScimWebApplicationFactory.cs"

param(
    [Parameter(Mandatory=$false)]
    [string]$Project = "EzSCIM.IntegrationTests",
    
    [Parameter(Mandatory=$false)]
    [string[]]$Files = @()
)

$ErrorActionPreference = "Continue"
$rootPath = Split-Path -Parent $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Checking Compilation Errors" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Find project file
$projectFile = Get-ChildItem -Path $rootPath -Recurse -Filter "$Project.csproj" | Select-Object -First 1

if (-not $projectFile) {
    Write-Host "❌ Project '$Project.csproj' not found" -ForegroundColor Red
    exit 1
}

# If specific files are provided, show them
if ($Files.Count -gt 0) {
    Write-Host "`nChecking specific files:" -ForegroundColor Yellow
    foreach ($file in $Files) {
        Write-Host "  - $file" -ForegroundColor Gray
    }
}

Write-Host "`nBuilding to detect errors..." -ForegroundColor Yellow

# Build and capture output
$output = dotnet build $projectFile.FullName --no-restore --verbosity quiet 2>&1 | Out-String

# Parse errors and warnings
$errors = $output | Select-String -Pattern "error CS\d+" -AllMatches
$warnings = $output | Select-String -Pattern "warning CS\d+" -AllMatches

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Results" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($errors) {
    Write-Host "`n❌ Found $($errors.Count) compilation error(s):" -ForegroundColor Red
    Write-Host ""
    
    $errors | ForEach-Object {
        $line = $_.Line
        # Extract file name and error message
        if ($line -match "(.+\.cs)\((\d+),(\d+)\):\s+error\s+(CS\d+):\s+(.+)") {
            $fileName = [System.IO.Path]::GetFileName($matches[1])
            $lineNum = $matches[2]
            $errorCode = $matches[4]
            $message = $matches[5]
            
            Write-Host "  $fileName($lineNum): $errorCode" -ForegroundColor Red
            Write-Host "    $message" -ForegroundColor Gray
        } else {
            Write-Host "  $line" -ForegroundColor Red
        }
    }
    
    $hasErrors = $true
} else {
    Write-Host "✅ No compilation errors found!" -ForegroundColor Green
    $hasErrors = $false
}

if ($warnings) {
    Write-Host "`n⚠️  Found $($warnings.Count) warning(s):" -ForegroundColor Yellow
    Write-Host ""
    
    $warnings | Select-Object -First 5 | ForEach-Object {
        $line = $_.Line
        if ($line -match "(.+\.cs)\((\d+),(\d+)\):\s+warning\s+(CS\d+):\s+(.+)") {
            $fileName = [System.IO.Path]::GetFileName($matches[1])
            $lineNum = $matches[2]
            $warningCode = $matches[4]
            $message = $matches[5]
            
            Write-Host "  $fileName($lineNum): $warningCode" -ForegroundColor Yellow
            Write-Host "    $message" -ForegroundColor Gray
        }
    }
    
    if ($warnings.Count -gt 5) {
        Write-Host "  ... and $($warnings.Count - 5) more warning(s)" -ForegroundColor Gray
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan

if ($hasErrors) {
    exit 1
} else {
    exit 0
}

