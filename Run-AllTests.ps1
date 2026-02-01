#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SCIM API Test Runner - Comprehensive test automation script
.DESCRIPTION
    This script provides convenient commands to compile, test, and validate the SCIM API implementation.
    It handles the FilterExpression integration and verifies all compilation requirements.
.EXAMPLE
    .\Run-AllTests.ps1 -CompileOnly
    .\Run-AllTests.ps1 -TestRepository
    .\Run-AllTests.ps1 -FullValidation
#>

param(
    [Parameter(Mandatory = $false)]
    [switch]$CompileOnly,
    
    [Parameter(Mandatory = $false)]
    [switch]$TestRepository,
    
    [Parameter(Mandatory = $false)]
    [switch]$TestControllers,
    
    [Parameter(Mandatory = $false)]
    [switch]$TestFilters,
    
    [Parameter(Mandatory = $false)]
    [switch]$FullValidation,
    
    [Parameter(Mandatory = $false)]
    [switch]$Verbose
)

# Colors for output
$colors = @{
    Success = 'Green'
    Error   = 'Red'
    Warning = 'Yellow'
    Info    = 'Cyan'
    Prompt  = 'Magenta'
}

function Write-Section {
    param([string]$Title, [string]$Color = 'Cyan')
    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $Color
    Write-Host "  $Title" -ForegroundColor $Color
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $Color
}

function Write-Result {
    param([string]$Message, [bool]$Success)
    $icon = if ($Success) { "✅" } else { "❌" }
    $color = if ($Success) { $colors.Success } else { $colors.Error }
    Write-Host "$icon $Message" -ForegroundColor $color
}

function Invoke-Dotnet {
    param([string]$Command, [string]$Description)
    Write-Host "`n➤ $Description..." -ForegroundColor $colors.Prompt
    $result = & dotnet $Command 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Result "$Description" $true
        return $true
    }
    else {
        Write-Result "$Description" $false
        if ($Verbose) {
            Write-Host $result -ForegroundColor $colors.Error
        }
        return $false
    }
}

# Main execution
Write-Host "`n🚀 SCIM API - Test & Validation Suite" -ForegroundColor $colors.Info -BackgroundColor Black

# If no switches provided, run full validation
if (-not ($CompileOnly -or $TestRepository -or $TestControllers -or $TestFilters)) {
    $FullValidation = $true
}

$allPassed = $true

# ============ STEP 1: CLEAN BUILD ============
Write-Section "STEP 1: Clean Build"

if ($Verbose) {
    dotnet clean 2>&1 | Where-Object { $_ -match "error|warning|cleaned" }
}
else {
    dotnet clean > $null 2>&1
}

$allPassed = Invoke-Dotnet "clean" "Cleaning build artifacts" -and $allPassed

# ============ STEP 2: RESTORE DEPENDENCIES ============
Write-Section "STEP 2: Restore Dependencies"
$allPassed = Invoke-Dotnet "restore" "Restoring NuGet packages" -and $allPassed

# ============ STEP 3: COMPILE MAIN PROJECT ============
Write-Section "STEP 3: Compile Main Project"
$allPassed = Invoke-Dotnet "build ScimAPI/ScimAPI.csproj --configuration Debug" "Compiling ScimAPI project" -and $allPassed

# ============ STEP 4: COMPILE TEST PROJECT ============
Write-Section "STEP 4: Compile Test Project"
$allPassed = Invoke-Dotnet "build ScimAPI.Tests/ScimAPI.Tests.csproj --configuration Debug" "Compiling ScimAPI.Tests project" -and $allPassed

if (-not $allPassed) {
    Write-Section "⚠️  BUILD FAILED - Cannot proceed to testing" "Red"
    exit 1
}

if ($CompileOnly) {
    Write-Section "✅ Compilation Complete - CompileOnly mode (no tests run)" "Green"
    exit 0
}

# ============ STEP 5: RUN REPOSITORY TESTS ============
if ($TestRepository -or $FullValidation) {
    Write-Section "STEP 5: Repository Filter Tests"
    Write-Host "`nTesting InMemoryScimRepository with FilterExpression AST..." -ForegroundColor $colors.Info
    
    $result = & dotnet test ScimAPI.Tests/ScimAPI.Tests.csproj `
        --filter "ClassName=ScimAPI.Tests.InMemoryScimRepositoryTests" `
        --configuration Debug `
        --logger "console;verbosity=minimal" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Result "Repository Tests Passed" $true
    }
    else {
        Write-Result "Repository Tests Failed" $false
        $allPassed = $false
    }
}

# ============ STEP 6: RUN CONTROLLER TESTS ============
if ($TestControllers -or $FullValidation) {
    Write-Section "STEP 6: Controller Filter Tests"
    
    Write-Host "`nTesting UsersController with FilterExpression..." -ForegroundColor $colors.Info
    $result = & dotnet test ScimAPI.Tests/ScimAPI.Tests.csproj `
        --filter "ClassName=ScimAPI.Tests.UsersControllerTests" `
        --configuration Debug `
        --logger "console;verbosity=minimal" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Result "UsersController Tests Passed" $true
    }
    else {
        Write-Result "UsersController Tests Failed" $false
        $allPassed = $false
    }
    
    Write-Host "`nTesting GroupsController with FilterExpression..." -ForegroundColor $colors.Info
    $result = & dotnet test ScimAPI.Tests/ScimAPI.Tests.csproj `
        --filter "ClassName=ScimAPI.Tests.GroupsControllerTests" `
        --configuration Debug `
        --logger "console;verbosity=minimal" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Result "GroupsController Tests Passed" $true
    }
    else {
        Write-Result "GroupsController Tests Failed" $false
        $allPassed = $false
    }
}

# ============ STEP 7: RUN FILTER PARSER TESTS ============
if ($TestFilters -or $FullValidation) {
    Write-Section "STEP 7: Filter Parser Tests"
    
    Write-Host "`nTesting Filter Parser (AST conversion)..." -ForegroundColor $colors.Info
    $result = & dotnet test ScimAPI.Tests/ScimAPI.Tests.csproj `
        --filter "ClassName=ScimAPI.Tests.Filtering.FilterParserTests" `
        --configuration Debug `
        --logger "console;verbosity=minimal" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Result "FilterParser Tests Passed" $true
    }
    else {
        Write-Result "FilterParser Tests Failed" $false
        $allPassed = $false
    }
    
    Write-Host "`nTesting Filter Parser Error Handling..." -ForegroundColor $colors.Info
    $result = & dotnet test ScimAPI.Tests/ScimAPI.Tests.csproj `
        --filter "ClassName=ScimAPI.Tests.Filtering.FilterParserErrorTests" `
        --configuration Debug `
        --logger "console;verbosity=minimal" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Result "FilterParser Error Tests Passed" $true
    }
    else {
        Write-Result "FilterParser Error Tests Failed" $false
        $allPassed = $false
    }
}

# ============ STEP 8: FULL TEST SUITE ============
if ($FullValidation) {
    Write-Section "STEP 8: Full Test Suite"
    Write-Host "`nRunning ALL tests..." -ForegroundColor $colors.Info
    
    $result = & dotnet test ScimAPI.Tests/ScimAPI.Tests.csproj `
        --configuration Debug `
        --logger "console;verbosity=normal" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Result "Full Test Suite Passed" $true
    }
    else {
        Write-Result "Full Test Suite Failed" $false
        $allPassed = $false
    }
}

# ============ FINAL SUMMARY ============
Write-Section "FINAL RESULTS" $(if ($allPassed) { "Green" } else { "Red" })

if ($allPassed) {
    Write-Host "`n✅ ALL CHECKS PASSED - Implementation is ready!`n" -ForegroundColor $colors.Success
    
    Write-Host "Next steps:" -ForegroundColor $colors.Info
    Write-Host "  1. Start the API: dotnet run --project ScimAPI/ScimAPI.csproj" -ForegroundColor $colors.Info
    Write-Host "  2. Test endpoints using ScimAPI.http or curl" -ForegroundColor $colors.Info
    Write-Host "  3. Review documentation in markdown files" -ForegroundColor $colors.Info
    exit 0
}
else {
    Write-Host "`n❌ SOME CHECKS FAILED - Please review errors above`n" -ForegroundColor $colors.Error
    exit 1
}
