using EzSCIM.DataRepositories;
using EzSCIM.Demo.Data.Entities;
using EzSCIM.Filtering;
using EzSCIM.Filtering.AST;
using EzSCIM.Models;
using EzSCIM.Repositories;
using EzSCIM.Services;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.Demo.Data;

/// <summary>
/// SCIM repository backed by EF Core.
/// Uses <see cref="IUserGroupDataRepository{TUser,TGroup}"/> for raw EF CRUD and entity
/// extension methods to handle JSON multi-valued attributes (emails, phones, addresses, members).
/// This is the main <see cref="IScimRepository"/> implementation for demo applications.
/// </summary>
public class DemoScimRepository : IScimRepository
{
    private readonly IUserGroupDataRepository<DemoUserEntity, DemoGroupEntity> _data;
    private readonly IScimFilterTranslator<DemoUserEntity> _userFilter;
    private readonly IScimFilterTranslator<DemoGroupEntity> _groupFilter;

    public DemoScimRepository(
        IUserGroupDataRepository<DemoUserEntity, DemoGroupEntity> data,
        IScimFilterTranslator<DemoUserEntity> userFilter,
        IScimFilterTranslator<DemoGroupEntity> groupFilter)
    {
        _data = data;
        _userFilter = userFilter;
        _groupFilter = groupFilter;
    }

    // -------------------------------------------------------------------------
    // User operations
    // -------------------------------------------------------------------------

    public async Task<ScimUser?> GetUserAsync(string id)
    {
        var entity = await _data.GetUserAsync(id);
        return entity?.ToScimUser();
    }

    public async Task<ScimUser?> GetUserByUserNameAsync(string userName)
    {
        var entity = await _data.QueryUsers()
            .Where(u => u.UserName.ToLower() == userName.ToLower())
            .FirstOrDefaultAsync();
        return entity?.ToScimUser();
    }

    public async Task<ScimListResponse<ScimUser>> GetUsersAsync(
        FilterExpression? filter = null,
        int startIndex = 1,
        int count = 100)
    {
        var query = _data.QueryUsers();

        if (filter != null)
        {
            var predicate = _userFilter.BuildPredicate(filter);
            if (predicate != null)
                query = query.Where(predicate);
        }

        var total = await query.CountAsync();
        var users = await query.Skip(startIndex - 1).Take(count).ToListAsync();

        return new ScimListResponse<ScimUser>
        {
            TotalResults = total,
            StartIndex = startIndex,
            ItemsPerPage = users.Count,
            Resources = users.Select(u => u.ToScimUser()).ToList()
        };
    }

    public async Task<ScimUser> CreateUserAsync(ScimUser scimUser)
    {
        var entity = new DemoUserEntity
        {
            Id = string.IsNullOrEmpty(scimUser.Id) ? Guid.NewGuid().ToString() : scimUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
        entity.UpdateFromScimUser(scimUser);

        var created = await _data.CreateUserAsync(entity);
        return created.ToScimUser();
    }

    public async Task<ScimUser?> UpdateUserAsync(string id, ScimUser scimUser)
    {
        var existing = await _data.GetUserAsync(id);
        if (existing is null) return null;

        existing.UpdateFromScimUser(scimUser);

        var updated = await _data.UpdateUserAsync(id, existing);
        return updated?.ToScimUser();
    }

    public async Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest)
    {
        var existing = await _data.GetUserAsync(id);
        if (existing is null) return null;

        var scimUser = existing.ToScimUser();
        ScimPatchService.ApplyPatch(scimUser, patchRequest);
        existing.UpdateFromScimUser(scimUser);

        var updated = await _data.UpdateUserAsync(id, existing);
        return updated?.ToScimUser();
    }

    public async Task<bool> DeleteUserAsync(string id)
        => await _data.DeleteUserAsync(id);

    // -------------------------------------------------------------------------
    // Group operations
    // -------------------------------------------------------------------------

    public async Task<ScimGroup?> GetGroupAsync(string id)
    {
        var entity = await _data.GetGroupAsync(id);
        return entity?.ToScimGroup();
    }

    public async Task<ScimGroup?> GetGroupByDisplayNameAsync(string displayName)
    {
        var entity = await _data.QueryGroups()
            .Where(g => g.DisplayName.ToLower() == displayName.ToLower())
            .FirstOrDefaultAsync();
        return entity?.ToScimGroup();
    }

    public async Task<ScimListResponse<ScimGroup>> GetGroupsAsync(
        FilterExpression? filter = null,
        int startIndex = 1,
        int count = 100)
    {
        var query = _data.QueryGroups();

        if (filter != null)
        {
            var predicate = _groupFilter.BuildPredicate(filter);
            if (predicate != null)
                query = query.Where(predicate);
        }

        var total = await query.CountAsync();
        var groups = await query.Skip(startIndex - 1).Take(count).ToListAsync();

        return new ScimListResponse<ScimGroup>
        {
            TotalResults = total,
            StartIndex = startIndex,
            ItemsPerPage = groups.Count,
            Resources = groups.Select(g => g.ToScimGroup()).ToList()
        };
    }

    public async Task<ScimGroup> CreateGroupAsync(ScimGroup scimGroup)
    {
        var entity = new DemoGroupEntity
        {
            Id = string.IsNullOrEmpty(scimGroup.Id) ? Guid.NewGuid().ToString() : scimGroup.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
        entity.UpdateFromScimGroup(scimGroup);

        var created = await _data.CreateGroupAsync(entity);
        return created.ToScimGroup();
    }

    public async Task<ScimGroup?> UpdateGroupAsync(string id, ScimGroup scimGroup)
    {
        var existing = await _data.GetGroupAsync(id);
        if (existing is null) return null;

        existing.UpdateFromScimGroup(scimGroup);

        var updated = await _data.UpdateGroupAsync(id, existing);
        return updated?.ToScimGroup();
    }

    public async Task<ScimGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest)
    {
        var existing = await _data.GetGroupAsync(id);
        if (existing is null) return null;

        var scimGroup = existing.ToScimGroup();
        ScimPatchService.ApplyPatch(scimGroup, patchRequest);
        existing.UpdateFromScimGroup(scimGroup);

        var updated = await _data.UpdateGroupAsync(id, existing);
        return updated?.ToScimGroup();
    }

    public async Task<bool> DeleteGroupAsync(string id)
        => await _data.DeleteGroupAsync(id);
}

