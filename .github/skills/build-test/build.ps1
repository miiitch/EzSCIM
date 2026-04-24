# Build.ps1
# Generic build script for .NET projects
#
# Usage:
#   .\scripts\Build.ps1 -Project "EzSCIM.IntegrationTests"
#   .\scripts\Build.ps1 -Project "EzSCIM" -Clean -Verbose

param(
    [Parameter(Mandatory=$false)]
    [string]$Project = "EzSCIM.IntegrationTests",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [switch]$Clean,
    [switch]$VerboseOutput,
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"
$rootPath = Split-Path -Parent $PSScriptRoot

# Find project file
$projectFile = Get-ChildItem -Path $rootPath -Recurse -Filter "$Project.csproj" | Select-Object -First 1

if (-not $projectFile) {
    Write-Host "❌ Project '$Project.csproj' not found" -ForegroundColor Red
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building: $($projectFile.Name)" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Clean if requested
if ($Clean) {
    Write-Host "`nCleaning..." -ForegroundColor Yellow
    dotnet clean $projectFile.FullName --configuration $Configuration --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Clean failed!" -ForegroundColor Red
        exit 1
    }
}

# Build
$verbosity = if ($VerboseOutput) { "normal" } else { "minimal" }
$restoreFlag = if ($NoRestore) { "--no-restore" } else { "" }

Write-Host "`nCompiling..." -ForegroundColor Yellow
$buildArgs = @(
    "build",
    $projectFile.FullName,
    "--configuration", $Configuration,
    "--verbosity", $verbosity
)

if ($NoRestore) {
    $buildArgs += "--no-restore"
}

$output = & dotnet @buildArgs 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Build succeeded!" -ForegroundColor Green
    Write-Host "Project: $($projectFile.FullName)" -ForegroundColor Gray
    exit 0
} else {
    Write-Host "`n❌ Build failed!" -ForegroundColor Red
    $output | Write-Host
    exit 1
}

