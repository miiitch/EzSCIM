using ScimAPI.Helpers;
using ScimAPI.Models;
using System;

namespace ScimAPI.Tests
{
    /// <summary>
    /// Quick test to verify schema generation
    /// </summary>
    class SchemaGenerationTest
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Testing SCIM Schema Generation ===\n");
            
            // Test User Schema
            Console.WriteLine("User Schema:");
            Console.WriteLine($"  ID: {ScimSchemaGenerator.UserSchema.Id}");
            Console.WriteLine($"  Name: {ScimSchemaGenerator.UserSchema.Name}");
            Console.WriteLine($"  Description: {ScimSchemaGenerator.UserSchema.Description}");
            Console.WriteLine($"  Attributes Count: {ScimSchemaGenerator.UserSchema.Attributes.Count}");
            
            Console.WriteLine("\n  Top Attributes:");
            foreach (var attr in ScimSchemaGenerator.UserSchema.Attributes.Take(5))
            {
                Console.WriteLine($"    - {attr.Name} ({attr.Type}){(attr.Required ? " [REQUIRED]" : "")}");
                if (attr.SubAttributes != null && attr.SubAttributes.Count > 0)
                {
                    Console.WriteLine($"      SubAttributes: {attr.SubAttributes.Count}");
                    foreach (var sub in attr.SubAttributes.Take(3))
                    {
                        Console.WriteLine($"        • {sub.Name} ({sub.Type})");
                    }
                }
            }
            
            // Test Group Schema
            Console.WriteLine("\n\nGroup Schema:");
            Console.WriteLine($"  ID: {ScimSchemaGenerator.GroupSchema.Id}");
            Console.WriteLine($"  Name: {ScimSchemaGenerator.GroupSchema.Name}");
            Console.WriteLine($"  Description: {ScimSchemaGenerator.GroupSchema.Description}");
            Console.WriteLine($"  Attributes Count: {ScimSchemaGenerator.GroupSchema.Attributes.Count}");
            
            Console.WriteLine("\n  Attributes:");
            foreach (var attr in ScimSchemaGenerator.GroupSchema.Attributes)
            {
                Console.WriteLine($"    - {attr.Name} ({attr.Type}){(attr.Required ? " [REQUIRED]" : "")}");
                if (attr.SubAttributes != null && attr.SubAttributes.Count > 0)
                {
                    Console.WriteLine($"      SubAttributes: {attr.SubAttributes.Count}");
                }
            }
            
            Console.WriteLine("\n=== Schema Generation Test Complete ===");
        }
    }
}
