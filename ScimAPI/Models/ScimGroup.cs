using ScimAPI.Attributes;

namespace ScimAPI.Models
{
    /// <summary>
    /// Complete SCIM Group with optional attributes
    /// </summary>
    public class ScimGroup : ScimGroupBase
    {
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
}

