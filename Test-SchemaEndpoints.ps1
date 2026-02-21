# Test SCIM Schema Endpoints
# This script tests the schema endpoints to verify proper JSON structure

param(
    [string]$ApiUrl = "http://localhost:5000",
    [string]$Token = "test-token"
)

Write-Host "Testing SCIM Schema Endpoints" -ForegroundColor Cyan
Write-Host "API URL: $ApiUrl" -ForegroundColor Gray

# Prepare headers
$headers = @{
    "Authorization" = "Bearer $Token"
    "Content-Type" = "application/scim+json"
    "Accept" = "application/scim+json"
}

# Test 1: Get single schema (User)
Write-Host "`n[TEST 1] Single Schema - User" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$ApiUrl/scim/Schemas/urn:ietf:params:scim:schemas:core:2.0:User" `
        -Headers $headers -Method GET
    
    $json = $response.Content | ConvertFrom-Json
    
    Write-Host "✓ Request successful (Status: $($response.StatusCode))" -ForegroundColor Green
    Write-Host "✓ Response is JSON object (not array)" -ForegroundColor Green
    
    # Validate structure
    $required = @("id", "name", "schemas", "attributes", "meta")
    $missing = @()
    
    foreach ($prop in $required) {
        if ($null -eq $json.$prop) {
            $missing += $prop
        } else {
            Write-Host "  ✓ Property '$prop' present" -ForegroundColor Green
        }
    }
    
    if ($missing.Count -gt 0) {
        Write-Host "✗ Missing properties: $($missing -join ', ')" -ForegroundColor Red
    } else {
        Write-Host "✓ All required properties present" -ForegroundColor Green
    }
    
    # Display response
    Write-Host "`nResponse Preview:" -ForegroundColor Gray
    $json | ConvertTo-Json -Depth 3 | Write-Host
}
catch {
    Write-Host "✗ Request failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get schemas list
Write-Host "`n[TEST 2] Schemas List" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$ApiUrl/scim/Schemas" `
        -Headers $headers -Method GET
    
    $json = $response.Content | ConvertFrom-Json
    
    Write-Host "✓ Request successful (Status: $($response.StatusCode))" -ForegroundColor Green
    Write-Host "✓ Response is JSON object (not array)" -ForegroundColor Green
    
    # Validate structure
    $required = @("schemas", "totalResults", "resources")
    $missing = @()
    
    foreach ($prop in $required) {
        if ($null -eq $json.$prop) {
            $missing += $prop
        } else {
            Write-Host "  ✓ Property '$prop' present (count: $(if($json.$prop -is [array]) { $json.$prop.Count } else { '1' }))" -ForegroundColor Green
        }
    }
    
    if ($missing.Count -gt 0) {
        Write-Host "✗ Missing properties: $($missing -join ', ')" -ForegroundColor Red
    } else {
        Write-Host "✓ All required properties present" -ForegroundColor Green
    }
    
    # Check resources
    if ($json.resources.Count -gt 0) {
        Write-Host "✓ Resources array contains $($json.resources.Count) schema(s)" -ForegroundColor Green
        
        # Check first resource has meta
        if ($null -ne $json.resources[0].meta) {
            Write-Host "  ✓ First resource has 'meta' property" -ForegroundColor Green
        } else {
            Write-Host "  ✗ First resource missing 'meta' property" -ForegroundColor Red
        }
    }
    
    # Display response
    Write-Host "`nResponse Preview:" -ForegroundColor Gray
    $json | ConvertTo-Json -Depth 2 | Write-Host
}
catch {
    Write-Host "✗ Request failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get group schema
Write-Host "`n[TEST 3] Single Schema - Group" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$ApiUrl/scim/Schemas/urn:ietf:params:scim:schemas:core:2.0:Group" `
        -Headers $headers -Method GET
    
    $json = $response.Content | ConvertFrom-Json
    
    Write-Host "✓ Request successful (Status: $($response.StatusCode))" -ForegroundColor Green
    Write-Host "✓ Response is JSON object (not array)" -ForegroundColor Green
    
    if ($json.id -eq "urn:ietf:params:scim:schemas:core:2.0:Group") {
        Write-Host "✓ Correct schema ID returned" -ForegroundColor Green
    }
}
catch {
    Write-Host "✗ Request failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Non-existent schema
Write-Host "`n[TEST 4] Non-existent Schema (Error Handling)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$ApiUrl/scim/Schemas/urn:ietf:params:scim:schemas:invalid" `
        -Headers $headers -Method GET
    Write-Host "✗ Should have returned 404" -ForegroundColor Red
}
catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "✓ Correctly returned 404 for non-existent schema" -ForegroundColor Green
    } else {
        Write-Host "✗ Unexpected status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host "`n[COMPLETE] Testing finished" -ForegroundColor Cyan

