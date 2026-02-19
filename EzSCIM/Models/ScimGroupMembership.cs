using EzSCIM.Attributes;

namespace EzSCIM.Models;

public class ScimGroupMembership
{
    [ScimProperty("value", "string", Description = "Group identifier")]
    public string Value { get; set; } = string.Empty;
        
    [ScimProperty("$ref", "reference", Description = "URI reference to the group resource")]
    public string? Ref { get; set; }
        
    [ScimProperty("display", "string", Description = "Display name of the group")]
    public string? Display { get; set; }
}