# Script pour exécuter les tests des classes de base ScimUserBase et ScimGroupBase

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Tests des Classes de Base SCIM" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$testProject = "ScimAPI.Tests\ScimAPI.Tests.csproj"

# Exécuter tous les tests des classes de base
Write-Host "Exécution des tests pour ScimUserBase et ScimGroupBase..." -ForegroundColor Yellow
Write-Host ""

$testMethods = @(
    "ScimUserBase_ShouldHaveRequiredAttributesOnly",
    "ScimUserBase_UserName_ShouldBeRequired",
    "ScimUserBase_ShouldHaveSystemProperties",
    "ScimGroupBase_ShouldHaveRequiredAttributesOnly",
    "ScimGroupBase_DisplayName_ShouldBeRequired",
    "ScimGroupBase_ShouldHaveSystemProperties",
    "ScimUser_ShouldHaveMoreAttributesThanBase",
    "ScimGroup_ShouldHaveMoreAttributesThanBase",
    "ScimUserBase_And_ScimUser_ShouldHaveSameSchemaId",
    "ScimGroupBase_And_ScimGroup_ShouldHaveSameSchemaId"
)

$passed = 0
$failed = 0
$total = $testMethods.Count

foreach ($test in $testMethods) {
    Write-Host "Test: $test" -ForegroundColor Cyan
    
    $result = dotnet test $testProject --filter "FullyQualifiedName~$test" --nologo --verbosity quiet 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ PASSED" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "  ✗ FAILED" -ForegroundColor Red
        Write-Host "  Details: $result" -ForegroundColor Red
        $failed++
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Résumé des Tests" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total:  $total" -ForegroundColor White
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
Write-Host ""

if ($failed -eq 0) {
    Write-Host "✓ Tous les tests sont passés avec succès!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "✗ Certains tests ont échoué." -ForegroundColor Red
    exit 1
}
