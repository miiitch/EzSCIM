﻿using ScimAPI.Filtering.AST;
using ScimAPI.Models;

namespace ScimAPI.Repositories
{
    /// <summary>
    /// Repository interface for SCIM User resource management.
    /// Handles all User CRUD operations and queries.
    /// </summary>
    /// <typeparam name="TUser">The user type, must inherit from ScimUser</typeparam>
    public interface IScimUserRepository<TUser> where TUser : ScimUser
    {
        /// <summary>
        /// Gets a user by unique identifier.
        /// </summary>
        Task<TUser?> GetUserAsync(string id);
        
        /// <summary>
        /// Gets a user by username.
        /// </summary>
        Task<TUser?> GetUserByUserNameAsync(string userName);
        
        /// <summary>
        /// Gets a paginated list of users with optional filtering using FilterExpression AST.
        /// </summary>
        Task<ScimListResponse<TUser>> GetUsersAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100);
        
        /// <summary>
        /// Creates a new user.
        /// </summary>
        Task<TUser> CreateUserAsync(TUser user);
        
        /// <summary>
        /// Updates an existing user (full replacement - PUT operation).
        /// </summary>
        Task<TUser?> UpdateUserAsync(string id, TUser user);
        
        /// <summary>
        /// Partially updates a user (PATCH operation).
        /// </summary>
        Task<TUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest);
        
        /// <summary>
        /// Deletes a user.
        /// </summary>
        Task<bool> DeleteUserAsync(string id);
    }

    /// <summary>
    /// Repository interface for SCIM Group resource management.
    /// Handles all Group CRUD operations and queries.
    /// </summary>
    /// <typeparam name="TGroup">The group type, must inherit from ScimGroup</typeparam>
    public interface IScimGroupRepository<TGroup> where TGroup : ScimGroup
    {
        /// <summary>
        /// Gets a group by unique identifier.
        /// </summary>
        Task<TGroup?> GetGroupAsync(string id);
        
        /// <summary>
        /// Gets a group by display name.
        /// </summary>
        Task<TGroup?> GetGroupByDisplayNameAsync(string displayName);
        
        /// <summary>
        /// Gets a paginated list of groups with optional filtering using FilterExpression AST.
        /// </summary>
        Task<ScimListResponse<TGroup>> GetGroupsAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100);
        
        /// <summary>
        /// Creates a new group.
        /// </summary>
        Task<TGroup> CreateGroupAsync(TGroup group);
        
        /// <summary>
        /// Updates an existing group (full replacement - PUT operation).
        /// </summary>
        Task<TGroup?> UpdateGroupAsync(string id, TGroup group);
        
        /// <summary>
        /// Partially updates a group (PATCH operation).
        /// </summary>
        Task<TGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest);
        
        /// <summary>
        /// Deletes a group.
        /// </summary>
        Task<bool> DeleteGroupAsync(string id);
    }

    /// <summary>
    /// Main SCIM repository interface that combines User and Group management.
    /// This interface indicates that the provider supports both User and Group resources.
    /// Inherit from this interface if your implementation supports both Users and Groups.
    /// Uses concrete types (ScimUser, ScimGroup) to maintain DI compatibility.
    /// </summary>
    public interface IScimRepository : IScimUserRepository<ScimUser>, IScimGroupRepository<ScimGroup>
    {
    }
}

