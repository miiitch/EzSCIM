namespace ScimAPI.Models
{
    public class ScimPatchRequest
    {
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };
        public List<ScimPatchOperation> Operations { get; set; } = new();
    }
}

