# PowerShell script to configure Azure Key Vault for JWT

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, HelpMessage="Name of the Key Vault")]
    [string]$KeyVaultName,
    
    [Parameter(HelpMessage="Resource group name")]
    [string]$ResourceGroup = "default",
    
    [Parameter(HelpMessage="Length of the secret key")]
    [int]$KeyLength = 32,
    
    [switch]$GenerateNewKey,
    
    [string]$ExistingKey = ""
)

Write-Host ""
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host "🔐 AZURE KEY VAULT CONFIGURATION - JWT SECRET" -ForegroundColor Cyan
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host ""

# Verify that Azure CLI is installed
Write-Host "🔍 Checking prerequisites..." -ForegroundColor Yellow

try {
    $azVersion = az version --query '"azure-cli"' --output tsv 2>&1
    Write-Host "  ✓ Azure CLI: $azVersion"
}
catch {
    Write-Host "  ✗ Azure CLI not found. Install: https://aka.ms/InstallAzureCliWindows"
    exit 1
}

Write-Host ""
Write-Host "🔑 Key Vault Configuration:" -ForegroundColor Yellow
Write-Host "  Vault: $KeyVaultName"
Write-Host "  Resource Group: $ResourceGroup"
Write-Host ""

# 1. Verify Azure connection
Write-Host "🔐 Verifying Azure connection..." -ForegroundColor Cyan

try {
    $currentUser = az account show --query "user.name" --output tsv 2>&1
    Write-Host "  ✓ Logged in as: $currentUser"
}
catch {
    Write-Host "  ✗ Not authenticated. Run: az login"
    exit 1
}

Write-Host ""

# 2. Verify that the Key Vault exists
Write-Host "🔍 Checking Key Vault..." -ForegroundColor Cyan

$vaultExists = az keyvault show --name $KeyVaultName --resource-group $ResourceGroup 2>&1
if (-not $vaultExists) {
    Write-Host "  ✗ Key Vault '$KeyVaultName' not found in '$ResourceGroup'"
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  1. Create a new vault:"
    Write-Host "     az keyvault create --name $KeyVaultName --resource-group $ResourceGroup"
    Write-Host "  2. Verify the name and resource group"
    Write-Host ""
    exit 1
}
Write-Host "  ✓ Key Vault found"

Write-Host ""

# 3. Generate or use the secret key
Write-Host "🔑 Generating secret key..." -ForegroundColor Cyan

if ($ExistingKey) {
    Write-Host "  📋 Using provided key"
    $secretKey = $ExistingKey
}
elseif ($GenerateNewKey) {
    # Generate a random key
    $secretKey = [System.Convert]::ToBase64String([System.Security.Cryptography.RNGCryptoServiceProvider]::new().GetBytes($KeyLength))
    # Keep only alphanumeric and safe symbols
    $secretKey = ($secretKey -replace '[^a-zA-Z0-9-_]', '') -replace '.{32}$' , { $_ }
    $secretKey = $secretKey.Substring(0, [Math]::Min($KeyLength, $secretKey.Length))
    
    Write-Host "  ✓ Key generated: ${secretKey.Substring(0,8)}...${secretKey.Substring(-8)}"
}
else {
    Write-Host "  ✗ No key provided. Use -GenerateNewKey or -ExistingKey"
    exit 1
}

Write-Host "  Length: $($secretKey.Length) characters"

# Verify length
if ($secretKey.Length -lt 32) {
    Write-Host "  ⚠️  WARNING: Key is too short (min. 32). Current length: $($secretKey.Length)"
}

Write-Host ""

# 4. Create/update the secret in Key Vault
Write-Host "📝 Creating secret 'Jwt-SecretKey' in Key Vault..." -ForegroundColor Cyan

try {
    $output = az keyvault secret set `
        --vault-name $KeyVaultName `
        --name "Jwt-SecretKey" `
        --value $secretKey `
        2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Secret created/updated successfully"
    }
    else {
        throw $output
    }
}
catch {
    Write-Host "  ✗ Error: $_"
    exit 1
}

Write-Host ""

# 5. Verify the secret
Write-Host "🔍 Verifying secret..." -ForegroundColor Cyan

try {
    $secret = az keyvault secret show `
        --vault-name $KeyVaultName `
        --name "Jwt-SecretKey" `
        --query "value" `
        --output tsv `
        2>&1
    
    if ($secret) {
        Write-Host "  ✓ Secret retrievable: ${secret.Substring(0,8)}...${secret.Substring(-8)}"
    }
    else {
        Write-Host "  ✗ Secret not found"
        exit 1
    }
}
catch {
    Write-Host "  ✗ Error: $_"
    exit 1
}

Write-Host ""

# 6. Configure Managed Identity (optional)
Write-Host "👤 Managed Identity Configuration (optional):" -ForegroundColor Yellow
Write-Host ""
Write-Host "  To grant access to your application, run:" -ForegroundColor Gray
Write-Host "  az keyvault set-policy --name $KeyVaultName \" -ForegroundColor Gray
Write-Host "    --object-id <APP_IDENTITY_PRINCIPAL_ID> \" -ForegroundColor Gray
Write-Host "    --secret-permissions get list" -ForegroundColor Gray
Write-Host ""

Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Configuration completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Summary:" -ForegroundColor Cyan
Write-Host "  • Key Vault: $KeyVaultName"
Write-Host "  • Secret: Jwt-SecretKey"
Write-Host "  • Key: ${secretKey.Substring(0,8)}...${secretKey.Substring(-8)}"
Write-Host ""
Write-Host "🔧 Application Configuration:" -ForegroundColor Yellow
Write-Host "  1. Update appsettings.Production.json:"
Write-Host "     ""AzureKeyVault"": { ""VaultUri"": ""https://$KeyVaultName.vault.azure.net/"" }"
Write-Host ""
Write-Host "  2. Configure Managed Identity for your App Service"
Write-Host ""
Write-Host "  3. Add Key Vault access via set-policy (see above)"
Write-Host ""

Write-Host "=" * 70 -ForegroundColor Cyan


