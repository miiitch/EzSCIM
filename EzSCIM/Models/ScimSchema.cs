namespace EzSCIM.Models
{
    public class ScimSchema
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:schemas:core:2.0:Schema" };
        public List<ScimSchemaAttribute> Attributes { get; set; } = new();
    }
}

