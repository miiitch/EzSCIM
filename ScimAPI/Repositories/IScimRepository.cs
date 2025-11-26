using ScimAPI.Models;

namespace ScimAPI.Repositories
{
    public interface IScimRepository
    {
        Task<ScimUser?> GetUserAsync(string id);
        Task<ScimUser?> GetUserByUserNameAsync(string userName);
        Task<ScimListResponse<ScimUser>> GetUsersAsync(string? filter = null, int startIndex = 1, int count = 100);
        Task<ScimUser> CreateUserAsync(ScimUser user);
        Task<ScimUser?> UpdateUserAsync(string id, ScimUser user);
        Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest);
        Task<bool> DeleteUserAsync(string id);
        
        Task<ScimGroup?> GetGroupAsync(string id);
        Task<ScimGroup?> GetGroupByDisplayNameAsync(string displayName);
        Task<ScimListResponse<ScimGroup>> GetGroupsAsync(string? filter = null, int startIndex = 1, int count = 100);
        Task<ScimGroup> CreateGroupAsync(ScimGroup group);
        Task<ScimGroup?> UpdateGroupAsync(string id, ScimGroup group);
        Task<ScimGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest);
        Task<bool> DeleteGroupAsync(string id);
        
        Task<List<ScimSchema>> GetCustomSchemasAsync();
        Task AddCustomSchemaAsync(ScimSchema schema);
    }
}

