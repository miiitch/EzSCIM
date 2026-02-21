using EzSCIM.Attributes;
using EzSCIM.Helpers;
using System.Text.Json.Serialization;

namespace EzSCIM.Models;

public class ScimEntraRole
{
    [ScimProperty("value", "string", Description = "The value of a role")]
    public string? Value { get; set; }

    [ScimProperty("display", "string", Description = "Human-readable role name")]
    public string? Display { get; set; }

    [ScimProperty("type", "string", Description = "Label indicating the attribute's function")]
    public string? Type { get; set; }

    [ScimProperty("primary", "boolean", Description = "Indicates if this is the primary role")]
    [JsonConverter(typeof(FlexibleBooleanJsonConverter))]
    public bool Primary { get; set; }
}