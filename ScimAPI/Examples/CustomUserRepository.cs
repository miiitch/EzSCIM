using ScimAPI.Repositories;

namespace ScimAPI.Examples
{
    /// <summary>
    /// Example implementation of IUserDataRepository using in-memory storage.
    /// In production, this would connect to your database (EF Core, Dapper, etc.)
    /// </summary>
    public class CustomUserRepository : IUserDataRepository<CustomUser>
    {
        private readonly Dictionary<string, CustomUser> _users = new();

        public Task<CustomUser?> GetAsync(string id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public IQueryable<CustomUser> Query()
        {
            // In production, this would return DbSet<CustomUser> or similar
            return _users.Values.AsQueryable();
        }

        public Task<CustomUser> CreateAsync(CustomUser user)
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }

            user.CreatedAt = DateTime.UtcNow;
            user.ModifiedAt = DateTime.UtcNow;

            _users[user.Id] = user;
            return Task.FromResult(user);
        }

        public Task<CustomUser?> UpdateAsync(string id, CustomUser user)
        {
            if (!_users.ContainsKey(id))
            {
                return Task.FromResult<CustomUser?>(null);
            }

            user.Id = id;
            user.ModifiedAt = DateTime.UtcNow;
            _users[id] = user;

            return Task.FromResult<CustomUser?>(user);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return Task.FromResult(_users.Remove(id));
        }
    }
}


