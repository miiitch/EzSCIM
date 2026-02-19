using EzSCIM.Attributes;
using EzSCIM.Constants;
using System.ComponentModel.DataAnnotations;

namespace EzSCIM.IntegrationTests.Data.Entities;

/// <summary>
/// Entity Framework group entity with SCIM property mappings for integration tests.
/// Supports all standard SCIM Group attributes plus custom extensions.
/// </summary>
public class GroupEntity
{
    // ========== Core SCIM Fields ==========

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

    // ========== Additional SCIM Group Fields ==========

    /// <summary>
    /// Description of the group (optional SCIM field)
    /// </summary>
    [ScimProperty("description", "string")]
    public string? Description { get; set; }

    // ========== Custom Extension Fields ==========

    /// <summary>
    /// Custom field 1 - for testing custom attribute support
    /// </summary>
    [ScimProperty("urn:scim:custom:Group:customField1", "string")]
    public string? CustomField1 { get; set; }

    /// <summary>
    /// Custom field 2 - for testing custom attribute support
    /// </summary>
    [ScimProperty("urn:scim:custom:Group:customField2", "string")]
    public string? CustomField2 { get; set; }

    /// <summary>
    /// Is admin group - for testing custom boolean attribute
    /// </summary>
    [ScimProperty("urn:scim:custom:Group:isAdminGroup", "boolean")]
    public bool IsAdminGroup { get; set; } = false;

    // ========== Metadata ==========

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}

