using ScimAPI.Attributes;

namespace ScimAPI.Models
{
    /// <summary>
    /// Base class with REQUIRED attributes for SCIM User (RFC 7643)
    /// </summary>
    [ScimResource(
        "urn:ietf:params:scim:schemas:core:2.0:User",
        "User",
        "User Account")]
    public class ScimUserBase
    {
        // ==================== REQUIRED ATTRIBUTES (RFC 7643) ====================
        
        /// <summary>
        /// Unique identifier for the SCIM resource (REQUIRED)
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Unique identifier for the User (REQUIRED)
        /// </summary>
        [ScimProperty("userName", "string", Required = true, Uniqueness = "server", Description = "Unique identifier for the User")]
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// SCIM schema URIs (REQUIRED)
        /// </summary>
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:schemas:core:2.0:User" };
        
        /// <summary>
        /// Resource metadata (REQUIRED)
        /// </summary>
        public ScimMeta Meta { get; set; } = new();
    }

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

    public class ScimName
    {
        [ScimProperty("formatted", "string", Description = "The full name, including all middle names, titles, and suffixes")]
        public string? Formatted { get; set; }
        
        [ScimProperty("familyName", "string", Description = "The family name of the User, or last name")]
        public string? FamilyName { get; set; }
        
        [ScimProperty("givenName", "string", Description = "The given name of the User, or first name")]
        public string? GivenName { get; set; }
        
        [ScimProperty("middleName", "string", Description = "The middle name(s) of the User")]
        public string? MiddleName { get; set; }
        
        [ScimProperty("honorificPrefix", "string", Description = "The honorific prefix(es) of the User")]
        public string? HonorificPrefix { get; set; }
        
        [ScimProperty("honorificSuffix", "string", Description = "The honorific suffix(es) of the User")]
        public string? HonorificSuffix { get; set; }
    }

    public class ScimEmail
    {
        [ScimProperty("value", "string", Description = "Email address")]
        public string Value { get; set; } = string.Empty;
        
        [ScimProperty("type", "string", Description = "Type of email address (e.g., work, home)")]
        public string? Type { get; set; }
        
        [ScimProperty("primary", "boolean", Description = "Indicates if this is the primary email")]
        public bool Primary { get; set; }
    }

    public class ScimPhoneNumber
    {
        [ScimProperty("value", "string", Description = "Phone number")]
        public string Value { get; set; } = string.Empty;
        
        [ScimProperty("type", "string", Description = "Type of phone number (e.g., work, home, mobile)")]
        public string? Type { get; set; }
        
        [ScimProperty("primary", "boolean", Description = "Indicates if this is the primary phone number")]
        public bool Primary { get; set; }
    }

    public class ScimAddress
    {
        [ScimProperty("formatted", "string", Description = "The full mailing address, formatted for display")]
        public string? Formatted { get; set; }
        
        [ScimProperty("streetAddress", "string", Description = "The full street address")]
        public string? StreetAddress { get; set; }
        
        [ScimProperty("locality", "string", Description = "The city or locality")]
        public string? Locality { get; set; }
        
        [ScimProperty("region", "string", Description = "The state or region")]
        public string? Region { get; set; }
        
        [ScimProperty("postalCode", "string", Description = "The zip code or postal code")]
        public string? PostalCode { get; set; }
        
        [ScimProperty("country", "string", Description = "The country name")]
        public string? Country { get; set; }
        
        [ScimProperty("type", "string", Description = "Type of address (e.g., work, home)")]
        public string? Type { get; set; }
        
        [ScimProperty("primary", "boolean", Description = "Indicates if this is the primary address")]
        public bool Primary { get; set; }
    }

    public class ScimGroupMembership
    {
        [ScimProperty("value", "string", Description = "Group identifier")]
        public string Value { get; set; } = string.Empty;
        
        [ScimProperty("$ref", "reference", Description = "URI reference to the group resource")]
        public string? Ref { get; set; }
        
        [ScimProperty("display", "string", Description = "Display name of the group")]
        public string? Display { get; set; }
    }
}

