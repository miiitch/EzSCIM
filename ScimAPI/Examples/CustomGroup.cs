using ScimAPI.Attributes;
using ScimAPI.Constants;

namespace ScimAPI.Examples
{
    /// <summary>
    /// Example custom group class with SCIM property mappings.
    /// This demonstrates how to annotate your existing group model for SCIM integration.
    /// </summary>
    public class CustomGroup
    {
        /// <summary>
        /// Unique identifier (maps to SCIM id)
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Group name (maps to SCIM displayName - REQUIRED)
        /// </summary>
        [ScimProperty(ScimAttributeNames.Group.DisplayName, "string", Required = true)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Group description
        /// </summary>
        [ScimProperty("description", "string")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// External system ID (maps to SCIM externalId)
        /// </summary>
        [ScimProperty(ScimAttributeNames.Common.ExternalId, "string")]
        public string ExternalSystemId { get; set; } = string.Empty;

        /// <summary>
        /// Member user IDs (simplified - in real scenario, use proper member objects)
        /// </summary>
        public List<string> MemberIds { get; set; } = new();

        /// <summary>
        /// Created timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last modified timestamp
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Group type (e.g., Security, Distribution, etc.)
        /// </summary>
        public string GroupType { get; set; } = "Security";
    }
}

