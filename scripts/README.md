# EzSCIM Build & Test Scripts

Generic PowerShell scripts for building and testing the EzSCIM project.

## Available Scripts

### 1. Build.ps1
Compile .NET projects with flexible configuration.

**Parameters:**
- `Project` (string): Project name without .csproj extension (default: "EzSCIM.IntegrationTests")
- `Configuration` (string): Build configuration - Debug or Release (default: "Release")
- `Clean` (switch): Clean before building
- `Verbose` (switch): Show detailed build output
- `NoRestore` (switch): Skip package restore

**Examples:**
```powershell
# Build with defaults (Release configuration)
.\scripts\Build.ps1

# Build specific project with clean
.\scripts\Build.ps1 -Project "EzSCIM" -Clean

# Build in Debug mode with verbose output
.\scripts\Build.ps1 -Configuration Debug -Verbose

# Build without restoring packages
.\scripts\Build.ps1 -NoRestore
```

---

### 2. Test.ps1
Run tests with flexible filtering and configuration.

**Parameters:**
- `Project` (string): Test project name (default: "EzSCIM.IntegrationTests")
- `Filter` (string): Test name filter pattern (default: "" - all tests)
- `Configuration` (string): Build configuration (default: "Release")
- `Verbosity` (string): Test output verbosity - quiet, minimal, normal, detailed, diagnostic (default: "normal")
- `NoBuild` (switch): Skip build step
- `Rebuild` (switch): Clean and rebuild before testing

**Examples:**
```powershell
# Run all tests
.\scripts\Test.ps1

# Run only PATCH-related tests
.\scripts\Test.ps1 -Filter "Patch"

# Run specific test with detailed output
.\scripts\Test.ps1 -Filter "PatchUser_ReplaceAllScalarAttributes" -Verbosity detailed

# Run tests without building
.\scripts\Test.ps1 -NoBuild -Filter "ScimValidatorComplianceTests"

# Rebuild and test
.\scripts\Test.ps1 -Rebuild -Filter "Patch"
```

---

### 3. Check-Errors.ps1
Check for compilation errors in the project.

**Parameters:**
- `Project` (string): Project name to check (default: "EzSCIM.IntegrationTests")
- `Files` (string[]): Specific files to mention in output (optional)

**Examples:**
```powershell
# Check all compilation errors
.\scripts\Check-Errors.ps1

# Check specific project
.\scripts\Check-Errors.ps1 -Project "EzSCIM"

# Check with specific files highlighted
.\scripts\Check-Errors.ps1 -Files "UserEntityPatchApplier.cs", "ScimWebApplicationFactory.cs"
```

---

## Common Workflows

### Verify Changes After Code Modification
```powershell
# 1. Check for compilation errors
.\scripts\Check-Errors.ps1

# 2. Build the project
.\scripts\Build.ps1

# 3. Run affected tests
.\scripts\Test.ps1 -Filter "Patch"
```

### Quick Test Cycle
```powershell
# Build and run specific test
.\scripts\Build.ps1 && .\scripts\Test.ps1 -Filter "PatchUser_RemoveDisplayName" -NoBuild
```

### Full Validation
```powershell
# Clean build and run all compliance tests
.\scripts\Build.ps1 -Clean
.\scripts\Test.ps1 -Filter "ScimValidatorComplianceTests" -NoBuild -Verbosity detailed
```

### Debug Failing Test
```powershell
# Run single test with maximum verbosity
.\scripts\Test.ps1 -Filter "PatchUser_ReplaceAllScalarAttributes_ViaValueObject" -Verbosity diagnostic -NoBuild
```

---

## Exit Codes

All scripts use standard exit codes:
- `0` - Success
- `1` - Failure (build error, test failure, compilation error)

This allows chaining commands with `&&`:
```powershell
.\scripts\Build.ps1 && .\scripts\Test.ps1 -NoBuild
```

---

## Integration with Development Workflow

### Pre-commit Check
```powershell
.\scripts\Check-Errors.ps1 && .\scripts\Test.ps1 -Filter "Patch" -Verbosity quiet
```

### Continuous Integration
```powershell
.\scripts\Build.ps1 -Clean -Verbose
.\scripts\Test.ps1 -NoBuild -Verbosity normal
```

### Local Development
```powershell
# Quick iteration cycle
.\scripts\Build.ps1
.\scripts\Test.ps1 -NoBuild -Filter "YourTestName"
```

---

## Notes

- All scripts automatically locate project files recursively
- Scripts use `$PSScriptRoot` to work from any directory
- Color-coded output for better readability (Green = success, Red = error, Yellow = warning)
- Test output is captured and parsed for summary display
- Scripts are designed to be generic and reusable across different projects

