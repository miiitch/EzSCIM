using EzSCIM.Models;
using System.Text.Json;

namespace EzSCIM.IntegrationTests.Data;

/// <summary>
/// Extension methods for converting GroupEntity with JSON columns to ScimGroup
/// </summary>
public static class GroupEntityExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Converts GroupEntity (with JSON columns) to ScimGroup
    /// </summary>
    public static ScimGroup ToScimGroup(this Entities.GroupEntity group)
    {
        var scimGroup = new ScimGroup
        {
            Id = group.Id,
            DisplayName = group.DisplayName,
            ExternalId = group.ExternalId
        };

        // Map Members from JSON
        if (!string.IsNullOrWhiteSpace(group.MembersJson))
        {
            try
            {
                var members = JsonSerializer.Deserialize<List<MemberData>>(group.MembersJson, JsonOptions);
                if (members != null && members.Count > 0)
                {
                    scimGroup.Members = members.Select(m => new ScimMember
                    {
                        Value = m.Value,
                        Display = m.Display
                    }).ToList();
                }
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        // Set metadata
        scimGroup.Meta = new ScimMeta
        {
            ResourceType = "Group",
            Created = group.CreatedAt,
            LastModified = group.ModifiedAt,
            Location = $"/scim/Groups/{group.Id}"
        };

        return scimGroup;
    }

    /// <summary>
    /// Updates GroupEntity from ScimGroup (for CREATE/UPDATE/PATCH operations)
    /// </summary>
    public static void UpdateFromScimGroup(this Entities.GroupEntity entity, ScimGroup scimGroup)
    {
        entity.DisplayName = scimGroup.DisplayName;
        entity.ExternalId = scimGroup.ExternalId;

        // Map Members to JSON
        if (scimGroup.Members != null && scimGroup.Members.Count > 0)
        {
            var membersData = scimGroup.Members.Select(m => new MemberData
            {
                Value = m.Value,
                Display = m.Display ?? ""
            }).ToList();
            entity.MembersJson = JsonSerializer.Serialize(membersData, JsonOptions);
        }
        else
        {
            entity.MembersJson = "[]";
        }

        entity.ModifiedAt = DateTime.UtcNow;
    }

    private class MemberData
    {
        public string Value { get; set; } = "";
        public string Display { get; set; } = "";
    }
}

