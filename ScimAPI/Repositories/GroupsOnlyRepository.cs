using ScimAPI.Models;
using ScimAPI.Repositories;

namespace ScimAPI.Repositories.Examples
{
    /// <summary>
    /// Example implementation of a Groups-only SCIM repository.
    /// This shows how to implement only IScimGroupRepository when your provider
    /// only supports Group resources (no Users or Schemas).
    /// </summary>
    public class GroupsOnlyRepository : IScimGroupRepository
    {
        private readonly ILogger<GroupsOnlyRepository> _logger;
        private readonly Dictionary<string, ScimGroup> _groups = new();

        public GroupsOnlyRepository(ILogger<GroupsOnlyRepository> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets a group by unique identifier.
        /// </summary>
        public Task<ScimGroup?> GetGroupAsync(string id)
        {
            _logger.LogInformation("Getting group: {GroupId}", id);
            _groups.TryGetValue(id, out var group);
            return Task.FromResult(group);
        }

        /// <summary>
        /// Gets a group by display name.
        /// </summary>
        public Task<ScimGroup?> GetGroupByDisplayNameAsync(string displayName)
        {
            _logger.LogInformation("Getting group by display name: {DisplayName}", displayName);
            var group = _groups.Values.FirstOrDefault(g => g.DisplayName == displayName);
            return Task.FromResult(group);
        }

        /// <summary>
        /// Gets a paginated list of groups with optional filtering.
        /// </summary>
        public Task<ScimListResponse<ScimGroup>> GetGroupsAsync(string? filter = null, int startIndex = 1, int count = 100)
        {
            _logger.LogInformation("Getting groups. Filter: {Filter}, StartIndex: {StartIndex}, Count: {Count}", 
                filter, startIndex, count);

            var allGroups = _groups.Values.ToList();

            // Simple filtering example (in production, use a proper SCIM filter parser)
            if (!string.IsNullOrEmpty(filter))
            {
                if (filter.Contains("displayName"))
                {
                    var displayName = filter.Split("\"")[1];
                    allGroups = allGroups.Where(g => g.DisplayName == displayName).ToList();
                }
            }

            var groups = allGroups
                .Skip(startIndex - 1)
                .Take(count)
                .ToList();

            return Task.FromResult(new ScimListResponse<ScimGroup>
            {
                TotalResults = allGroups.Count,
                ItemsPerPage = count,
                StartIndex = startIndex,
                Resources = groups
            });
        }

        /// <summary>
        /// Creates a new group.
        /// </summary>
        public Task<ScimGroup> CreateGroupAsync(ScimGroup group)
        {
            if (string.IsNullOrEmpty(group.Id))
            {
                group.Id = Guid.NewGuid().ToString();
            }

            group.Meta = new ScimMeta
            {
                ResourceType = "Group",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Location = $"/Groups/{group.Id}"
            };

            _groups[group.Id] = group;
            _logger.LogInformation("Created group: {GroupId}", group.Id);
            return Task.FromResult(group);
        }

        /// <summary>
        /// Updates an existing group (full replacement).
        /// </summary>
        public Task<ScimGroup?> UpdateGroupAsync(string id, ScimGroup group)
        {
            if (!_groups.ContainsKey(id))
            {
                return Task.FromResult<ScimGroup?>(null);
            }

            group.Id = id;
            group.Meta = new ScimMeta
            {
                ResourceType = "Group",
                Created = _groups[id].Meta?.Created ?? DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Location = $"/Groups/{id}"
            };

            _groups[id] = group;
            _logger.LogInformation("Updated group: {GroupId}", id);
            return Task.FromResult<ScimGroup?>(group);
        }

        /// <summary>
        /// Partially updates a group (PATCH operation).
        /// </summary>
        public Task<ScimGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest)
        {
            if (!_groups.TryGetValue(id, out var group))
            {
                return Task.FromResult<ScimGroup?>(null);
            }

            // Simple PATCH implementation (in production, implement full PATCH semantics)
            foreach (var operation in patchRequest.Operations)
            {
                if (operation.Op == "replace")
                {
                    if (operation.Path == "displayName" && operation.Value is string displayName)
                    {
                        group.DisplayName = displayName;
                    }
                }
            }

            group.Meta!.LastModified = DateTime.UtcNow;
            _logger.LogInformation("Patched group: {GroupId}", id);
            return Task.FromResult<ScimGroup?>(group);
        }

        /// <summary>
        /// Deletes a group.
        /// </summary>
        public Task<bool> DeleteGroupAsync(string id)
        {
            var deleted = _groups.Remove(id);
            if (deleted)
            {
                _logger.LogInformation("Deleted group: {GroupId}", id);
            }
            return Task.FromResult(deleted);
        }
    }
}
