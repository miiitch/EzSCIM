using EzSCIM.Attributes;

namespace EzSCIM.Models;

/// <summary>
/// Base class with REQUIRED attributes for SCIM Group (RFC 7643)
/// </summary>
[ScimResource(
    "urn:ietf:params:scim:schemas:core:2.0:Group",
    "Group",
    "Group")]
public class ScimGroupBase
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
}