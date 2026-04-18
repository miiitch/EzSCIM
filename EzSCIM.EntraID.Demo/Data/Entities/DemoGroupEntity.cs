using EzSCIM.Attributes;
using EzSCIM.Constants;
using EzSCIM.EfCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EzSCIM.EntraID.Demo.Data.Entities;

/// <summary>
/// EF entity for SCIM Groups stored in SQL Server (Azure SQL or local container).
/// Members are stored as JSON in a nvarchar(max) column.
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

    // Members stored as JSON (nvarchar(max) for SQL Server / Azure SQL)
    [Column(TypeName = "nvarchar(max)")]
    public string? MembersJson { get; set; }

    [ScimProperty("description", "string")]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}

