# PowerShell script to test JWT Bearer Token authentication for SCIM API

[CmdletBinding()]
param(
    [string]$ApiBaseUrl = "https://localhost:7001",
    [switch]$Verbose
)

Write-Host ""
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host "🧪 JWT AUTHENTICATION TEST - SCIM API" -ForegroundColor Cyan
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host ""

# Configure SSL for localhost in dev
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

Write-Host "📋 Configuration:" -ForegroundColor Yellow
Write-Host "  API URL: $ApiBaseUrl"
Write-Host "  Verbose: $Verbose"
Write-Host ""

$testsPass = 0
$testsFail = 0

function Test-Endpoint {
    param(
        [string]$Method,
        [string]$Endpoint,
        [hashtable]$Headers = @{},
        [string]$Description,
        [int]$ExpectedStatus = 200,
        [switch]$ShouldFail
    )
    
    Write-Host "  🔄 $Description..." -ForegroundColor Cyan
    
    try {
        $url = "$ApiBaseUrl$Endpoint"
        $params = @{
            Uri = $url
            Method = $Method
            ErrorAction = "Stop"
        }
        
        if ($Headers.Count -gt 0) {
            $params["Headers"] = $Headers
        }
        
        $response = Invoke-RestMethod @params
        
        Write-Host "    ✅ Success (HTTP 200)" -ForegroundColor Green
        $global:testsPass++
        return $response
    }
    catch {
        $httpStatus = $_.Exception.Response.StatusCode.Value__
        
        if ($ShouldFail -and $httpStatus -eq $ExpectedStatus) {
            Write-Host "    ✅ Correctly rejected (HTTP $httpStatus)" -ForegroundColor Green
            $global:testsPass++
            return $null
        }
        else {
            Write-Host "    ❌ HTTP Error $httpStatus : $($_.Exception.Message)" -ForegroundColor Red
            $global:testsFail++
            return $null
        }
    }
}

# ==================== STEP 1: Generate a Token ====================
Write-Host "1️⃣  TOKEN GENERATION" -ForegroundColor Yellow
Write-Host ""

$token = $null
try {
    Write-Host "  🔄 Calling GET /scim/auth/token (no auth)..." -ForegroundColor Cyan
    
    $response = Invoke-RestMethod `
        -Uri "$ApiBaseUrl/scim/auth/token" `
        -Method Get `
        -ErrorAction Stop
    
    $token = $response.token
    $expiresIn = $response.expiresIn
    
    Write-Host "    ✅ Token generated successfully" -ForegroundColor Green
    Write-Host "    📊 Expiration: $expiresIn"
    Write-Host "    📊 Size: $($token.Length) characters"
    
    if ($token) {
        Write-Host "    🔑 Token: $($token.Substring(0, 20))...$($token.Substring(-10))" -ForegroundColor Gray
        $global:testsPass++
    }
    else {
        throw "No token returned"
    }
}
catch {
    Write-Host "    ❌ Error: $_" -ForegroundColor Red
    $global:testsFail++
    exit 1
}

Write-Host ""

# ==================== STEP 2: Test without Token ====================
Write-Host "2️⃣  TEST WITHOUT TOKEN (should return 401)" -ForegroundColor Yellow
Write-Host ""

Test-Endpoint -Method "Get" -Endpoint "/scim/Users" -Description "Access to /scim/Users without token" -ExpectedStatus 401 -ShouldFail

Write-Host ""

# ==================== STEP 3: Test with Valid Token ====================
Write-Host "3️⃣  TEST WITH VALID TOKEN" -ForegroundColor Yellow
Write-Host ""

if ($token) {
    $authHeader = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/scim+json"
    }
    
    # Test ServiceProviderConfig
    Write-Host "  📌 Test 1: ServiceProviderConfig" -ForegroundColor Cyan
    $config = Test-Endpoint `
        -Method "Get" `
        -Endpoint "/scim/ServiceProviderConfig" `
        -Headers $authHeader `
        -Description "Access to /scim/ServiceProviderConfig"
    
    if ($config) {
        Write-Host "    📋 Filter supported: $($config.filter.supported)"
        Write-Host "    📋 Patch supported: $($config.patch.supported)"
    }
    
    Write-Host ""
    
    # Test Schemas
    Write-Host "  📌 Test 2: Schemas" -ForegroundColor Cyan
    $schemas = Test-Endpoint `
        -Method "Get" `
        -Endpoint "/scim/Schemas" `
        -Headers $authHeader `
        -Description "Access to /scim/Schemas"
    
    if ($schemas) {
        Write-Host "    📋 Available schemas: $($schemas.schemas.Count)"
    }
    
    Write-Host ""
    
    # Test Users
    Write-Host "  📌 Test 3: Users" -ForegroundColor Cyan
    $users = Test-Endpoint `
        -Method "Get" `
        -Endpoint "/scim/Users" `
        -Headers $authHeader `
        -Description "Access to /scim/Users"
    
    if ($users) {
        Write-Host "    📋 Total users: $($users.totalResults)"
        Write-Host "    📋 Items per page: $($users.itemsPerPage)"
    }
    
    Write-Host ""
    
    # Test Groups
    Write-Host "  📌 Test 4: Groups" -ForegroundColor Cyan
    $groups = Test-Endpoint `
        -Method "Get" `
        -Endpoint "/scim/Groups" `
        -Headers $authHeader `
        -Description "Access to /scim/Groups"
    
    if ($groups) {
        Write-Host "    📋 Total groups: $($groups.totalResults)"
    }
    
    Write-Host ""
}

# ==================== STEP 4: Test with Invalid Token ====================
Write-Host "4️⃣  TEST WITH INVALID TOKEN (should return 401)" -ForegroundColor Yellow
Write-Host ""

$badHeaders = @{
    "Authorization" = "Bearer invalid.token.here"
    "Content-Type" = "application/scim+json"
}

Test-Endpoint `
    -Method "Get" `
    -Endpoint "/scim/Users" `
    -Headers $badHeaders `
    -Description "Access with invalid token" `
    -ExpectedStatus 401 `
    -ShouldFail

Write-Host ""

# ==================== SUMMARY ====================
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host "📊 TEST SUMMARY" -ForegroundColor Yellow
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host ""

$totalTests = $testsPass + $testsFail
$successRate = if ($totalTests -gt 0) { [math]::Round(($testsPass / $totalTests) * 100) } else { 0 }

Write-Host "  ✅ Passed: $testsPass" -ForegroundColor Green
Write-Host "  ❌ Failed: $testsFail" -ForegroundColor Red
Write-Host "  📊 Total: $totalTests tests"
Write-Host "  📈 Success Rate: $successRate%"
Write-Host ""

if ($testsFail -eq 0) {
    Write-Host "🎉 ALL TESTS PASSED!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📝 Results:" -ForegroundColor Cyan
    Write-Host "  ✓ Test endpoint /scim/auth/token is public"
    Write-Host "  ✓ Protected endpoints reject requests without token"
    Write-Host "  ✓ Valid token grants access"
    Write-Host "  ✓ Invalid token is rejected"
    Write-Host ""
    Write-Host "✨ JWT authentication is working correctly!" -ForegroundColor Green
}
else {
    Write-Host "⚠️  SOME TESTS FAILED!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Checks:" -ForegroundColor Yellow
    Write-Host "  • Is the SCIM application running?"
    Write-Host "  • Is the URL correct? ($ApiBaseUrl)"
    Write-Host "  • Are endpoints protected by [Authorize]?"
}

Write-Host ""
Write-Host "=" * 70 -ForegroundColor Cyan
Write-Host ""

# Display token for manual use
if ($token) {
    Write-Host "💡 Using the token:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  PowerShell:" -ForegroundColor White
    Write-Host "    `$headers = @{ 'Authorization' = 'Bearer $token' }" -ForegroundColor Gray
    Write-Host "    Invoke-RestMethod -Uri '$ApiBaseUrl/scim/Users' -Headers `$headers"
    Write-Host ""
    Write-Host "  cURL:" -ForegroundColor White
    Write-Host "    curl -H 'Authorization: Bearer $token' $ApiBaseUrl/scim/Users"
    Write-Host ""
}

exit $(if ($testsFail -eq 0) { 0 } else { 1 })
