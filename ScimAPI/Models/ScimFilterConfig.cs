namespace ScimAPI.Models;

public class ScimFilterConfig
{
    public bool Supported { get; set; } = true;
    public int MaxResults { get; set; } = 200;
}