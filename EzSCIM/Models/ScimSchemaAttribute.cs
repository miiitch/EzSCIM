namespace EzSCIM.Models;

public class ScimSchemaAttribute
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool MultiValued { get; set; } = false;
    public string? Description { get; set; }
    public bool Required { get; set; } = false;
    public bool CaseExact { get; set; } = false;
    public string Mutability { get; set; } = "readWrite";
    public string Returned { get; set; } = "default";
    public string Uniqueness { get; set; } = "none";
    public List<ScimSchemaAttribute>? SubAttributes { get; set; }
}