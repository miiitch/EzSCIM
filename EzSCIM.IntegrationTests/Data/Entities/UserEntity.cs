using EzSCIM.Attributes;
using EzSCIM.Constants;
using EzSCIM.EfCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace EzSCIM.IntegrationTests.Data.Entities;

/// <summary>
/// Entity Framework user entity with SCIM property mappings for integration tests.
/// Supports all standard SCIM User attributes plus custom extensions.
/// Multi-valued attributes (emails, phoneNumbers, addresses) are stored as JSON.
/// </summary>
public class UserEntity : IScimEntity
{
    // ========== Core SCIM Fields ==========

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
    /// Formatted name (maps to SCIM name.formatted)
    /// </summary>
    [ScimProperty("name.formatted", "string")]
    public string? NameFormatted { get; set; }

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
    /// Middle name (maps to SCIM name.middleName)
    /// </summary>
    [ScimProperty("name.middleName", "string")]
    public string? NameMiddleName { get; set; }

    /// <summary>
    /// Honorific prefix (maps to SCIM name.honorificPrefix)
    /// </summary>
    [ScimProperty("name.honorificPrefix", "string")]
    public string? NameHonorificPrefix { get; set; }

    /// <summary>
    /// Honorific suffix (maps to SCIM name.honorificSuffix)
    /// </summary>
    [ScimProperty("name.honorificSuffix", "string")]
    public string? NameHonorificSuffix { get; set; }

    // ========== Multi-Valued Attributes (JSON) ==========

    /// <summary>
    /// Emails stored as JSON array.
    /// Format: [{"value":"email@example.com","type":"work","primary":true}]
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? EmailsJson { get; set; }

    /// <summary>
    /// Phone numbers stored as JSON array.
    /// Format: [{"value":"123-456-7890","type":"work","primary":true}]
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? PhoneNumbersJson { get; set; }

    /// <summary>
    /// Addresses stored as JSON array.
    /// Format: [{"formatted":"123 Main St","streetAddress":"123 Main St","locality":"City","region":"State","postalCode":"12345","country":"USA","type":"work","primary":true}]
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? AddressesJson { get; set; }

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

    // ========== Additional SCIM User Fields ==========

    /// <summary>
    /// Nickname (maps to SCIM nickName)
    /// </summary>
    [ScimProperty("nickName", "string")]
    public string? NickName { get; set; }

    /// <summary>
    /// Profile URL (maps to SCIM profileUrl)
    /// </summary>
    [ScimProperty("profileUrl", "string")]
    public string? ProfileUrl { get; set; }

    /// <summary>
    /// User type (maps to SCIM userType)
    /// </summary>
    [ScimProperty("userType", "string")]
    public string? UserType { get; set; }

    /// <summary>
    /// Preferred language (maps to SCIM preferredLanguage)
    /// </summary>
    [ScimProperty("preferredLanguage", "string")]
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// Locale (maps to SCIM locale)
    /// </summary>
    [ScimProperty("locale", "string")]
    public string? Locale { get; set; }

    /// <summary>
    /// Timezone (maps to SCIM timezone)
    /// </summary>
    [ScimProperty("timezone", "string")]
    public string? Timezone { get; set; }

    // ========== Custom Extension Fields ==========

    /// <summary>
    /// Department (maps to SCIM enterprise extension)
    /// </summary>
    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:department", "string")]
    public string? Department { get; set; }

    /// <summary>
    /// Manager ID (maps to SCIM enterprise extension)
    /// </summary>
    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:manager.value", "string")]
    public string? ManagerId { get; set; }

    /// <summary>
    /// Cost center (maps to SCIM enterprise extension)
    /// </summary>
    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:costCenter", "string")]
    public string? CostCenter { get; set; }

    /// <summary>
    /// Organization (maps to SCIM enterprise extension)
    /// </summary>
    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:organization", "string")]
    public string? Organization { get; set; }

    /// <summary>
    /// Division (maps to SCIM enterprise extension)
    /// </summary>
    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:division", "string")]
    public string? Division { get; set; }

    /// <summary>
    /// Employee number (maps to SCIM enterprise extension)
    /// </summary>
    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:employeeNumber", "string")]
    public string? EmployeeNumber { get; set; }

    // ========== Custom Extension Fields ==========

    /// <summary>
    /// Custom field 1 - for testing custom attribute support
    /// </summary>
    [ScimProperty("urn:scim:custom:User:customField1", "string")]
    public string? CustomField1 { get; set; }

    /// <summary>
    /// Custom field 2 - for testing custom attribute support
    /// </summary>
    [ScimProperty("urn:scim:custom:User:customField2", "string")]
    public string? CustomField2 { get; set; }

    /// <summary>
    /// Custom boolean field - for testing custom boolean attribute support
    /// </summary>
    [ScimProperty("urn:scim:custom:User:isVip", "boolean")]
    public bool IsVip { get; set; } = false;

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

