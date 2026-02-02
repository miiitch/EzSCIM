using ScimAPI.Attributes;

namespace ScimAPI.Models;

public class ScimPhoneNumber
{
    [ScimProperty("value", "string", Description = "Phone number")]
    public string Value { get; set; } = string.Empty;
        
    [ScimProperty("type", "string", Description = "Type of phone number (e.g., work, home, mobile)")]
    public string? Type { get; set; }
        
    [ScimProperty("primary", "boolean", Description = "Indicates if this is the primary phone number")]
    public bool Primary { get; set; }
}