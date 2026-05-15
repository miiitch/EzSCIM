﻿using EzSCIM.Filtering;
using EzSCIM.Filtering.AST;
using EzSCIM.Models;
using EzSCIM.Services;
using Microsoft.Extensions.Logging;

namespace EzSCIM.Repositories
{
    /// <summary>
    /// Example implementation of a Users-only SCIM repository.
    /// This shows how to implement only IScimUserOnlyRepository when your provider
    /// only supports User resources (no Groups).
    /// </summary>
    public class UsersOnlyRepository : IScimUserOnlyRepository<ScimUser>
    {
        private readonly ILogger<UsersOnlyRepository> _logger;
        private readonly Dictionary<string, ScimUser> _users = new();

        public UsersOnlyRepository(ILogger<UsersOnlyRepository> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets a user by unique identifier.
        /// </summary>
        public Task<ScimUser?> GetUserAsync(string id)
        {
            _logger.LogInformation("Getting user: {UserId}", id);
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        /// <summary>
        /// Gets a user by username.
        /// </summary>
        public Task<ScimUser?> GetUserByUserNameAsync(string userName)
        {
            _logger.LogInformation("Getting user by username: {UserName}", userName);
            var user = _users.Values.FirstOrDefault(u => u.UserName == userName);
            return Task.FromResult(user);
        }

        /// <summary>
        /// Gets a paginated list of users with optional filtering using FilterExpression AST.
        /// </summary>
        public Task<ScimListResponse<ScimUser>> GetUsersAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100)
        {
            _logger.LogInformation("Getting users. Filter: {Filter}, StartIndex: {StartIndex}, Count: {Count}", 
                filter, startIndex, count);

            var allUsers = _users.Values.AsEnumerable();

            if (filter != null)
            {
                allUsers = allUsers.Where(filter);
            }

            allUsers = allUsers.ToList();

            var users = allUsers
                .Skip(startIndex - 1)
                .Take(count)
                .ToList();

            return Task.FromResult(new ScimListResponse<ScimUser>
            {
                TotalResults = allUsers.Count(),
                ItemsPerPage = count,
                StartIndex = startIndex,
                Resources = users
            });
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        public Task<ScimUser> CreateUserAsync(ScimUser user)
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }

            user.Meta = new ScimMeta
            {
                ResourceType = "User",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Location = $"/Users/{user.Id}"
            };

            _users[user.Id] = user;
            _logger.LogInformation("Created user: {UserId}", user.Id);
            return Task.FromResult(user);
        }

        /// <summary>
        /// Updates an existing user (full replacement).
        /// </summary>
        public Task<ScimUser?> UpdateUserAsync(string id, ScimUser user)
        {
            if (!_users.ContainsKey(id))
            {
                return Task.FromResult<ScimUser?>(null);
            }

            var existingUser = _users[id];
            user.Id = id;
            user.Meta = new ScimMeta
            {
                ResourceType = "User",
                Created = existingUser.Meta.Created,
                LastModified = DateTime.UtcNow,
                Location = $"/Users/{id}"
            };

            _users[id] = user;
            _logger.LogInformation("Updated user: {UserId}", id);
            return Task.FromResult<ScimUser?>(user);
        }

        /// <summary>
        /// Partially updates a user (PATCH operation).
        /// </summary>
        public Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest)
        {
            if (!_users.TryGetValue(id, out var user))
            {
                return Task.FromResult<ScimUser?>(null);
            }

            ScimPatchService.ApplyPatch(user, patchRequest);
            _logger.LogInformation("Patched user: {UserId}", id);
            return Task.FromResult<ScimUser?>(user);
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        public Task<bool> DeleteUserAsync(string id)
        {
            var deleted = _users.Remove(id);
            if (deleted)
            {
                _logger.LogInformation("Deleted user: {UserId}", id);
            }
            return Task.FromResult(deleted);
        }
    }
}

