$ErrorActionPreference = "Continue"

Write-Host "Building project..." -ForegroundColor Yellow
dotnet build C:\Users\MichelPerfetti\src\private\scimwork\EzSCIM.IntegrationTests\EzSCIM.IntegrationTests.csproj

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Running test..." -ForegroundColor Yellow
dotnet test C:\Users\MichelPerfetti\src\private\scimwork\EzSCIM.IntegrationTests\EzSCIM.IntegrationTests.csproj `
    --filter "FullyQualifiedName~PatchUser_ReplaceFilteredEmailPrimaryValue_ShouldPersist" `
    --logger "console;verbosity=detailed" `
    --no-build `
    2>&1 | Tee-Object -FilePath "C:\Users\MichelPerfetti\src\private\scimwork\test-output.txt"

Write-Host "`nTest execution complete. Check test-output.txt for details." -ForegroundColor Green

