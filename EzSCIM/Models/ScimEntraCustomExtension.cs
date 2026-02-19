using EzSCIM.Attributes;

namespace EzSCIM.Models;

public class ScimEntraCustomExtension
{
    [ScimProperty("tag", "string", Description = "Custom tag")]
    public string? Tag { get; set; }
}