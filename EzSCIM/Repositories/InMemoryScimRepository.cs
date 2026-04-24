﻿using EzSCIM.Filtering;
using EzSCIM.Filtering.AST;
using EzSCIM.Models;
using EzSCIM.Services;
using System.Collections.Concurrent;

namespace EzSCIM.Repositories
{
    /// <summary>
    /// In-memory implementation of SCIM repository for both Users and Groups.
    /// This is a reference implementation suitable for testing and development.
    /// </summary>
    public class InMemoryScimRepository : IScimRepository
    {
        private readonly ConcurrentDictionary<string, ScimUser> _users = new();
        private readonly ConcurrentDictionary<string, ScimGroup> _groups = new();

        #region User Operations

        public Task<ScimUser?> GetUserAsync(string id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<ScimUser?> GetUserByUserNameAsync(string userName)
        {
            var user = _users.Values.FirstOrDefault(u => 
                u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<ScimListResponse<ScimUser>> GetUsersAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100)
        {
            var users = _users.Values.AsEnumerable();

            if (filter != null)
            {
                users = users.Where(filter);
            }

            var usersList = users.ToList();
            var totalResults = usersList.Count;
            var pagedUsers = usersList.Skip(startIndex - 1).Take(count).ToList();

            return Task.FromResult(new ScimListResponse<ScimUser>
            {
                TotalResults = totalResults,
                StartIndex = startIndex,
                ItemsPerPage = pagedUsers.Count,
                Resources = pagedUsers
            });
        }

        public Task<ScimUser> CreateUserAsync(ScimUser user)
        {
            user.Id = Guid.NewGuid().ToString();
            user.Meta = new ScimMeta
            {
                ResourceType = "User",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Location = $"/scim/Users/{user.Id}"
            };

            _users[user.Id] = user;
            return Task.FromResult(user);
        }

        public Task<ScimUser?> UpdateUserAsync(string id, ScimUser user)
        {
            if (!_users.ContainsKey(id))
                return Task.FromResult<ScimUser?>(null);

            user.Id = id;
            user.Meta.LastModified = DateTime.UtcNow;
            user.Meta.Location = $"/scim/Users/{id}";
            user.Meta.ResourceType = "User";

            _users[id] = user;
            return Task.FromResult<ScimUser?>(user);
        }

        public Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest)
        {
            if (!_users.TryGetValue(id, out var user))
                return Task.FromResult<ScimUser?>(null);

            ScimPatchService.ApplyPatch(user, patchRequest);
            return Task.FromResult<ScimUser?>(user);
        }

        public Task<bool> DeleteUserAsync(string id)
        {
            return Task.FromResult(_users.TryRemove(id, out _));
        }

        #endregion

        #region Group Operations

        public Task<ScimGroup?> GetGroupAsync(string id)
        {
            _groups.TryGetValue(id, out var group);
            return Task.FromResult(group);
        }

        public Task<ScimGroup?> GetGroupByDisplayNameAsync(string displayName)
        {
            var group = _groups.Values.FirstOrDefault(g => 
                g.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(group);
        }

        public Task<ScimListResponse<ScimGroup>> GetGroupsAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100)
        {
            var groups = _groups.Values.AsEnumerable();

            if (filter != null)
            {
                groups = groups.Where(filter);
            }

            var groupsList = groups.ToList();
            var totalResults = groupsList.Count;
            var pagedGroups = groupsList.Skip(startIndex - 1).Take(count).ToList();

            return Task.FromResult(new ScimListResponse<ScimGroup>
            {
                TotalResults = totalResults,
                StartIndex = startIndex,
                ItemsPerPage = pagedGroups.Count,
                Resources = pagedGroups
            });
        }

        public Task<ScimGroup> CreateGroupAsync(ScimGroup group)
        {
            group.Id = Guid.NewGuid().ToString();
            group.Meta = new ScimMeta
            {
                ResourceType = "Group",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Location = $"/scim/Groups/{group.Id}"
            };

            _groups[group.Id] = group;
            return Task.FromResult(group);
        }

        public Task<ScimGroup?> UpdateGroupAsync(string id, ScimGroup group)
        {
            if (!_groups.ContainsKey(id))
                return Task.FromResult<ScimGroup?>(null);

            group.Id = id;
            group.Meta.LastModified = DateTime.UtcNow;
            group.Meta.Location = $"/scim/Groups/{id}";
            group.Meta.ResourceType = "Group";

            _groups[id] = group;
            return Task.FromResult<ScimGroup?>(group);
        }

        public Task<ScimGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest)
        {
            if (!_groups.TryGetValue(id, out var group))
                return Task.FromResult<ScimGroup?>(null);

            ScimPatchService.ApplyPatch(group, patchRequest);
            return Task.FromResult<ScimGroup?>(group);
        }

        public Task<bool> DeleteGroupAsync(string id)
        {
            return Task.FromResult(_groups.TryRemove(id, out _));
        }

        #endregion
    }
}
