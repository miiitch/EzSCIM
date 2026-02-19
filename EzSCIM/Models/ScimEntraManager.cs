using EzSCIM.Attributes;

namespace EzSCIM.Models;

public class ScimEntraManager
{
    [ScimProperty("value", "string", Description = "The id of the SCIM resource representing the User's manager")]
    public string? Value { get; set; }

    [ScimProperty("$ref", "reference", Description = "The URI of the SCIM resource representing the User's manager")]
    public string? Ref { get; set; }

    [ScimProperty("displayName", "string", Description = "The displayName of the User's manager")]
    public string? DisplayName { get; set; }
}