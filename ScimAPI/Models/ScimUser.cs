namespace ScimAPI.Models
{
    public class ScimUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ExternalId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public ScimName Name { get; set; } = new();
        public string? DisplayName { get; set; }
        public string? NickName { get; set; }
        public string? ProfileUrl { get; set; }
        public string? Title { get; set; }
        public string? UserType { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? Locale { get; set; }
        public string? Timezone { get; set; }
        public bool Active { get; set; } = true;
        public List<ScimEmail> Emails { get; set; } = new();
        public List<ScimPhoneNumber> PhoneNumbers { get; set; } = new();
        public List<ScimAddress> Addresses { get; set; } = new();
        public List<ScimGroupMembership> Groups { get; set; } = new();
        public Dictionary<string, object> CustomAttributes { get; set; } = new();
        public ScimMeta Meta { get; set; } = new();
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:schemas:core:2.0:User" };
    }

    public class ScimName
    {
        public string? Formatted { get; set; }
        public string? FamilyName { get; set; }
        public string? GivenName { get; set; }
        public string? MiddleName { get; set; }
        public string? HonorificPrefix { get; set; }
        public string? HonorificSuffix { get; set; }
    }

    public class ScimEmail
    {
        public string Value { get; set; } = string.Empty;
        public string? Type { get; set; }
        public bool Primary { get; set; }
    }

    public class ScimPhoneNumber
    {
        public string Value { get; set; } = string.Empty;
        public string? Type { get; set; }
        public bool Primary { get; set; }
    }

    public class ScimAddress
    {
        public string? Formatted { get; set; }
        public string? StreetAddress { get; set; }
        public string? Locality { get; set; }
        public string? Region { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Type { get; set; }
        public bool Primary { get; set; }
    }

    public class ScimGroupMembership
    {
        public string Value { get; set; } = string.Empty;
        public string? Ref { get; set; }
        public string? Display { get; set; }
    }
}

