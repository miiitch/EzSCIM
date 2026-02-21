# Script de migration des fichiers Markdown vers docs/
# Avec renommage selon convention docs/<theme>/<topic>.md

$root = "C:\Users\MichelPerfetti\src\private\scimwork"

# Mapping: ancien_fichier -> (destination, nouveau_nom)
$fileMapping = @{
    # Auth
    "AUTHENTICATION_SETUP.md" = @("docs/auth", "setup.md")
    "AUTH_INDEX.md" = @("docs/auth", "index.md")
    "QUICK-FIX-JWT-SERVICE.md" = @("docs/auth", "jwt-service-quick-fix.md")
    "RESOLUTION-GUIDE-JWT-DI.md" = @("docs/auth", "jwt-di-resolution.md")
    "TODO_AUTH.md" = @("docs/auth", "pre-production-checklist.md")
    
    # Guides généraux
    "QUICKSTART.md" = @("docs/guides", "quickstart.md")
    "DEVELOPMENT_INSTRUCTIONS.md" = @("docs/guides", "development-setup.md")
    "POWERSHELL-SCRIPTS-README.md" = @("docs/guides", "powershell-scripts.md")
    "USEFUL-COMMANDS.md" = @("docs/guides", "useful-commands.md")
    "VISUAL-SEPARATION-GUIDE.md" = @("docs/guides", "visual-separation.md")
    "NEXT-TASKS.md" = @("docs/guides", "next-tasks.md")
    "NEXT-TASKS-CHECKLIST.md" = @("docs/guides", "next-tasks-checklist.md")
    "QUICK-GUIDE-PROVIDER-MODES.md" = @("docs/guides", "provider-modes.md")
    "DEVELOPMENT_INSTRUCTIONS.md" = @("docs/guides", "development-setup.md")
    
    # Filters
    "SCIM_FILTERS.md" = @("docs/filters", "overview.md")
    "SCIM-FILTER-DOCUMENTATION.md" = @("docs/filters", "reference.md")
    "SCIM-FILTER-PARSER-README.md" = @("docs/filters", "parser.md")
    "FILTER-IMPLEMENTATION-GUIDE.md" = @("docs/filters", "implementation-guide.md")
    "FILTER-QUICK-EXAMPLES.md" = @("docs/filters", "examples.md")
    "FILTER-VALUE-TYPES.md" = @("docs/filters", "value-types.md")
    "FILTER-VALUE-TYPES-QUICK.md" = @("docs/filters", "value-types-quick-reference.md")
    "NESTED-FILTERS-DOCUMENTATION.md" = @("docs/filters", "nested-filters.md")
    "FILTER-ERRORS-DOCUMENTATION.md" = @("docs/filters", "error-handling.md")
    "URL-ENCODING-GUIDE.md" = @("docs/filters", "url-encoding.md")
    
    # Migration/Repository
    "QUICK-START-REPOSITORY-INTEGRATION.md" = @("docs/migration", "quick-start-repository.md")
    "REPOSITORY-MAPPING-README.md" = @("docs/migration", "repository-mapping-overview.md")
    "REPOSITORY-MAPPING-INDEX.md" = @("docs/migration", "repository-mapping-index.md")
    "REPOSITORY-ADAPTER-GUIDE.md" = @("docs/migration", "repository-adapter-guide.md")
    "README-MAPPING.md" = @("docs/migration", "mapping-readme.md")
    "GROUPS-AND-CONSTANTS-EXTENSION-COMPLETE.md" = @("docs/migration", "groups-and-constants-extension.md")
    "INTERFACE-SEPARATION.md" = @("docs/migration", "interface-separation.md")
    
    # Schema
    "SCHEMA-SYSTEM-README.md" = @("docs/schema", "system-overview.md")
    "SCHEMA-EXTENSION-GUIDE.md" = @("docs/schema", "extension-guide.md")
    "SCIM-MODELS-REQUIRED-OPTIONAL.md" = @("docs/schema", "models-required-optional.md")
    "EXPECTED-ACTUAL-PATTERN.md" = @("docs/schema", "expected-actual-pattern.md")
    
    # Tests
    "TEST-SUITE-UPDATE-COMPLETE.md" = @("docs/tests", "test-suite-update.md")
    "FILTER-TESTS-COMPLETE.md" = @("docs/tests", "filter-tests.md")
    "FILTER-ERROR-TESTS-DOCUMENTATION.md" = @("docs/tests", "filter-error-tests.md")
    "BASE-CLASSES-TESTS.md" = @("docs/tests", "base-classes-tests.md")
    "SUMMARY-BASE-CLASSES-TESTS.md" = @("docs/tests", "base-classes-summary.md")
    "ENTRA_INTEGRATION.md" = @("docs/tests", "entra-integration.md")
    
    # Status reports (Option B: inclure COMPLETION, DELIVERY, MIGRATION)
    "IMPLEMENTATION-STATUS.md" = @("docs/status", "implementation-status.md")
    "MIGRATION-SUMMARY.md" = @("docs/status", "migration-summary.md")
    "MIGRATION_COMPLETE.md" = @("docs/status", "migration-complete.md")
    "COMPLETION-REPORT.md" = @("docs/status", "completion-report.md")
    "SESSION-COMPLETE-SUMMARY.md" = @("docs/status", "session-summary.md")
    "TODAY-SUMMARY.md" = @("docs/status", "today-summary.md")
    "TESTS_SUMMARY.md" = @("docs/status", "tests-summary.md")
    "DELIVERY-SUMMARY-JWT-EXTENSION.md" = @("docs/status", "jwt-extension-delivery.md")
    "PROJECT-DELIVERY-MANIFEST.md" = @("docs/status", "delivery-manifest.md")
    "PHASE-4-COMPLETION.md" = @("docs/status", "phase-4-completion.md")
    
    # Archive - Fichiers *_COMPLETE.md
    "FILTER-EXPRESSION-INTEGRATION-COMPLETE.md" = @("docs/archive", "filter-expression-integration.md")
    "FILTER-TESTS-COMPLETE.md" = @("docs/archive", "filter-tests.md")
    "FILTER-ERROR-TESTING-COMPLETE.md" = @("docs/archive", "filter-error-testing.md")
    "NESTED-FILTERS-COMPLETE.md" = @("docs/archive", "nested-filters.md")
    "FINAL-IMPLEMENTATION-COMPLETE.md" = @("docs/archive", "final-implementation.md")
    "IMPLEMENTATION-COMPLETE.md" = @("docs/archive", "implementation.md")
    "IMPLEMENTATION-JWT-EXTENSION-COMPLETE.md" = @("docs/archive", "jwt-extension-implementation.md")
    "INTERFACE-SEPARATION-COMPLETE.md" = @("docs/archive", "interface-separation.md")
    "INTEGRATION-TESTS-COMPLETE.md" = @("docs/archive", "integration-tests.md")
    "INTEGRATION-TESTS-FINAL-STATUS.md" = @("docs/archive", "integration-tests-final.md")
    "TEST-SUITE-UPDATE-COMPLETE.md" = @("docs/archive", "test-suite-update.md")
    "SCHEMA-GENERATION-IMPLEMENTATION-COMPLETE.md" = @("docs/archive", "schema-generation-implementation.md")
    "SCHEMA-IMPLEMENTATION-COMPLETE.md" = @("docs/archive", "schema-implementation.md")
    "REPOSITORY-MAPPING-IMPLEMENTATION-COMPLETE.md" = @("docs/archive", "repository-mapping-implementation.md")
    "PATCH-CUSTOM-FIELDS-COMPLETE.md" = @("docs/archive", "patch-custom-fields.md")
    "CODE-CLEANUP-COMPLETE.md" = @("docs/archive", "code-cleanup.md")
    "ERROROR-IMPLEMENTATION-COMPLETE.md" = @("docs/archive", "erroror-implementation.md")
    "ENTRAID-TESTS-COMPLETE.md" = @("docs/archive", "entraid-tests.md")
    "POWERSHELL-COMPLETE.md" = @("docs/archive", "powershell.md")
    "ISSUE-RESOLVED-DUPLICATE-REGISTRATION.md" = @("docs/archive", "issue-duplicate-registration.md")
    "PROJECTS-NAMESPACE-FIXED.md" = @("docs/archive", "projects-namespace.md")
    "FINAL-SUMMARY.md" = @("docs/archive", "final-summary.md")
    "FINAL-PROJECTS-FIX.md" = @("docs/archive", "final-projects-fix.md")
    "FIX-DUPLICATE-SERVICE-REGISTRATION.md" = @("docs/archive", "fix-duplicate-registration.md")
    "INTEGRATION-TESTS-FIX-SCOPED-SERVICE.md" = @("docs/archive", "integration-tests-fix-scoped.md")
    "INTEGRATION-TESTS-FIXES-SUMMARY.md" = @("docs/archive", "integration-tests-fixes.md")
    "INTEGRATION-TESTS-SUMMARY.md" = @("docs/archive", "integration-tests-summary.md")
    "INTEGRATION-TESTS-STATUS.md" = @("docs/archive", "integration-tests-status.md")
    "FILTER-TESTS-REFACTORING.md" = @("docs/archive", "filter-tests-refactoring.md")
    "FILTER-ERROR-TESTING-COMPLETE.md" = @("docs/archive", "filter-error-testing.md")
    "FILTER-EXTENSIONS-REFACTORING.md" = @("docs/archive", "filter-extensions-refactoring.md")
    "FILTER-DOCUMENTATION-SUMMARY.md" = @("docs/archive", "filter-documentation-summary.md")
    "TESTS-COMPLETED.md" = @("docs/archive", "tests-completed.md")
    "TESTS-FIXED.md" = @("docs/archive", "tests-fixed.md")
    "TESTS-FIXES-REPORT.md" = @("docs/archive", "tests-fixes-report.md")
    "COMPILATION_FIXED.md" = @("docs/archive", "compilation-fixed-v1.md")
    "COMPILATION-FIXED.md" = @("docs/archive", "compilation-fixed-v2.md")
    "COMPILATION-FIX-BASE-TESTS.md" = @("docs/archive", "compilation-fix-base-tests.md")
    "AZURE-KEYVAULT-FIXED.md" = @("docs/archive", "azure-keyvault-fixed.md")
    "SCHEMA-GENERATION-DONE.md" = @("docs/archive", "schema-generation-done.md")
}

Write-Host "🚀 Démarrage de la migration des fichiers Markdown..." -ForegroundColor Green
Write-Host ""

$movedCount = 0
$skippedCount = 0
$errorCount = 0

foreach ($oldName in $fileMapping.Keys) {
    $sourcePath = Join-Path $root $oldName
    
    if (-not (Test-Path $sourcePath)) {
        Write-Host "⏭️  IGNORÉ: $oldName (fichier non trouvé)" -ForegroundColor Yellow
        $skippedCount++
        continue
    }
    
    $destination, $newName = $fileMapping[$oldName]
    $destDir = Join-Path $root $destination
    $destPath = Join-Path $destDir $newName
    
    # Créer le dossier destination s'il n'existe pas
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    }
    
    try {
        # Copier le fichier
        Copy-Item -Path $sourcePath -Destination $destPath -Force
        Write-Host "✅ DÉPLACÉ: $oldName → $destination/$newName" -ForegroundColor Cyan
        $movedCount++
    }
    catch {
        Write-Host "❌ ERREUR: $oldName - $_" -ForegroundColor Red
        $errorCount++
    }
}

Write-Host ""
Write-Host "📊 Résumé:" -ForegroundColor Green
Write-Host "  ✅ Fichiers déplacés: $movedCount"
Write-Host "  ⏭️  Fichiers ignorés: $skippedCount"
Write-Host "  ❌ Erreurs: $errorCount"
Write-Host ""

# Fichiers à conserver à la racine
Write-Host "📋 Fichiers conservés à la racine:" -ForegroundColor Yellow
@("README.md", "CHANGELOG.md", "DEVELOPMENT_INSTRUCTIONS.md", "START_HERE.md") | ForEach-Object {
    Write-Host "  - $_"
}

