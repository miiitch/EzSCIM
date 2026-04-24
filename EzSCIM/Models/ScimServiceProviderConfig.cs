namespace EzSCIM.Models
{
    public class ScimServiceProviderConfig
    {
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:schemas:core:2.0:ServiceProviderConfig" };
        public string? DocumentationUri { get; set; }
        public ScimPatchConfig Patch { get; set; } = new();
        public ScimBulkConfig Bulk { get; set; } = new();
        public ScimFilterConfig Filter { get; set; } = new();
        public ScimChangePasswordConfig ChangePassword { get; set; } = new();
        public ScimSortConfig Sort { get; set; } = new();
        public ScimEtagConfig Etag { get; set; } = new();
        public List<ScimAuthenticationScheme> AuthenticationSchemes { get; set; } = new();
    }
}

