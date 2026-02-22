# Scripts Index

PowerShell automation scripts for EzSCIM development workflow.

## Quick Start

```powershell
# Verify everything works
.\scripts\Verify.ps1

# Run PATCH tests
.\scripts\Test.ps1 -Filter "Patch"
```

## Available Scripts

| Script | Purpose | Key Parameters |
|--------|---------|----------------|
| `Build.ps1` | Compile projects | `-Project`, `-Configuration`, `-Clean` |
| `Test.ps1` | Run tests with filtering | `-Filter`, `-Verbosity`, `-NoBuild` |
| `Check-Errors.ps1` | Check compilation errors | `-Project`, `-Files` |
| `Quick-Test.ps1` | Fast test iteration | `-TestPattern` |
| `Verify.ps1` | Quick build verification | _(none)_ |

## Common Commands

### Development Workflow
```powershell
# 1. Check errors
.\scripts\Check-Errors.ps1

# 2. Build
.\scripts\Build.ps1

# 3. Run tests
.\scripts\Test.ps1 -Filter "Patch" -NoBuild
```

### Specific Test
```powershell
.\scripts\Test.ps1 -Filter "PatchUser_ReplaceAllScalarAttributes" -Verbosity detailed
```

### Quick Iteration
```powershell
.\scripts\Quick-Test.ps1 -TestPattern "PatchUser_RemoveDisplayName"
```

### Clean Build
```powershell
.\scripts\Build.ps1 -Clean -Verbose
```

## Documentation

See `scripts/README.md` for complete documentation with:
- Full parameter descriptions
- Usage examples
- Common workflows
- Integration patterns

## Exit Codes

All scripts return:
- `0` = Success
- `1` = Failure

Enable command chaining:
```powershell
.\scripts\Build.ps1 && .\scripts\Test.ps1 -NoBuild
```

