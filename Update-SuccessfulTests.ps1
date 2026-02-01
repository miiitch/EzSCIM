# Script to update all successful test patterns in FilterParserTests.cs
# Transforms: var actual = _parser.Parse(...)
# Into:        var result = _parser.Parse(...); result.IsError.ShouldBeFalse(); var actual = result.Value;

$filePath = "ScimAPI.Tests\Filtering\FilterParserTests.cs"
$content = Get-Content $filePath -Raw

# Pattern to match: var actual = _parser.Parse(...)
# Replace with: var result = _parser.Parse(...); result.IsError.ShouldBeFalse(); var actual = result.Value;

$pattern = '(\s+)var actual = (_parser\.Parse\([^;]+\));'
$replacement = '$1var result = $2;$1result.IsError.ShouldBeFalse();$1var actual = result.Value;'

$newContent = $content -replace $pattern, $replacement

Set-Content $filePath $newContent -NoNewline

Write-Host "✅ Updated all successful test patterns!" -ForegroundColor Green
Write-Host "Modified file: $filePath" -ForegroundColor Cyan
