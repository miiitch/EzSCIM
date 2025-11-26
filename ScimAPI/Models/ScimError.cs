namespace ScimAPI.Models
{
    public class ScimError
    {
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:api:messages:2.0:Error" };
        public string? Detail { get; set; }
        public int Status { get; set; }
        public string? ScimType { get; set; }
    }
}

