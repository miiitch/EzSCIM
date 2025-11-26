namespace ScimAPI.Models
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

    public class ScimPatchConfig
    {
        public bool Supported { get; set; } = true;
    }

    public class ScimBulkConfig
    {
        public bool Supported { get; set; } = false;
        public int MaxOperations { get; set; } = 0;
        public int MaxPayloadSize { get; set; } = 0;
    }

    public class ScimFilterConfig
    {
        public bool Supported { get; set; } = true;
        public int MaxResults { get; set; } = 200;
    }

    public class ScimChangePasswordConfig
    {
        public bool Supported { get; set; } = false;
    }

    public class ScimSortConfig
    {
        public bool Supported { get; set; } = false;
    }

    public class ScimEtagConfig
    {
        public bool Supported { get; set; } = false;
    }

    public class ScimAuthenticationScheme
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? SpecUri { get; set; }
        public string? DocumentationUri { get; set; }
    }
}

