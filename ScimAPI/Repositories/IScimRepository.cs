using ScimAPI.Models;

namespace ScimAPI.Repositories
{
    /// <summary>
    /// Repository interface for SCIM User resource management.
    /// Handles all User CRUD operations and queries.
    /// </summary>
    public interface IScimUserRepository
    {
        /// <summary>
        /// Gets a user by unique identifier.
        /// </summary>
        Task<ScimUser?> GetUserAsync(string id);
        
        /// <summary>
        /// Gets a user by username.
        /// </summary>
        Task<ScimUser?> GetUserByUserNameAsync(string userName);
        
        /// <summary>
        /// Gets a paginated list of users with optional filtering.
        /// </summary>
        Task<ScimListResponse<ScimUser>> GetUsersAsync(string? filter = null, int startIndex = 1, int count = 100);
        
        /// <summary>
        /// Creates a new user.
        /// </summary>
        Task<ScimUser> CreateUserAsync(ScimUser user);
        
        /// <summary>
        /// Updates an existing user (full replacement - PUT operation).
        /// </summary>
        Task<ScimUser?> UpdateUserAsync(string id, ScimUser user);
        
        /// <summary>
        /// Partially updates a user (PATCH operation).
        /// </summary>
        Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest);
        
        /// <summary>
        /// Deletes a user.
        /// </summary>
        Task<bool> DeleteUserAsync(string id);
    }

    /// <summary>
    /// Repository interface for SCIM Group resource management.
    /// Handles all Group CRUD operations and queries.
    /// </summary>
    public interface IScimGroupRepository
    {
        /// <summary>
        /// Gets a group by unique identifier.
        /// </summary>
        Task<ScimGroup?> GetGroupAsync(string id);
        
        /// <summary>
        /// Gets a group by display name.
        /// </summary>
        Task<ScimGroup?> GetGroupByDisplayNameAsync(string displayName);
        
        /// <summary>
        /// Gets a paginated list of groups with optional filtering.
        /// </summary>
        Task<ScimListResponse<ScimGroup>> GetGroupsAsync(string? filter = null, int startIndex = 1, int count = 100);
        
        /// <summary>
        /// Creates a new group.
        /// </summary>
        Task<ScimGroup> CreateGroupAsync(ScimGroup group);
        
        /// <summary>
        /// Updates an existing group (full replacement - PUT operation).
        /// </summary>
        Task<ScimGroup?> UpdateGroupAsync(string id, ScimGroup group);
        
        /// <summary>
        /// Partially updates a group (PATCH operation).
        /// </summary>
        Task<ScimGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest);
        
        /// <summary>
        /// Deletes a group.
        /// </summary>
        Task<bool> DeleteGroupAsync(string id);
    }

    /// <summary>
    /// Repository interface for SCIM Schema management.
    /// Handles custom schema storage and retrieval.
    /// </summary>
    public interface IScimSchemaRepository
    {
        /// <summary>
        /// Gets all custom SCIM schemas.
        /// </summary>
        Task<List<ScimSchema>> GetCustomSchemasAsync();
        
        /// <summary>
        /// Adds or updates a custom SCIM schema.
        /// </summary>
        Task AddCustomSchemaAsync(ScimSchema schema);
    }

    /// <summary>
    /// Main SCIM repository interface that combines User, Group, and Schema management.
    /// This interface indicates that the provider supports both User and Group resources.
    /// Inherit from this interface if your implementation supports both Users and Groups.
    /// </summary>
    public interface IScimRepository : IScimUserRepository, IScimGroupRepository, IScimSchemaRepository
    {
    }
}

