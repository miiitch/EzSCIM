namespace ScimAPI.Models;

public class ScimBulkConfig
{
    public bool Supported { get; set; } = false;
    public int MaxOperations { get; set; } = 0;
    public int MaxPayloadSize { get; set; } = 0;
}