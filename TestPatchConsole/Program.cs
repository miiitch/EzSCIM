using EzSCIM.IntegrationTests;
using EzSCIM.IntegrationTests.Data.Entities;
using EzSCIM.Models;

Console.WriteLine("=== Testing ScimPatchApplier ===\n");

// Test 1: Filtered path
Console.WriteLine("Test 1: Filtered path emails[primary eq true].value");
var user1 = new UserEntity
{
    Id = Guid.NewGuid().ToString(),
    UserName = "test@example.com",
    Email = "original@example.com"
};

Console.WriteLine($"  Original Email: {user1.Email}");

try
{
    var operations1 = new[]
    {
        new ScimPatchOperation
        {
            Op = "replace",
            Path = "emails[primary eq true].value",
            Value = "updated@example.com"
        }
    };

    var result1 = ScimPatchApplier.ApplyPatch(user1, operations1);
    Console.WriteLine($"  ApplyPatch returned: {result1}");
    Console.WriteLine($"  Updated Email: {user1.Email}");
    
    if (user1.Email == "updated@example.com")
    {
        Console.WriteLine("  ✓ TEST PASSED");
    }
    else
    {
        Console.WriteLine("  ✗ TEST FAILED - Email not updated");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"  ✗ EXCEPTION: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"    Inner: {ex.InnerException.Message}");
    }
}

Console.WriteLine();

// Test 2: Direct array path
Console.WriteLine("Test 2: Direct array path emails[0].value");
var user2 = new UserEntity
{
    Id = Guid.NewGuid().ToString(),
    UserName = "test2@example.com",
    Email = "original2@example.com"
};

Console.WriteLine($"  Original Email: {user2.Email}");

try
{
    var operations2 = new[]
    {
        new ScimPatchOperation
        {
            Op = "replace",
            Path = "emails[0].value",
            Value = "updated2@example.com"
        }
    };

    var result2 = ScimPatchApplier.ApplyPatch(user2, operations2);
    Console.WriteLine($"  ApplyPatch returned: {result2}");
    Console.WriteLine($"  Updated Email: {user2.Email}");
    
    if (user2.Email == "updated2@example.com")
    {
        Console.WriteLine("  ✓ TEST PASSED");
    }
    else
    {
        Console.WriteLine("  ✗ TEST FAILED - Email not updated");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"  ✗ EXCEPTION: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"    Inner: {ex.InnerException.Message}");
    }
}

Console.WriteLine("\n=== Tests Complete ===");

