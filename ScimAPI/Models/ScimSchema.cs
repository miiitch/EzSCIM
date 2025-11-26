namespace ScimAPI.Models
{
    public class ScimSchema
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:schemas:core:2.0:Schema" };
        public List<ScimSchemaAttribute> Attributes { get; set; } = new();
    }

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
}

