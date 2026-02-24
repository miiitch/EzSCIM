using EzSCIM.IntegrationTests.Data.Entities;
using EzSCIM.DataRepositories;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.IntegrationTests.Data.Repositories;

/// <summary>
/// Entity Framework implementation of IUserGroupDataRepository for integration tests.
/// Manages both User and Group entities through a single repository.
/// </summary>
public class EfUserGroupDataRepository : IUserGroupDataRepository<UserEntity, GroupEntity>
{
    private readonly ScimDbContext _context;

    public EfUserGroupDataRepository(ScimDbContext context)
    {
        _context = context;
    }

    #region User Operations

    public async Task<UserEntity?> GetUserAsync(string id)
    {
        return await _context.Users.FindAsync(id);
    }

    public IQueryable<UserEntity> QueryUsers()
    {
        return _context.Users.AsQueryable();
    }

    public async Task<UserEntity> CreateUserAsync(UserEntity user)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            user.Id = Guid.NewGuid().ToString();
        }

        user.CreatedAt = DateTime.UtcNow;
        user.ModifiedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<UserEntity?> UpdateUserAsync(string id, UserEntity user)
    {
        var existing = await _context.Users.FindAsync(id);
        if (existing == null)
        {
            return null;
        }

        user.Id = id;
        user.CreatedAt = existing.CreatedAt;
        user.ModifiedAt = DateTime.UtcNow;

        _context.Entry(existing).CurrentValues.SetValues(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Group Operations

    public async Task<GroupEntity?> GetGroupAsync(string id)
    {
        return await _context.Groups.FindAsync(id);
    }

    public IQueryable<GroupEntity> QueryGroups()
    {
        return _context.Groups.AsQueryable();
    }

    public async Task<GroupEntity> CreateGroupAsync(GroupEntity group)
    {
        if (string.IsNullOrEmpty(group.Id))
        {
            group.Id = Guid.NewGuid().ToString();
        }

        group.CreatedAt = DateTime.UtcNow;
        group.ModifiedAt = DateTime.UtcNow;

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return group;
    }

    public async Task<GroupEntity?> UpdateGroupAsync(string id, GroupEntity group)
    {
        var existing = await _context.Groups.FindAsync(id);
        if (existing == null)
        {
            return null;
        }

        group.Id = id;
        group.CreatedAt = existing.CreatedAt;
        group.ModifiedAt = DateTime.UtcNow;

        _context.Entry(existing).CurrentValues.SetValues(group);
        await _context.SaveChangesAsync();

        return group;
    }

    public async Task<bool> DeleteGroupAsync(string id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null)
        {
            return false;
        }

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion
}

