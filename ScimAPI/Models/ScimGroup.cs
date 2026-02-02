using ScimAPI.Attributes;

namespace ScimAPI.Models
{
    [ScimResource(
        "urn:ietf:params:scim:schemas:core:2.0:Group",
        "Group",
        "Group")]
    public class ScimGroup
    {
        // ==================== REQUIRED ATTRIBUTES (RFC 7643) ====================
        
        /// <summary>
        /// Unique identifier for the SCIM resource (REQUIRED)
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Display name for the Group (REQUIRED)
        /// </summary>
        [ScimProperty("displayName", "string", Required = true, Description = "Display name for the Group")]
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// SCIM schema URIs (REQUIRED)
        /// </summary>
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:schemas:core:2.0:Group" };
        
        /// <summary>
        /// Resource metadata (REQUIRED)
        /// </summary>
        public ScimMeta Meta { get; set; } = new();

        // ==================== OPTIONAL ATTRIBUTES ====================
        
        /// <summary>
        /// External unique identifier defined by provisioning client (OPTIONAL)
        /// </summary>
        [ScimProperty("externalId", "string", Description = "External unique identifier defined by provisioning client")]
        public string? ExternalId { get; set; }
        
        /// <summary>
        /// List of members in this group (OPTIONAL, multi-valued)
        /// </summary>
        [ScimProperty("members", "complex", MultiValued = true, Description = "List of members in this group")]
        public List<ScimMember> Members { get; set; } = new();
        
        /// <summary>
        /// Custom/extension attributes (OPTIONAL)
        /// </summary>
        public Dictionary<string, object> CustomAttributes { get; set; } = new();
    }

    public class ScimMember
    {
        /// <summary>
        /// Member identifier (REQUIRED in member object)
        /// </summary>
        [ScimProperty("value", "string", Required = true, Description = "Member identifier")]
        public string Value { get; set; } = string.Empty;
        
        /// <summary>
        /// URI reference to the member resource (OPTIONAL)
        /// </summary>
        [ScimProperty("$ref", "reference", Description = "URI reference to the member resource")]
        public string? Ref { get; set; }
        
        /// <summary>
        /// Display name of the member (OPTIONAL)
        /// </summary>
        [ScimProperty("display", "string", Description = "Display name of the member")]
        public string? Display { get; set; }
        
        /// <summary>
        /// Type of member (e.g., User, Group) (OPTIONAL, default: User)
        /// </summary>
        [ScimProperty("type", "string", Description = "Type of member")]
        public string Type { get; set; } = "User";
    }
}

