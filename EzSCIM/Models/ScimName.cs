using EzSCIM.Attributes;

namespace EzSCIM.Models;

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