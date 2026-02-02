namespace ScimAPI.Models;

public class ScimPatchOperation
{
    public string Op { get; set; } = string.Empty;
    public string? Path { get; set; }
    public object? Value { get; set; }
}