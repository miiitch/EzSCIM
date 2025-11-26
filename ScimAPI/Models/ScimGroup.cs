namespace ScimAPI.Models
{
    public class ScimGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ExternalId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<ScimMember> Members { get; set; } = new();
        public Dictionary<string, object> CustomAttributes { get; set; } = new();
        public ScimMeta Meta { get; set; } = new();
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:schemas:core:2.0:Group" };
    }

    public class ScimMember
    {
        public string Value { get; set; } = string.Empty;
        public string? Ref { get; set; }
        public string? Display { get; set; }
        public string Type { get; set; } = "User";
    }
}

