# TODO - Prochaines Étapes Authentification JWT

## Avant Déploiement en Production

### 1. Tests Intégration
- [ ] Exécuter `dotnet test` et vérifier que tous les tests passent
- [ ] Tester avec PowerShell: `.\test-auth.ps1`
- [ ] Générer un token en développement et valider sa structure sur [jwt.io](https://jwt.io)
- [ ] Tester appels API avec Postman/Insomnia

### 2. Configuration Azure Key Vault
- [ ] Créer ou utiliser Key Vault existant
- [ ] Créer secret `Jwt-SecretKey` avec clé de 32+ caractères
- [ ] Configurer Managed Identity pour l'application
- [ ] Tester accès Key Vault: `az keyvault secret show --name Jwt-SecretKey`
- [ ] Vérifier logs pour confirmer chargement clé depuis Key Vault

### 3. Configuration Entra ID
- [ ] Générer JWT valide signé avec clé secrète
- [ ] Tester Token dans Entra: **Test Connection** doit passer
- [ ] Valider que les endpoints retournent 401 sans token
- [ ] Valider que les endpoints retournent 200 avec token valide

### 4. Sécurité
- [ ] Vérifier que token n'est pas loggé nulle part
- [ ] Vérifier que clé secrète n'est pas en plaintext (sauf Dev)
- [ ] Configurer expiration tokens appropriée
- [ ] Ajouter monitoring pour echecs authentification
- [ ] Vérifier HTTPS est forcé en production

### 5. Documentation
- [ ] Documenter processus génération token pour ops
- [ ] Documenter rotation clé secrète
- [ ] Documenter troubleshooting authentification
- [ ] Mettre à jour runbook déploiement

## Optimisations Futures

### 1. Amélioration Tokens
- [ ] Ajouter claims additionnels (aud, iss)
- [ ] Implémenter refresh tokens
- [ ] Ajouter token revocation list (blacklist)
- [ ] Implémenter rate limiting par token

### 2. Scalabilité
- [ ] Implémenter token caching pour validation
- [ ] Ajouter métriques Prometheus pour tokens
- [ ] Implémenter circuit breaker pour Key Vault

### 3. Authentification Multi-Tenant
- [ ] Supporter plusieurs secrets par tenant
- [ ] Ajouter tenant-id dans claims
- [ ] Implémenter isolation données par tenant

### 4. Intégrations Avancées
- [ ] Supporter Entra ID tokens directement (au lieu de JWT généré)
- [ ] Implémenter OAuth2 flow complet
- [ ] Supporter multiple identity providers

## Dépannage - Common Issues

### Issue: Token invalide en production
```
Symptôme: HTTP 401 partout avec token que devrait être valide
Cause: Clé secrète ne correspond pas entre génération et validation
Action: 
  1. Vérifier clé secrète en Key Vault
  2. Régénérer JWT avec clé correcte
  3. Tester sur jwt.io avec même clé
```

### Issue: Key Vault non accessible
```
Symptôme: Application ne démarre pas en prod, logs montrent erreur Key Vault
Cause: Managed Identity pas configurée ou sans permissions
Action:
  1. Vérifier Managed Identity assignée à l'app
  2. Vérifier secret existe dans Key Vault
  3. Vérifier policy Key Vault donne get/list permissions
  4. Vérifier URI Key Vault correct dans config
```

### Issue: Endpoint /scim/auth/token retourne 403
```
Symptôme: En production, tentative obtenir token retourne Forbidden
Cause: C'est voulu! Endpoint disponible seulement en dev
Action: Générer token via application CLI sécurisée ou script local
```

## Checklist de Sécurité

Avant de mettre en production:

- [ ] JWT utilise HS256 avec clé 32+ caractères
- [ ] Clé secrète ne jamais commitée dans Git
- [ ] Clé secrète stockée dans Azure Key Vault
- [ ] Managed Identity configurée
- [ ] Tokens expirent après temps raisonnable (60 min recommandé)
- [ ] HTTPS forcé (UseHttpsRedirection active)
- [ ] Authentification active sur TOUS les endpoints
- [ ] Endpoint /scim/auth/token retourne 403 en prod
- [ ] Logs d'authentification activés pour audit
- [ ] Erreurs d'authentification loggées sans exposer détails sensibles
- [ ] Rate limiting recommandé (non implémenté, voir améliorations)
- [ ] Certificats SSL/TLS valides et à jour

## Monitoring

Ajouter ces métriques au monitoring:

1. **Authentification Failures**
   - Nombre requêtes 401
   - Raison échec (token invalide, expiré, manquant)

2. **Token Generation**
   - Nombre tokens générés par jour
   - Utilisation endpoint /scim/auth/token

3. **Performance**
   - Temps validation token (devrait être <1ms)
   - Temps accès Key Vault

4. **Security**
   - Alertes sur patterns anormaux (ex: trop 401)
   - Tracking tentatives avec tokens invalides

## Références Implémentation

- `ScimAPI/Services/JwtTokenService.cs` - Service génération/validation
- `ScimAPI/Authentication/JwtBearerTokenAuthenticationHandler.cs` - Handler custom
- `ScimAPI/Controllers/ScimConfigController.cs` - Endpoint test `/scim/auth/token`
- `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json` - Configs
- `AUTHENTICATION_SETUP.md` - Guide complet
- `test-auth.ps1`, `test-auth.sh` - Scripts de test

