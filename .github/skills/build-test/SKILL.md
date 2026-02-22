---
name: build-test
description: GitHub Copilot skill for building and testing the solution.
---
# EzSCIM Build & Test
GitHub Copilot skill for building and testing the EzSCIM SCIM server.
## build
Build .NET projects with flexible configuration.
**Script:** `build.ps1`
**Parameters:**
- `Project` (string): Project name without .csproj (default: "EzSCIM.IntegrationTests")
- `Configuration` (string): Debug or Release (default: "Release")
- `Clean` (switch): Clean before building
- `Verbose` (switch): Detailed build output
- `NoRestore` (switch): Skip package restore
**Examples:**
```powershell
@skill build
@skill build -Project "EzSCIM" -Clean
@skill build -Configuration Debug -Verbose
```
---
## test
Run tests with flexible filtering and configuration.
**Script:** `test.ps1`
**Parameters:**
- `Project` (string): Test project name (default: "EzSCIM.IntegrationTests")
- `Filter` (string): Test name filter pattern
- `Configuration` (string): Debug or Release (default: "Release")
- `Verbosity` (string): quiet, minimal, normal, detailed, diagnostic (default: "normal")
- `NoBuild` (switch): Skip build step
- `Rebuild` (switch): Clean and rebuild before testing
**Examples:**
```powershell
@skill test -Filter "Patch"
@skill test -Filter "PatchUser_ReplaceAllScalarAttributes" -Verbosity detailed
@skill test -NoBuild -Filter "ScimValidatorComplianceTests"
```
---
## check
Check for compilation errors in the project.
**Script:** `check-errors.ps1`
**Parameters:**
- `Project` (string): Project name to check (default: "EzSCIM.IntegrationTests")
- `Files` (array): Specific files to highlight in output
**Examples:**
```powershell
@skill check
@skill check -Project "EzSCIM"
```
---
## quick-test
Fast test iteration with minimal configuration.
**Script:** `quick-test.ps1`
**Parameters:**
- `TestPattern` (string): Test name pattern to filter (default: "Patch")
**Examples:**
```powershell
@skill quick-test
@skill quick-test -TestPattern "PatchUser_Remove"
```
---
## verify
Quick build verification and summary.
**Script:** `verify.ps1`
**Parameters:** None
**Examples:**
```powershell
@skill verify
```
---
## Common Workflows
### After Code Changes
```powershell
@skill check          # Check for compilation errors
@skill build          # Compile the project
@skill test -Filter "Patch" -NoBuild   # Run related tests
```
### Debug Failing Test
```powershell
@skill test -Filter "TestName" -Verbosity diagnostic -NoBuild
```
### Full Validation
```powershell
@skill build -Clean
@skill test -NoBuild -Verbosity detailed
```
### Quick Iteration
```powershell
@skill build
@skill quick-test -TestPattern "YourTest"
```
---
## Technical Details
- **Language:** PowerShell 5.1+
- **Framework:** .NET 8.0+
- **Exit Codes:** 0 (success), 1 (failure)
- **Output:** Color-coded console output (Green=success, Red=error, Yellow=warning)
- **Location:** `.github/skills/build-test/`
---
## Integration
These scripts work with:
- GitHub Copilot CLI (`@skill` commands)
- Local PowerShell execution
- CI/CD pipelines
- Manual command-line usage
All scripts use relative paths and auto-locate project files for portability.
---
## Version
1.0.0 (2026-02-23)
