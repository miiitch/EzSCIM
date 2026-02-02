using ScimAPI.Attributes;

namespace ScimAPI.Models
{
    /// <summary>
    /// SCIM User tailored for Microsoft Entra mappings and extensions.
    /// </summary>
    public class ScimEntraUser : ScimUserBase
    {
        /// <summary>
        /// External identifier defined by the provisioning client.
        /// </summary>
        [ScimProperty("externalId", "string", Description = "External identifier defined by the provisioning client")]
        public string? ExternalId { get; set; }

        /// <summary>
        /// User's name with given and family names.
        /// </summary>
        [ScimProperty("name", "complex", Description = "User's name")]
        public ScimName Name { get; set; } = new();

        /// <summary>
        /// Enterprise extension for Entra (manager reference).
        /// </summary>
        [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User", "complex", Description = "Enterprise user extension")]
        public ScimEntraEnterpriseExtension EnterpriseExtension { get; set; } = new();

        /// <summary>
        /// Custom extension for Entra (tag).
        /// </summary>
        [ScimProperty("urn:ietf:params:scim:schemas:extension:CustomExtensionName:2.0:User", "complex", Description = "Custom user extension")]
        public ScimEntraCustomExtension CustomExtension { get; set; } = new();

        /// <summary>
        /// Roles for the user (OPTIONAL, multi-valued).
        /// </summary>
        [ScimProperty("roles", "complex", MultiValued = true, Description = "A list of roles for the User")]
        public List<ScimEntraRole> Roles { get; set; } = new();

        public ScimEntraUser()
        {
            Schemas = new List<string>
            {
                "urn:ietf:params:scim:schemas:core:2.0:User",
                "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User",
                "urn:ietf:params:scim:schemas:extension:CustomExtensionName:2.0:User"
            };
        }
    }
}
