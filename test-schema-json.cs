using System.Text.Json;
using System.Text.Json.Serialization;
using EzSCIM.Models;
using EzSCIM.Helpers;

// Quick test to verify schema JSON structure
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true
};

// Test single schema
var schema = ScimSchemaGenerator.UserSchema;
schema.Meta = new ScimMeta
{
    ResourceType = "Schema",
    Location = "https://example.com/scim/Schemas/urn:ietf:params:scim:schemas:core:2.0:User"
};

Console.WriteLine("=== Single Schema JSON ===");
Console.WriteLine(JsonSerializer.Serialize(schema, options));

// Test schema list response
var schemas = new List<ScimSchema>
{
    ScimSchemaGenerator.UserSchema,
    ScimSchemaGenerator.GroupSchema
};

foreach (var s in schemas)
{
    s.Meta = new ScimMeta
    {
        ResourceType = "Schema",
        Location = $"https://example.com/scim/Schemas/{Uri.EscapeDataString(s.Id)}"
    };
}

var response = new ScimListResponse<ScimSchema>
{
    TotalResults = schemas.Count,
    StartIndex = 1,
    ItemsPerPage = schemas.Count,
    Resources = schemas
};

Console.WriteLine("\n=== List Response JSON ===");
Console.WriteLine(JsonSerializer.Serialize(response, options));

