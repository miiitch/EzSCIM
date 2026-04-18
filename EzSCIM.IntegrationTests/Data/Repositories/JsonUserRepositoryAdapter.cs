﻿using EzSCIM.DataRepositories;
using EzSCIM.Filtering;
using EzSCIM.Filtering.AST;
using EzSCIM.IntegrationTests.Data.Entities;
using EzSCIM.Models;
using EzSCIM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.IntegrationTests.Data.Repositories;

/// <summary>
/// SCIM User repository adapter for UserEntity with JSON multi-valued attributes
/// </summary>
public class JsonUserRepositoryAdapter : IScimUserOnlyRepository<ScimUser>
{
    private readonly IUserDataRepository<UserEntity> _dataRepo;
    private readonly IScimFilterTranslator<UserEntity> _filterTranslator;

    public JsonUserRepositoryAdapter(
        IUserDataRepository<UserEntity> dataRepo,
        IScimFilterTranslator<UserEntity> filterTranslator)
    {
        _dataRepo = dataRepo;
        _filterTranslator = filterTranslator;
    }

    public async Task<ScimUser?> GetUserAsync(string id)
    {
        var user = await _dataRepo.GetUserAsync(id);
        return user?.ToScimUser();
    }

    public async Task<ScimUser?> GetUserByUserNameAsync(string userName)
    {
        var user = await _dataRepo.QueryUsers()
            .Where(u => u.UserName.ToLower() == userName.ToLower())
            .FirstOrDefaultAsync();

        return user?.ToScimUser();
    }

    public async Task<ScimListResponse<ScimUser>> GetUsersAsync(
        FilterExpression? filter = null,
        int startIndex = 1,
        int count = 100)
    {
        var query = _dataRepo.QueryUsers();

        // Apply filter if provided
        if (filter != null)
        {
            var predicate = _filterTranslator.BuildPredicate(filter);
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
        }

        var totalResults = await query.CountAsync();

        // Apply pagination
        var users = await query
            .Skip(startIndex - 1)
            .Take(count)
            .ToListAsync();

        return new ScimListResponse<ScimUser>
        {
            TotalResults = totalResults,
            ItemsPerPage = users.Count,
            StartIndex = startIndex,
            Resources = users.Select(u => u.ToScimUser()).ToList()
        };
    }

    public async Task<ScimUser> CreateUserAsync(ScimUser scimUser)
    {
        var entity = new UserEntity
        {
            Id = string.IsNullOrEmpty(scimUser.Id) ? Guid.NewGuid().ToString() : scimUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        entity.UpdateFromScimUser(scimUser);

        var created = await _dataRepo.CreateUserAsync(entity);
        return created.ToScimUser();
    }

    public async Task<ScimUser?> UpdateUserAsync(string id, ScimUser scimUser)
    {
        var existing = await _dataRepo.GetUserAsync(id);
        if (existing == null)
            return null;

        existing.UpdateFromScimUser(scimUser);
        existing.ModifiedAt = DateTime.UtcNow;

        var updated = await _dataRepo.UpdateUserAsync(id, existing);
        return updated?.ToScimUser();
    }

    public async Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest)
    {
        var existing = await _dataRepo.GetUserAsync(id);
        if (existing == null)
            return null;

        // Convert to ScimUser, apply PATCH via library, convert back
        var scimUser = existing.ToScimUser();
        EzSCIM.Services.ScimPatchService.ApplyPatch(scimUser, patchRequest);
        existing.UpdateFromScimUser(scimUser);
        existing.ModifiedAt = DateTime.UtcNow;

        var updated = await _dataRepo.UpdateUserAsync(id, existing);
        return updated?.ToScimUser();
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        return await _dataRepo.DeleteUserAsync(id);
    }
}



