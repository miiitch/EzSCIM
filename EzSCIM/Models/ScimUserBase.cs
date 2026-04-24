using EzSCIM.Attributes;

namespace EzSCIM.Models;

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