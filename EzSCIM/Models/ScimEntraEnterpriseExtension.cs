using EzSCIM.Attributes;

namespace EzSCIM.Models;

public class ScimEntraEnterpriseExtension
{
    [ScimProperty("manager", "complex", Description = "The user's manager")]
    public ScimEntraManager Manager { get; set; } = new();
}