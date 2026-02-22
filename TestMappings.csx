using System.Reflection;
using EzSCIM.Attributes;
using EzSCIM.IntegrationTests.Data.Entities;

Console.WriteLine("=== UserEntity ScimProperty Attributes ===\n");

var userEntityType = typeof(UserEntity);
var properties = userEntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

Console.WriteLine($"Total properties: {properties.Length}\n");

var count = 0;
foreach (var property in properties)
{
    var scimAttr = property.GetCustomAttribute<ScimPropertyAttribute>();
    if (scimAttr != null)
    {
        count++;
        Console.WriteLine($"{count,3}. {property.Name,-30} → ScimProperty(\"{scimAttr.Name}\")");
    }
}

Console.WriteLine($"\nTotal ScimProperty attributes found: {count}");

// Test specific ones
Console.WriteLine("\n=== Testing Specific Paths ===");
var emailProp = properties.FirstOrDefault(p => p.Name == "Email");
if (emailProp != null)
{
    var attr = emailProp.GetCustomAttribute<ScimPropertyAttribute>();
    Console.WriteLine($"Email property: {attr?.Name ?? "NO ATTRIBUTE"}");
}

var phoneProp = properties.FirstOrDefault(p => p.Name == "PhoneNumber");
if (phoneProp != null)
{
    var attr = phoneProp.GetCustomAttribute<ScimPropertyAttribute>();
    Console.WriteLine($"PhoneNumber property: {attr?.Name ?? "NO ATTRIBUTE"}");
}

// Test normalization
Console.WriteLine("\n=== Testing Path Normalization ===");

string NormalizePath(string path)
{
    var normalized = path.ToLowerInvariant();
    var bracketIndex = normalized.IndexOf('[');
    if (bracketIndex > 0)
    {
        var closeBracket = normalized.IndexOf(']', bracketIndex);
        if (closeBracket > bracketIndex)
        {
            var prefix = normalized.Substring(0, bracketIndex);
            var suffix = closeBracket < normalized.Length - 1 ? normalized.Substring(closeBracket + 1) : "";
            normalized = prefix + "[0]" + suffix;
        }
    }
    return normalized;
}

var testPaths = new[]
{
    "emails[primary eq true].value",
    "phoneNumbers[primary eq true].value",
    "addresses[primary eq true].formatted",
    "emails[0].value",
    "EMAILS[0].VALUE"
};

foreach (var path in testPaths)
{
    var normalized = NormalizePath(path);
    Console.WriteLine($"{path,-40} → {normalized}");
}

