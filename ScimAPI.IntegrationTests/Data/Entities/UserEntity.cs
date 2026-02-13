using ScimAPI.Attributes;
using ScimAPI.Constants;
using System.ComponentModel.DataAnnotations;

namespace ScimAPI.IntegrationTests.Data.Entities;

/// <summary>
/// Entity Framework user entity with SCIM property mappings for integration tests.
/// </summary>
public class UserEntity
{
    /// <summary>
    /// Unique identifier (Primary Key)
    /// </summary>
    [Key]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Username (maps to SCIM userName - REQUIRED)
    /// </summary>
    [ScimProperty(ScimAttributeNames.User.UserName, "string", Required = true, Uniqueness = "server")]
    [Required]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Display name (maps to SCIM displayName)
    /// </summary>
    [ScimProperty(ScimAttributeNames.User.DisplayName, "string")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Is user active (maps to SCIM active)
    /// </summary>
    [ScimProperty(ScimAttributeNames.User.Active, "boolean")]
    public bool Active { get; set; } = true;

    /// <summary>
    /// First name (maps to SCIM name.givenName)
    /// </summary>
    [ScimProperty(ScimAttributeNames.User.NameGivenName, "string")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name (maps to SCIM name.familyName)
    /// </summary>
    [ScimProperty(ScimAttributeNames.User.NameFamilyName, "string")]
    public string? LastName { get; set; }

    /// <summary>
    /// Email address (maps to SCIM emails[0].value)
    /// </summary>
    [ScimProperty("emails[0].value", "string")]
    public string? Email { get; set; }

    /// <summary>
    /// Job title (maps to SCIM title)
    /// </summary>
    [ScimProperty(ScimAttributeNames.User.Title, "string")]
    public string? Title { get; set; }

    /// <summary>
    /// External system ID (maps to SCIM externalId)
    /// </summary>
    [ScimProperty(ScimAttributeNames.Common.ExternalId, "string")]
    public string? ExternalId { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}

