# SCIM Run 06 - Quick Reference Card

## 🚨 The Bug

**What**: PATCH operations with filtered paths (`emails[primary eq true].value`) don't persist  
**Where**: `ScimPatchApplier.ApplyOperation` (line 138)  
**Why**: Property mapping lookup fails → returns `false` silently  
**Impact**: SCIM Validator Run 06 fails, data loss

## 📁 Documentation

| Document | Purpose |
|----------|---------|
| `scim-run06-index.md` | Navigation index |
| `scim-run06-complete-summary.md` | Executive summary |
| `scim-run06-patch-error-analysis.md` | Technical analysis |
| `scim-run06-tests-implementation.md` | Test guide |
| `IMPLEMENTATION-RUN06-FR.md` | Legacy filename, English implementation summary |

## 🧪 Tests Created

**Total**: 9 tests (~450 lines)  
**File**: `EzSCIM.IntegrationTests/ScimValidatorComplianceTests.cs`

### Main Test
- `PatchUser_ReplaceFilteredMultiValuedAttributes_Run06_ShouldPersistAll`

### Verification Tests (8)
- Add/Remove/Replace operations
- Filtered paths with different expressions
- User and Group PATCH operations

## 🎯 Quick Commands

```powershell
# Run Run 06 test
dotnet test --filter "FullyQualifiedName~Run06"

# Run all PATCH tests
dotnet test --filter "FullyQualifiedName~PatchUser"

# Build tests
dotnet build EzSCIM.IntegrationTests
```

## 🔧 Next Steps

1. ⏳ Run tests to verify they fail
2. ⏳ Implement fix (3 options in analysis doc)
3. ⏳ Verify tests pass
4. ⏳ Re-run SCIM validator

## 📊 Status

✅ Tests implemented  
✅ Documentation complete  
✅ Compilation successful  
⏳ Fix pending

---

**Created**: 2026-02-22  
**See**: `scim-run06-index.md` for full navigation
