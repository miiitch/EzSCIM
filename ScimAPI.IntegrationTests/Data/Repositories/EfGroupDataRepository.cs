using ScimAPI.DataRepositories;
using ScimAPI.IntegrationTests.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ScimAPI.IntegrationTests.Data.Repositories;

/// <summary>
/// Entity Framework implementation of IGroupDataRepository for integration tests.
/// </summary>
public class EfGroupDataRepository : IGroupDataRepository<GroupEntity>
{
    private readonly ScimDbContext _context;

    public EfGroupDataRepository(ScimDbContext context)
    {
        _context = context;
    }

    public async Task<GroupEntity?> GetAsync(string id)
    {
        return await _context.Groups.FindAsync(id);
    }

    public IQueryable<GroupEntity> Query()
    {
        return _context.Groups.AsQueryable();
    }

    public async Task<GroupEntity> CreateAsync(GroupEntity group)
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

    public async Task<GroupEntity?> UpdateAsync(string id, GroupEntity group)
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

    public async Task<bool> DeleteAsync(string id)
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
}

