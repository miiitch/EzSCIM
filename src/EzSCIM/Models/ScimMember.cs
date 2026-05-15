using EzSCIM.Attributes;

namespace EzSCIM.Models;

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