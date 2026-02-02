using ScimAPI.Attributes;

namespace ScimAPI.Models
{
    /// <summary>
    /// Complete SCIM User with optional attributes
    /// </summary>
    public class ScimUser : ScimUserBase
    {
        // ==================== OPTIONAL ATTRIBUTES ====================
        
        /// <summary>
        /// External unique identifier defined by provisioning client (OPTIONAL)
        /// </summary>
        [ScimProperty("externalId", "string", Description = "External unique identifier defined by provisioning client")]
        public string? ExternalId { get; set; }
        
        /// <summary>
        /// User's full name (OPTIONAL)
        /// </summary>
        [ScimProperty("name", "complex", Description = "User's full name")]
        public ScimName Name { get; set; } = new();
        
        /// <summary>
        /// Name to display (OPTIONAL)
        /// </summary>
        [ScimProperty("displayName", "string", Description = "Name to display")]
        public string? DisplayName { get; set; }
        
        /// <summary>
        /// Casual name (OPTIONAL)
        /// </summary>
        [ScimProperty("nickName", "string", Description = "Casual name")]
        public string? NickName { get; set; }
        
        /// <summary>
        /// URL to user's profile (OPTIONAL)
        /// </summary>
        [ScimProperty("profileUrl", "string", Description = "URL to user's profile")]
        public string? ProfileUrl { get; set; }
        
        /// <summary>
        /// User's title (OPTIONAL)
        /// </summary>
        [ScimProperty("title", "string", Description = "User's title")]
        public string? Title { get; set; }
        
        /// <summary>
        /// Type of user (e.g., Employee, Contractor) (OPTIONAL)
        /// </summary>
        [ScimProperty("userType", "string", Description = "Type of user")]
        public string? UserType { get; set; }
        
        /// <summary>
        /// Preferred language (OPTIONAL)
        /// </summary>
        [ScimProperty("preferredLanguage", "string", Description = "Preferred language")]
        public string? PreferredLanguage { get; set; }
        
        /// <summary>
        /// User's locale (OPTIONAL)
        /// </summary>
        [ScimProperty("locale", "string", Description = "User's locale")]
        public string? Locale { get; set; }
        
        /// <summary>
        /// User's timezone (OPTIONAL)
        /// </summary>
        [ScimProperty("timezone", "string", Description = "User's timezone")]
        public string? Timezone { get; set; }
        
        /// <summary>
        /// Whether the user is active (OPTIONAL, default: true)
        /// </summary>
        [ScimProperty("active", "boolean", Description = "Whether the user is active")]
        public bool Active { get; set; } = true;
        
        /// <summary>
        /// User's email addresses (OPTIONAL, multi-valued)
        /// </summary>
        [ScimProperty("emails", "complex", MultiValued = true, Description = "User's email addresses")]
        public List<ScimEmail> Emails { get; set; } = new();
        
        /// <summary>
        /// User's phone numbers (OPTIONAL, multi-valued)
        /// </summary>
        [ScimProperty("phoneNumbers", "complex", MultiValued = true, Description = "User's phone numbers")]
        public List<ScimPhoneNumber> PhoneNumbers { get; set; } = new();
        
        /// <summary>
        /// User's physical addresses (OPTIONAL, multi-valued)
        /// </summary>
        [ScimProperty("addresses", "complex", MultiValued = true, Description = "User's physical addresses")]
        public List<ScimAddress> Addresses { get; set; } = new();
        
        /// <summary>
        /// Groups to which the user belongs (OPTIONAL, multi-valued, read-only)
        /// </summary>
        [ScimProperty("groups", "complex", MultiValued = true, Mutability = "readOnly", Description = "Groups to which the user belongs")]
        public List<ScimGroupMembership> Groups { get; set; } = new();
        
        /// <summary>
        /// Custom/extension attributes (OPTIONAL)
        /// </summary>
        public Dictionary<string, object> CustomAttributes { get; set; } = new();
    }
}

