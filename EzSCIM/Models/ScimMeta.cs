using System.Text.Json.Serialization;

namespace EzSCIM.Models
{
    /// <summary>
    /// Represents metadata about a SCIM resource according to RFC 7643 Section 3.1
    /// </summary>
    public class ScimMeta
    {
        /// <summary>
        /// The name of the resource type of the resource
        /// </summary>
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// The DateTime that the resource was added to the service provider
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The most recent DateTime that the details of this resource were updated at the service provider
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The URI of the resource being returned
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Location { get; set; }

        /// <summary>
        /// The version of the resource
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Version { get; set; }
    }
}

