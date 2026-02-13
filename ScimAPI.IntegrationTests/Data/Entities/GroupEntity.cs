using ScimAPI.Attributes;
using ScimAPI.Constants;
using System.ComponentModel.DataAnnotations;

namespace ScimAPI.IntegrationTests.Data.Entities;

/// <summary>
/// Entity Framework group entity with SCIM property mappings for integration tests.
/// </summary>
public class GroupEntity
{
    /// <summary>
    /// Unique identifier (Primary Key)
    /// </summary>
    [Key]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name (maps to SCIM displayName - REQUIRED)
    /// </summary>
    [ScimProperty(ScimAttributeNames.Group.DisplayName, "string", Required = true, Uniqueness = "server")]
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// External system ID (maps to SCIM externalId)
    /// </summary>
    [ScimProperty(ScimAttributeNames.Common.ExternalId, "string")]
    public string? ExternalId { get; set; }

    /// <summary>
    /// Group members (stored as JSON for simplicity)
    /// Format: [{"value": "userId1", "display": "User Name"}, ...]
    /// </summary>
    public string? MembersJson { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}

