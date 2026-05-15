using System.Text.Json.Serialization;

namespace EzSCIM.Models;

/// <summary>
/// Represents a SCIM schema attribute according to RFC 7643
/// </summary>
public class ScimSchemaAttribute
{
    /// <summary>
    /// The name of the attribute
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The type of the attribute (string, boolean, decimal, integer, dateTime, reference, complex)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether the attribute can have multiple values
    /// </summary>
    public bool MultiValued { get; set; } = false;

    /// <summary>
    /// Description of the attribute
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the attribute is required
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    /// Whether the attribute is case-sensitive
    /// </summary>
    public bool CaseExact { get; set; } = false;

    /// <summary>
    /// Indicates if the attribute is mutable (readWrite, readOnly, immutable, writeOnly)
    /// </summary>
    public string Mutability { get; set; } = "readWrite";

    /// <summary>
    /// Indicates when the attribute should be returned (always, never, default, request)
    /// </summary>
    public string Returned { get; set; } = "default";

    /// <summary>
    /// Uniqueness constraint (none, server, global)
    /// </summary>
    public string Uniqueness { get; set; } = "none";

    /// <summary>
    /// Sub-attributes for complex types
    /// </summary>
    [JsonPropertyName("subAttributes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ScimSchemaAttribute>? SubAttributes { get; set; }
}