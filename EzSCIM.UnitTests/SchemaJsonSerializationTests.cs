using System.Text.Json;
using System.Text.Json.Serialization;
using EzSCIM.Models;
using EzSCIM.Helpers;
using Xunit;

namespace EzSCIM.UnitTests;

/// <summary>
/// Tests for schema JSON serialization and SCIM compliance
/// </summary>
public class SchemaJsonSerializationTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    [Fact]
    public void UserSchema_SerializesTo_ValidJsonObject()
    {
        // Arrange
        var schema = ScimSchemaGenerator.UserSchema;
        schema.Meta = new ScimMeta
        {
            ResourceType = "Schema",
            Location = "https://example.com/scim/Schemas/urn:ietf:params:scim:schemas:core:2.0:User"
        };

        // Act
        var json = JsonSerializer.Serialize(schema, _jsonOptions);
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.NotNull(doc.RootElement);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("id", out _), "Schema should have 'id' property");
        Assert.True(doc.RootElement.TryGetProperty("name", out _), "Schema should have 'name' property");
        Assert.True(doc.RootElement.TryGetProperty("attributes", out _), "Schema should have 'attributes' property");
        Assert.True(doc.RootElement.TryGetProperty("meta", out _), "Schema should have 'meta' property");

        Console.WriteLine("=== User Schema JSON ===");
        Console.WriteLine(json);
    }

    [Fact]
    public void SchemaListResponse_SerializesTo_ValidListResponseObject()
    {
        // Arrange
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

        // Act
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.NotNull(doc.RootElement);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("schemas", out _), "Response should have 'schemas' property");
        Assert.True(doc.RootElement.TryGetProperty("totalResults", out var totalResults), "Response should have 'totalResults' property");
        Assert.True(doc.RootElement.TryGetProperty("resources", out var resources), "Response should have 'resources' property");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);
        Assert.Equal(2, totalResults.GetInt32());

        Console.WriteLine("\n=== Schemas List Response JSON ===");
        Console.WriteLine(json);
    }

    [Fact]
    public void SchemaAttributes_SerializeSubAttributes_AsValidJson()
    {
        // Arrange
        var schema = ScimSchemaGenerator.UserSchema;
        var complexAttr = schema.Attributes.FirstOrDefault(a => a.Type.Equals("complex", StringComparison.OrdinalIgnoreCase));

        // Act
        var json = JsonSerializer.Serialize(schema, _jsonOptions);
        var doc = JsonDocument.Parse(json);

        // Assert - Verify the JSON is valid
        Assert.NotNull(doc.RootElement);
        Assert.True(doc.RootElement.TryGetProperty("attributes", out var attributes), "Should have attributes");
        Assert.Equal(JsonValueKind.Array, attributes.ValueKind);

        Console.WriteLine("\n=== Full User Schema with Attributes ===");
        Console.WriteLine(json);
    }
}

