using System.Text.Json.Serialization;

namespace EzSCIM.Models
{
    /// <summary>
    /// Represents a SCIM 2.0 Schema according to RFC 7643 Section 7
    /// </summary>
    public class ScimSchema
    {
        /// <summary>
        /// The unique URI of the schema (e.g., urn:ietf:params:scim:schemas:core:2.0:User)
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The name of the schema
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the schema
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The SCIM schema URN for this object (always points to the Schema schema)
        /// </summary>
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:schemas:core:2.0:Schema" };

        /// <summary>
        /// The attributes defined by this schema
        /// </summary>
        public List<ScimSchemaAttribute> Attributes { get; set; } = new();

        /// <summary>
        /// Metadata about this schema resource
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ScimMeta? Meta { get; set; }
    }
}

