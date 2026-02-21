# Instructions de Développement TestSCIM

## Gestion des Fichiers Temporaires

### Règle Générale
✅ **Tous les fichiers temporaires créés doivent être placés dans le dossier `tmp/` à la racine de la solution**

### Pourquoi ?
- Le dossier `tmp/` est défini dans le `.gitignore` et ne sera jamais commité
- Cela maintient la propreté du repository
- Les fichiers temporaires ne polluent pas l'historique Git

### Exemples de fichiers temporaires
- Résultats de tests
- Fichiers de log
- Fichiers de sortie des scripts
- Fichiers de cache ou temporaires
- Fichiers de débogage

### Comment l'implémenter dans le code
```csharp
// Bonne pratique
string tmpFolder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "tmp");
string logFile = Path.Combine(tmpFolder, "myapp.log");
File.WriteAllText(logFile, "log content");
```

## Exposition en DevTunnel

### ScimAPI - Exposition Anonyme
Le projet `TestSCIM.AppHost` expose le `ScimAPI` de manière anonyme via devtunnel.

**Configuration dans `Program.cs`:**
```csharp
builder.AddProject<Projects.ScimAPI>("scimapi")
    .WithExternalHttpEndpoints();  // Expose les endpoints publiquement
```

**Résultats:**
- Une URL publique est générée automatiquement
- Accessible de façon anonyme sans authentification
- Parfait pour les tests et démonstrations
- Les détails sont affichés dans la console lors du démarrage

## Accès au ScimAPI

### Localement
```
http://localhost:5000
```

### Via DevTunnel
Une URL publique sera affichée lors du démarrage:
```
https://<random-id>.devtunnels.ms/
```

## Ressources Importantes
- [SCIM_FILTERS.md](../SCIM_FILTERS.md) - Documentation des filtres SCIM
- [ENTRA_INTEGRATION.md](../ENTRA_INTEGRATION.md) - Intégration Entra ID
- [README.md](../README.md) - Documentation générale
