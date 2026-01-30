# PowerShell script to generate a JWT Bearer Token
# Calls the test endpoint /scim/auth/token to generate a token

[CmdletBinding()]
param(
    [string]$ApiBaseUrl = "https://localhost:7001",
    [switch]$CopyToClipboard,
    [switch]$ShowDecoded
)

Write-Host ""
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host "🔐 JWT BEARER TOKEN GENERATOR - SCIM API" -ForegroundColor Cyan
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host ""

# Configure SSL for localhost in dev
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

Write-Host "📋 Configuration:" -ForegroundColor Yellow
Write-Host "  API URL: $ApiBaseUrl"
Write-Host "  SSL Verification: Disabled (dev mode)" -ForegroundColor Yellow
Write-Host ""

try {
    Write-Host "🔄 Generating token via GET /scim/auth/token..." -ForegroundColor Cyan
    
    $response = Invoke-RestMethod `
        -Uri "$ApiBaseUrl/scim/auth/token" `
        -Method Get `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    $token = $response.token
    $expiresIn = $response.expiresIn
    
    if (-not $token) {
        throw "No token returned by API"
    }
    
    Write-Host "✅ Token generated successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Token Information:" -ForegroundColor Yellow
    Write-Host "  Expiration: $expiresIn"
    Write-Host "  Size: $($token.Length) characters"
    Write-Host ""
    
    # Display the token
    Write-Host "🔑 TOKEN:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host $token -ForegroundColor White
    Write-Host ""
    
    # Option: Decode the JWT (display claims)
    if ($ShowDecoded) {
        Write-Host "📖 DECODED PAYLOAD:" -ForegroundColor Yellow
        
        # Extract and decode JWT payload
        $parts = $token.Split('.')
        if ($parts.Count -eq 3) {
            # Add padding if needed
            $payload = $parts[1]
            while ($payload.Length % 4) { $payload += "=" }
            
            try {
                $decodedBytes = [System.Convert]::FromBase64String($payload)
                $decodedJson = [System.Text.Encoding]::UTF8.GetString($decodedBytes)
                $payloadObj = $decodedJson | ConvertFrom-Json
                
                Write-Host ""
                Write-Host ($payloadObj | ConvertTo-Json | Out-String) -ForegroundColor Gray
            }
            catch {
                Write-Host "  (Error during decoding)" -ForegroundColor Yellow
            }
        }
    }
    
    # Option: Copy to clipboard
    if ($CopyToClipboard) {
        $fullToken = "Bearer $token"
        $fullToken | Set-Clipboard
        Write-Host "📋 Token copied to clipboard (format: Bearer <token>)" -ForegroundColor Green
        Write-Host ""
    }
    
    Write-Host "💡 Usage:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  PowerShell:" -ForegroundColor White
    Write-Host '    $headers = @{ "Authorization" = "Bearer ' + $token + '" }' -ForegroundColor Gray
    Write-Host '    Invoke-RestMethod -Uri "https://localhost:7001/scim/Users" -Headers $headers'
    Write-Host ""
    Write-Host "  cURL:" -ForegroundColor White
    Write-Host '    curl -H "Authorization: Bearer ' + $token + '" https://localhost:7001/scim/Users'
    Write-Host ""
    
    Write-Host "=" * 70 -ForegroundColor Cyan
    Write-Host ""
    
    exit 0
}
catch {
    Write-Host "❌ ERROR: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Checks:" -ForegroundColor Yellow
    Write-Host "  ✓ Is the SCIM application running?"
    Write-Host "  ✓ Is the URL correct? ($ApiBaseUrl)"
    Write-Host "  ✓ Is the environment in development? (endpoint /scim/auth/token must be public)"
    Write-Host ""
    exit 1
}
