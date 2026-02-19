using EzSCIM.IntegrationTests.Data.Entities;
using EzSCIM.DataRepositories;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.IntegrationTests.Data.Repositories;

/// <summary>
/// Entity Framework implementation of IUserDataRepository for integration tests.
/// </summary>
public class EfUserDataRepository : IUserDataRepository<UserEntity>
{
    private readonly ScimDbContext _context;

    public EfUserDataRepository(ScimDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetAsync(string id)
    {
        return await _context.Users.FindAsync(id);
    }

    public IQueryable<UserEntity> Query()
    {
        return _context.Users.AsQueryable();
    }

    public async Task<UserEntity> CreateAsync(UserEntity user)
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

    public async Task<UserEntity?> UpdateAsync(string id, UserEntity user)
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

    public async Task<bool> DeleteAsync(string id)
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
}

