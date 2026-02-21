#!/usr/bin/env dotnet-script
#!meta {"framework": "net8.0"}

// Quick verification script for SCIM schema changes
using System;
using System.IO;
using System.Linq;

Console.WriteLine("=== SCIM Schema Fix Verification ===\n");

var changes = new[]
{
    ("Models", "EzSCIM/Models/ScimSchema.cs", new[] { "JsonPropertyName", "JsonIgnore", "public ScimMeta?" }),
    ("Models", "EzSCIM/Models/ScimSchemaAttribute.cs", new[] { "JsonPropertyName", "subAttributes" }),
    ("Models", "EzSCIM/Models/ScimMeta.cs", new[] { "JsonIgnore(Condition", "WhenWritingDefault" }),
    ("Controller", "EzSCIM/Controllers/ScimConfigController.cs", new[] { "ScimListResponse<ScimSchema>", "schema.Meta" }),
    ("Tests", "EzSCIM.UnitTests/SchemaJsonSerializationTests.cs", new[] { "SchemaJsonSerializationTests", "[Fact]" }),
};

var allGood = true;

foreach (var (category, filepath, keywords) in changes)
{
    var fullPath = Path.Combine(Environment.CurrentDirectory, filepath);
    
    if (!File.Exists(fullPath))
    {
        Console.WriteLine($"❌ {category}: {filepath} - FILE NOT FOUND");
        allGood = false;
        continue;
    }
    
    var content = File.ReadAllText(fullPath);
    var foundAll = keywords.All(k => content.Contains(k));
    
    if (foundAll)
    {
        Console.WriteLine($"✅ {category}: {filepath}");
        Console.WriteLine($"   - Found {keywords.Length} key indicators");
    }
    else
    {
        var missing = keywords.Where(k => !content.Contains(k)).ToList();
        Console.WriteLine($"⚠️  {category}: {filepath}");
        Console.WriteLine($"   - Missing keywords: {string.Join(", ", missing)}");
        allGood = false;
    }
}

Console.WriteLine("\n=== Documentation ===\n");

var docs = new[]
{
    "docs/schema/scim-validator-fix.md",
    "docs/schema/testing-scim-schema-validation.md",
    "Test-SchemaEndpoints.ps1",
    "IMPLEMENTATION-SUMMARY.md",
    "CODE-CHANGES-SUMMARY.md"
};

foreach (var doc in docs)
{
    var fullPath = Path.Combine(Environment.CurrentDirectory, doc);
    if (File.Exists(fullPath))
    {
        var lines = File.ReadAllLines(fullPath).Length;
        Console.WriteLine($"✅ {doc} ({lines} lines)");
    }
    else
    {
        Console.WriteLine($"❌ {doc} - NOT FOUND");
        allGood = false;
    }
}

Console.WriteLine("\n" + (allGood ? "✅ All changes verified successfully!" : "⚠️  Some issues found - review above"));

