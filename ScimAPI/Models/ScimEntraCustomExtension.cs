using ScimAPI.Attributes;

namespace ScimAPI.Models;

public class ScimEntraCustomExtension
{
    [ScimProperty("tag", "string", Description = "Custom tag")]
    public string? Tag { get; set; }
}