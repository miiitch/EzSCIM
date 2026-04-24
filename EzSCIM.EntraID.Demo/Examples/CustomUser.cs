using EzSCIM.Attributes;
using EzSCIM.Constants;

namespace EzSCIM.EntraID.Demo.Examples
{
    /// <summary>
    /// Example custom user class with SCIM property mappings.
    /// This demonstrates how to annotate your existing user model for SCIM integration.
    /// Uses ScimAttributeNames constants for type-safe attribute names.
    /// </summary>
    public class CustomUser
    {
        /// <summary>
        /// Unique identifier (maps to SCIM id)
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Username (maps to SCIM userName - REQUIRED)
        /// </summary>
        [ScimProperty(ScimAttributeNames.User.UserName, "string", Required = true, Uniqueness = "server")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email address (maps to SCIM emails[0].value)
        /// </summary>
        [ScimProperty("email", "string")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// First name (maps to SCIM name.givenName)
        /// </summary>
        [ScimProperty(ScimAttributeNames.User.NameGivenName, "string")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name (maps to SCIM name.familyName)
        /// </summary>
        [ScimProperty(ScimAttributeNames.User.NameFamilyName, "string")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Display name (maps to SCIM displayName)
        /// </summary>
        [ScimProperty(ScimAttributeNames.User.DisplayName, "string")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Is user active (maps to SCIM active)
        /// </summary>
        [ScimProperty(ScimAttributeNames.User.Active, "boolean")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Job title (maps to SCIM title)
        /// </summary>
        [ScimProperty(ScimAttributeNames.User.Title, "string")]
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>
        /// External system ID (maps to SCIM externalId)
        /// </summary>
        [ScimProperty(ScimAttributeNames.Common.ExternalId, "string")]
        public string ExternalSystemId { get; set; } = string.Empty;

        /// <summary>
        /// Created timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last modified timestamp
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    }
}


