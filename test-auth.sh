#!/bin/bash
# Script pour tester l'authentification JWT de l'API SCIM

set -e

API_BASE_URL="${1:-https://localhost:7001}"

echo "🔐 Test d'Authentification JWT SCIM API"
echo "========================================="
echo ""
echo "URL API: $API_BASE_URL"
echo ""

# 1. Générer un token
echo "1️⃣  Génération d'un token JWT..."
TOKEN_RESPONSE=$(curl -s -X GET "$API_BASE_URL/scim/auth/token")
TOKEN=$(echo "$TOKEN_RESPONSE" | jq -r '.token')
EXPIRES=$(echo "$TOKEN_RESPONSE" | jq -r '.expiresIn')

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
    echo "❌ Erreur: Impossible de générer un token"
    echo "Réponse: $TOKEN_RESPONSE"
    exit 1
fi

echo "✅ Token généré avec succès!"
echo "   Expiration: $EXPIRES"
echo ""

# 2. Utiliser le token pour accéder à /scim/ServiceProviderConfig
echo "2️⃣  Test d'accès à /scim/ServiceProviderConfig..."
CONFIG_RESPONSE=$(curl -s -X GET "$API_BASE_URL/scim/ServiceProviderConfig" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/scim+json")

if echo "$CONFIG_RESPONSE" | jq . > /dev/null 2>&1; then
    echo "✅ Accès autorisé!"
    echo "   Format réponse: $(echo "$CONFIG_RESPONSE" | jq '.filter.supported')"
else
    echo "❌ Erreur: Réponse non valide"
    echo "Réponse: $CONFIG_RESPONSE"
    exit 1
fi
echo ""

# 3. Tester sans token (doit retourner 401)
echo "3️⃣  Test sans token (doit retourner 401)..."
NO_TOKEN_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$API_BASE_URL/scim/Users")
HTTP_CODE=$(echo "$NO_TOKEN_RESPONSE" | tail -n1)
BODY=$(echo "$NO_TOKEN_RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "401" ]; then
    echo "✅ Correctement rejeté (HTTP 401)"
else
    echo "⚠️  Erreur: Code HTTP attendu 401, reçu $HTTP_CODE"
fi
echo ""

# 4. Accéder à /scim/Users avec token
echo "4️⃣  Test d'accès à /scim/Users..."
USERS_RESPONSE=$(curl -s -X GET "$API_BASE_URL/scim/Users" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/scim+json")

if echo "$USERS_RESPONSE" | jq . > /dev/null 2>&1; then
    TOTAL=$(echo "$USERS_RESPONSE" | jq '.totalResults')
    echo "✅ Accès autorisé!"
    echo "   Total utilisateurs: $TOTAL"
else
    echo "❌ Erreur: Réponse non valide"
    echo "Réponse: $USERS_RESPONSE"
    exit 1
fi
echo ""

# 5. Accéder à /scim/Schemas
echo "5️⃣  Test d'accès à /scim/Schemas..."
SCHEMAS_RESPONSE=$(curl -s -X GET "$API_BASE_URL/scim/Schemas" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/scim+json")

if echo "$SCHEMAS_RESPONSE" | jq . > /dev/null 2>&1; then
    COUNT=$(echo "$SCHEMAS_RESPONSE" | jq '.schemas | length')
    echo "✅ Accès autorisé!"
    echo "   Schémas disponibles: $COUNT"
else
    echo "❌ Erreur: Réponse non valide"
fi
echo ""

echo "🎉 Tous les tests sont passés!"
echo ""
echo "📝 Utilisation:"
echo "   Pour utiliser le token dans les requêtes:"
echo "   curl -H \"Authorization: Bearer $TOKEN\" https://..."
echo ""
echo "⏱️  Durée d'expiration: $EXPIRES"
