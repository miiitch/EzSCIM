using EzSCIM.Filtering.AST;
using EzSCIM.Models;

namespace EzSCIM.Repositories
{
    /// <summary>
    /// Repository interface for SCIM User resource management only.
    /// Handles all User CRUD operations and queries.
    /// Use this when your provider only supports User resources.
    /// </summary>
    /// <typeparam name="TUser">The user type, must inherit from ScimUser</typeparam>
    public interface IScimUserOnlyRepository<TUser> where TUser : ScimUser
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
    /// Repository interface for SCIM User and Group resource management.
    /// Inherits all User operations from IScimUserOnlyRepository and adds Group operations.
    /// In SCIM, groups always reference users, so a group-only repository has no meaning.
    /// </summary>
    /// <typeparam name="TUser">The user type, must inherit from ScimUser</typeparam>
    /// <typeparam name="TGroup">The group type, must inherit from ScimGroup</typeparam>
    public interface IScimUserGroupRepository<TUser, TGroup> : IScimUserOnlyRepository<TUser>
        where TUser : ScimUser
        where TGroup : ScimGroup
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
    /// This is a backward-compatible alias for IScimUserGroupRepository with concrete types.
    /// Inherit from this interface if your implementation supports both Users and Groups.
    /// </summary>
    public interface IScimRepository : IScimUserGroupRepository<ScimUser, ScimGroup>
    {
    }
}

