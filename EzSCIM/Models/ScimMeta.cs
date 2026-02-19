namespace EzSCIM.Models
{
    public class ScimMeta
    {
        public string ResourceType { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public string? Location { get; set; }
        public string? Version { get; set; }
    }
}

