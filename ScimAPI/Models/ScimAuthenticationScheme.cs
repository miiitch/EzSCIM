namespace ScimAPI.Models;

public class ScimAuthenticationScheme
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? SpecUri { get; set; }
    public string? DocumentationUri { get; set; }
}