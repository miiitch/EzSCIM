# Vérification rapide de la compilation et des tests

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Vérification de la Compilation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Compilation
Write-Host "Compilation du projet de tests..." -ForegroundColor Yellow
$buildOutput = dotnet build ScimAPI.Tests/ScimAPI.Tests.csproj 2>&1 | Out-String

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Compilation réussie!" -ForegroundColor Green
} else {
    Write-Host "✗ Erreur de compilation!" -ForegroundColor Red
    Write-Host $buildOutput -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Exécution d'un Test Simple" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test d'un test simple
Write-Host "Exécution du test ScimUserBase_ShouldHaveSystemProperties..." -ForegroundColor Yellow
$testOutput = dotnet test ScimAPI.Tests/ScimAPI.Tests.csproj --filter "FullyQualifiedName~ScimUserBase_ShouldHaveSystemProperties" --nologo 2>&1 | Out-String

if ($testOutput -match "Passed") {
    Write-Host "✓ Test réussi!" -ForegroundColor Green
} elseif ($testOutput -match "Failed") {
    Write-Host "✗ Test échoué!" -ForegroundColor Red
    Write-Host $testOutput -ForegroundColor Red
} else {
    Write-Host "⚠ Test exécuté (vérifier manuellement)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✓ Vérification Terminée" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pour exécuter tous les tests des classes de base:" -ForegroundColor Cyan
Write-Host "  .\Run-BaseClassesTests.ps1" -ForegroundColor White
Write-Host ""
