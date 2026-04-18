using EzSCIM.Attributes;
using EzSCIM.Constants;
using EzSCIM.EfCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EzSCIM.EntraID.Demo.Data.Entities;

/// <summary>
/// EF entity for SCIM Users stored in SQL Server (Azure SQL or local container).
/// Multi-valued attributes (emails, phones, addresses) are stored as JSON in nvarchar(max) columns.
/// </summary>
public class DemoUserEntity : IScimEntity
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [ScimProperty(ScimAttributeNames.User.UserName, "string", Required = true, Uniqueness = "server")]
    [Required]
    public string UserName { get; set; } = string.Empty;

    [ScimProperty(ScimAttributeNames.User.DisplayName, "string")]
    public string? DisplayName { get; set; }

    [ScimProperty(ScimAttributeNames.User.Active, "boolean")]
    public bool Active { get; set; } = true;

    [ScimProperty("name.formatted", "string")]
    public string? NameFormatted { get; set; }

    [ScimProperty(ScimAttributeNames.User.NameGivenName, "string")]
    public string? FirstName { get; set; }

    [ScimProperty(ScimAttributeNames.User.NameFamilyName, "string")]
    public string? LastName { get; set; }

    [ScimProperty("name.middleName", "string")]
    public string? NameMiddleName { get; set; }

    [ScimProperty("name.honorificPrefix", "string")]
    public string? NameHonorificPrefix { get; set; }

    [ScimProperty("name.honorificSuffix", "string")]
    public string? NameHonorificSuffix { get; set; }

    // Multi-valued attributes stored as JSON (nvarchar(max) for SQL Server / Azure SQL)
    [Column(TypeName = "nvarchar(max)")]
    public string? EmailsJson { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? PhoneNumbersJson { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? AddressesJson { get; set; }

    [ScimProperty(ScimAttributeNames.User.Title, "string")]
    public string? Title { get; set; }

    [ScimProperty(ScimAttributeNames.Common.ExternalId, "string")]
    public string? ExternalId { get; set; }

    [ScimProperty("nickName", "string")]
    public string? NickName { get; set; }

    [ScimProperty("profileUrl", "string")]
    public string? ProfileUrl { get; set; }

    [ScimProperty("userType", "string")]
    public string? UserType { get; set; }

    [ScimProperty("preferredLanguage", "string")]
    public string? PreferredLanguage { get; set; }

    [ScimProperty("locale", "string")]
    public string? Locale { get; set; }

    [ScimProperty("timezone", "string")]
    public string? Timezone { get; set; }

    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:department", "string")]
    public string? Department { get; set; }

    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:manager.value", "string")]
    public string? ManagerId { get; set; }

    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:costCenter", "string")]
    public string? CostCenter { get; set; }

    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:organization", "string")]
    public string? Organization { get; set; }

    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:division", "string")]
    public string? Division { get; set; }

    [ScimProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:employeeNumber", "string")]
    public string? EmployeeNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}

