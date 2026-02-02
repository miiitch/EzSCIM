# Test de validation de la génération automatique des schémas SCIM
# Ce script vérifie que les schémas User et Group sont correctement générés

Write-Host "=== Test de validation de la génération de schémas SCIM ===" -ForegroundColor Cyan
Write-Host ""

# Compiler le projet
Write-Host "1. Compilation du projet..." -ForegroundColor Yellow
cd "$PSScriptRoot\ScimAPI"
$buildResult = dotnet build --nologo --verbosity quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Échec de la compilation" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}
Write-Host "✅ Compilation réussie" -ForegroundColor Green
Write-Host ""

# Démarrer l'application en arrière-plan
Write-Host "2. Démarrage de l'application..." -ForegroundColor Yellow
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5555"
$process = Start-Process -FilePath "dotnet" -ArgumentList "run --no-build --urls http://localhost:5555" -PassThru -WorkingDirectory "$PSScriptRoot\ScimAPI" -WindowStyle Hidden

# Attendre que l'application démarre
Write-Host "   Attente du démarrage (10 secondes)..." -ForegroundColor Gray
Start-Sleep -Seconds 10

try {
    # Générer un token JWT pour l'authentification
    Write-Host "3. Génération du token d'authentification..." -ForegroundColor Yellow
    try {
        $tokenResponse = Invoke-RestMethod -Uri "http://localhost:5555/scim/auth/token" -Method Get -ErrorAction Stop
        $token = $tokenResponse.token
        Write-Host "✅ Token généré" -ForegroundColor Green
    } catch {
        Write-Host "⚠️  Impossible de générer un token, tentative sans authentification..." -ForegroundColor Yellow
        $token = $null
    }
    Write-Host ""

    # Tester l'endpoint /Schemas
    Write-Host "4. Test de l'endpoint GET /scim/Schemas..." -ForegroundColor Yellow
    $headers = @{}
    if ($token) {
        $headers["Authorization"] = "Bearer $token"
    }
    
    try {
        $schemas = Invoke-RestMethod -Uri "http://localhost:5555/scim/Schemas" -Method Get -Headers $headers -ErrorAction Stop
        Write-Host "✅ Endpoint accessible" -ForegroundColor Green
        Write-Host "   Nombre de schémas: $($schemas.Count)" -ForegroundColor Gray
        
        foreach ($schema in $schemas) {
            Write-Host ""
            Write-Host "   Schéma: $($schema.name)" -ForegroundColor Cyan
            Write-Host "     ID: $($schema.id)" -ForegroundColor Gray
            Write-Host "     Description: $($schema.description)" -ForegroundColor Gray
            Write-Host "     Nombre d'attributs: $($schema.attributes.Count)" -ForegroundColor Gray
            
            if ($schema.attributes.Count -gt 0) {
                Write-Host "     Premiers attributs:" -ForegroundColor Gray
                $schema.attributes | Select-Object -First 5 | ForEach-Object {
                    $required = if ($_.required) { " [REQUIRED]" } else { "" }
                    $multiValued = if ($_.multiValued) { " [MULTI-VALUED]" } else { "" }
                    Write-Host "       - $($_.name) ($($_.type))$required$multiValued" -ForegroundColor DarkGray
                    
                    if ($_.subAttributes -and $_.subAttributes.Count -gt 0) {
                        Write-Host "         Sous-attributs: $($_.subAttributes.Count)" -ForegroundColor DarkGray
                    }
                }
            }
        }
        
        Write-Host ""
        
        # Validation
        $userSchema = $schemas | Where-Object { $_.id -eq "urn:ietf:params:scim:schemas:core:2.0:User" }
        $groupSchema = $schemas | Where-Object { $_.id -eq "urn:ietf:params:scim:schemas:core:2.0:Group" }
        
        if ($userSchema) {
            Write-Host "✅ Schéma User trouvé avec $($userSchema.attributes.Count) attributs" -ForegroundColor Green
        } else {
            Write-Host "❌ Schéma User manquant" -ForegroundColor Red
        }
        
        if ($groupSchema) {
            Write-Host "✅ Schéma Group trouvé avec $($groupSchema.attributes.Count) attributs" -ForegroundColor Green
        } else {
            Write-Host "❌ Schéma Group manquant" -ForegroundColor Red
        }
        
        # Vérifier que userName est présent dans User schema
        $userNameAttr = $userSchema.attributes | Where-Object { $_.name -eq "userName" }
        if ($userNameAttr) {
            Write-Host "✅ Attribut 'userName' trouvé (required: $($userNameAttr.required))" -ForegroundColor Green
        } else {
            Write-Host "❌ Attribut 'userName' manquant" -ForegroundColor Red
        }
        
        # Vérifier les sous-attributs complexes (ex: name.givenName)
        $nameAttr = $userSchema.attributes | Where-Object { $_.name -eq "name" }
        if ($nameAttr -and $nameAttr.subAttributes) {
            Write-Host "✅ Attribut complexe 'name' avec $($nameAttr.subAttributes.Count) sous-attributs" -ForegroundColor Green
            $givenName = $nameAttr.subAttributes | Where-Object { $_.name -eq "givenName" }
            if ($givenName) {
                Write-Host "   ✅ Sous-attribut 'givenName' trouvé" -ForegroundColor Green
            }
        } else {
            Write-Host "❌ Attribut complexe 'name' manquant ou sans sous-attributs" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "❌ Erreur lors de l'appel à l'API: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host $_.Exception -ForegroundColor DarkRed
    }
    
} finally {
    # Arrêter l'application
    Write-Host ""
    Write-Host "5. Arrêt de l'application..." -ForegroundColor Yellow
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Application arrêtée" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Test terminé ===" -ForegroundColor Cyan
