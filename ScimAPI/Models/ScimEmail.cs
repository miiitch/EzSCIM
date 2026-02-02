using ScimAPI.Attributes;

namespace ScimAPI.Models;

public class ScimEmail
{
    [ScimProperty("value", "string", Description = "Email address")]
    public string Value { get; set; } = string.Empty;
        
    [ScimProperty("type", "string", Description = "Type of email address (e.g., work, home)")]
    public string? Type { get; set; }
        
    [ScimProperty("primary", "boolean", Description = "Indicates if this is the primary email")]
    public bool Primary { get; set; }
}