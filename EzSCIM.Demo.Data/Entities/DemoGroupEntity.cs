using EzSCIM.Attributes;
using EzSCIM.Constants;
using EzSCIM.EfCore;
using System.ComponentModel.DataAnnotations;

namespace EzSCIM.Demo.Data.Entities;

/// <summary>
/// EF entity for SCIM Groups. Provider-agnostic — column types are configured
/// by the provider-specific DbContext subclass (SQL Server, PostgreSQL, etc.).
/// Members are stored as JSON.
/// </summary>
public class DemoGroupEntity : IScimEntity
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [ScimProperty(ScimAttributeNames.Group.DisplayName, "string", Required = true, Uniqueness = "server")]
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [ScimProperty(ScimAttributeNames.Common.ExternalId, "string")]
    public string? ExternalId { get; set; }

    // Members stored as JSON (column type set by provider-specific DbContext)
    public string? MembersJson { get; set; }

    [ScimProperty("description", "string")]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}

