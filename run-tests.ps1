$ErrorActionPreference = "Continue"
cd C:\Users\MichelPerfetti\src\private\scimwork

Write-Host "`n=== Building ===" -ForegroundColor Cyan
dotnet build --nologo -v q

Write-Host "`n=== Running Tests ===" -ForegroundColor Cyan
$result = dotnet test --nologo --no-build 2>&1 | Out-String

# Extract summary
$lines = $result -split "`n"
$summary = $lines | Where-Object { $_ -match "total:|Failed:|Passed:" }

Write-Host "`n=== Test Summary ===" -ForegroundColor Yellow
$summary | ForEach-Object { Write-Host $_ }

# Show failures
$failures = $lines | Where-Object { $_ -match "FAIL" }
if ($failures) {
    Write-Host "`n=== Failed Tests ===" -ForegroundColor Red
    $failures | Select-Object -First 20 | ForEach-Object { Write-Host $_ }
}

# Save full output
$result | Out-File -FilePath "test-full-output.txt"
Write-Host "`nFull output saved to test-full-output.txt" -ForegroundColor Green

