#!/bin/bash
# Script de vérification de l'implémentation JWT

echo "✅ Vérification de l'Implémentation JWT Bearer Token"
echo "=================================================="
echo ""

SCIMWORK_DIR="C:\Users\MichelPerfetti\src\private\scimwork"
ERRORS=0

# Fonction pour vérifier un fichier
check_file() {
    local file="$1"
    local description="$2"
    if [ -f "$file" ]; then
        echo "✓ $description"
        return 0
    else
        echo "✗ $description - MANQUANT: $file"
        ((ERRORS++))
        return 1
    fi
}

# Fonction pour vérifier un contenu dans un fichier
check_content() {
    local file="$1"
    local pattern="$2"
    local description="$3"
    if grep -q "$pattern" "$file" 2>/dev/null; then
        echo "✓ $description"
        return 0
    else
        echo "✗ $description - MANQUANT dans: $file"
        ((ERRORS++))
        return 1
    fi
}

echo "📁 Fichiers Créés:"
check_file "$SCIMWORK_DIR/ScimAPI/Services/JwtTokenService.cs" "JwtTokenService.cs"
check_file "$SCIMWORK_DIR/ScimAPI/Authentication/JwtBearerTokenAuthenticationHandler.cs" "JwtBearerTokenAuthenticationHandler.cs"
check_file "$SCIMWORK_DIR/ScimAPI.Tests/AuthenticationTestHelper.cs" "AuthenticationTestHelper.cs"
check_file "$SCIMWORK_DIR/IMPLEMENTATION_SUMMARY.md" "IMPLEMENTATION_SUMMARY.md"
check_file "$SCIMWORK_DIR/AUTHENTICATION_SETUP.md" "AUTHENTICATION_SETUP.md"
check_file "$SCIMWORK_DIR/TODO_AUTH.md" "TODO_AUTH.md"
check_file "$SCIMWORK_DIR/appsettings.Production.json" "appsettings.Production.json"
check_file "$SCIMWORK_DIR/test-auth.ps1" "test-auth.ps1"
check_file "$SCIMWORK_DIR/test-auth.sh" "test-auth.sh"

echo ""
echo "🔧 Configurations Modifiées:"
check_file "$SCIMWORK_DIR/ScimAPI/appsettings.json" "appsettings.json"
check_file "$SCIMWORK_DIR/ScimAPI/appsettings.Development.json" "appsettings.Development.json"
check_file "$SCIMWORK_DIR/ScimAPI/Program.cs" "Program.cs"
check_file "$SCIMWORK_DIR/ScimAPI/Controllers/UsersController.cs" "UsersController.cs"
check_file "$SCIMWORK_DIR/ScimAPI/Controllers/GroupsController.cs" "GroupsController.cs"
check_file "$SCIMWORK_DIR/ScimAPI/Controllers/ScimConfigController.cs" "ScimConfigController.cs"

echo ""
echo "📦 Dépendances NuGet:"
check_content "$SCIMWORK_DIR/ScimAPI/ScimAPI.csproj" "System.IdentityModel.Tokens.Jwt" "System.IdentityModel.Tokens.Jwt"
check_content "$SCIMWORK_DIR/ScimAPI/ScimAPI.csproj" "Microsoft.IdentityModel.Tokens" "Microsoft.IdentityModel.Tokens"
check_content "$SCIMWORK_DIR/ScimAPI/ScimAPI.csproj" "Microsoft.AspNetCore.Authentication.JwtBearer" "Microsoft.AspNetCore.Authentication.JwtBearer"
check_content "$SCIMWORK_DIR/ScimAPI/ScimAPI.csproj" "Azure.Identity" "Azure.Identity"
check_content "$SCIMWORK_DIR/ScimAPI/ScimAPI.csproj" "Azure.Security.KeyVault.Secrets" "Azure.Security.KeyVault.Secrets"

echo ""
echo "🔐 Imports JWT:"
check_content "$SCIMWORK_DIR/ScimAPI/Program.cs" "JwtBearerTokenAuthenticationHandler" "Import JwtBearerTokenAuthenticationHandler"
check_content "$SCIMWORK_DIR/ScimAPI/Program.cs" "AddAuthentication" "Configuration authentification"
check_content "$SCIMWORK_DIR/ScimAPI/Program.cs" "AddAzureKeyVault" "Configuration Azure Key Vault"
check_content "$SCIMWORK_DIR/ScimAPI/Program.cs" "UseAuthentication" "Middleware UseAuthentication"

echo ""
echo "🛡️  Attributs Authorize:"
check_content "$SCIMWORK_DIR/ScimAPI/Controllers/UsersController.cs" "[Authorize]" "[Authorize] sur UsersController"
check_content "$SCIMWORK_DIR/ScimAPI/Controllers/GroupsController.cs" "[Authorize]" "[Authorize] sur GroupsController"
check_content "$SCIMWORK_DIR/ScimAPI/Controllers/ScimConfigController.cs" "[Authorize]" "[Authorize] sur ScimConfigController"

echo ""
echo "🧪 Tests Mocked:"
check_content "$SCIMWORK_DIR/ScimAPI.Tests/UsersControllerTests.cs" "AuthenticationTestHelper" "AuthenticationTestHelper dans UsersControllerTests"
check_content "$SCIMWORK_DIR/ScimAPI.Tests/GroupsControllerTests.cs" "AuthenticationTestHelper" "AuthenticationTestHelper dans GroupsControllerTests"

echo ""
echo "📚 Documentation:"
check_content "$SCIMWORK_DIR/ENTRA_INTEGRATION.md" "JWT Bearer Token" "Documentation JWT dans ENTRA_INTEGRATION.md"
check_content "$SCIMWORK_DIR/ENTRA_INTEGRATION.md" "/scim/auth/token" "Documentation endpoint test dans ENTRA_INTEGRATION.md"

echo ""
echo "=================================================="

if [ $ERRORS -eq 0 ]; then
    echo "✅ TOUS LES FICHIERS ET CONFIGURATIONS SONT EN PLACE!"
    echo ""
    echo "Prochaines étapes:"
    echo "1. cd C:\Users\MichelPerfetti\src\private\scimwork"
    echo "2. dotnet test (exécuter les tests)"
    echo "3. dotnet run (démarrer l'application)"
    echo "4. .\test-auth.ps1 (tester l'authentification)"
    exit 0
else
    echo "❌ $ERRORS ERREUR(S) DÉTECTÉE(S)"
    echo ""
    echo "Vérifier les fichiers listés ci-dessus"
    exit 1
fi
