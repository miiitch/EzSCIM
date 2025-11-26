namespace ScimAPI.Models
{
    public class ScimListResponse<T>
    {
        public List<string> Schemas { get; set; } = new() { "urn:ietf:params:scim:api:messages:2.0:ListResponse" };
        public int TotalResults { get; set; }
        public int StartIndex { get; set; } = 1;
        public int ItemsPerPage { get; set; }
        public List<T> Resources { get; set; } = new();
    }
}

