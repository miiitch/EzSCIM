using ScimAPI.Attributes;

namespace ScimAPI.Models;

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