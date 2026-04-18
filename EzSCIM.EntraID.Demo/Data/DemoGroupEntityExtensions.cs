using EzSCIM.EntraID.Demo.Data.Entities;
using EzSCIM.Models;

namespace EzSCIM.EntraID.Demo.Data;

/// <summary>
/// Extension methods to convert DemoGroupEntity ↔ ScimGroup.
/// Handles JSON-serialized members list.
/// </summary>
public static class DemoGroupEntityExtensions
{
    /// <summary>
    /// Converts a <see cref="DemoGroupEntity"/> to a <see cref="ScimGroup"/>.
    /// </summary>
    public static ScimGroup ToScimGroup(this DemoGroupEntity group)
    {
        var scimGroup = new ScimGroup
        {
            Id = group.Id,
            DisplayName = group.DisplayName,
            ExternalId = group.ExternalId
        };

        var members = MultiValuedAttributeHelper.DeserializeMembers(group.MembersJson);
        if (members.Count > 0)
        {
            scimGroup.Members = members.Select(m => new ScimMember
            {
                Value = m.Value,
                Display = m.Display
            }).ToList();
        }

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
    /// Updates a <see cref="DemoGroupEntity"/> from a <see cref="ScimGroup"/>.
    /// </summary>
    public static void UpdateFromScimGroup(this DemoGroupEntity entity, ScimGroup scimGroup)
    {
        entity.DisplayName = scimGroup.DisplayName;
        entity.ExternalId = scimGroup.ExternalId;

        entity.MembersJson = MultiValuedAttributeHelper.SerializeMembers(
            scimGroup.Members?.Select(m => new MemberData
            {
                Value = m.Value ?? "",
                Display = m.Display ?? ""
            }).ToList() ?? []);

        entity.ModifiedAt = DateTime.UtcNow;
    }
}

