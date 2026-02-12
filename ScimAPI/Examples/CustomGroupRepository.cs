using ScimAPI.Repositories;

namespace ScimAPI.Examples
{
    /// <summary>
    /// Example implementation of IGroupDataRepository using in-memory storage.
    /// In production, this would connect to your database (EF Core, Dapper, etc.)
    /// </summary>
    public class CustomGroupRepository : IGroupDataRepository<CustomGroup>
    {
        private readonly Dictionary<string, CustomGroup> _groups = new();

        public Task<CustomGroup?> GetAsync(string id)
        {
            _groups.TryGetValue(id, out var group);
            return Task.FromResult(group);
        }

        public IQueryable<CustomGroup> Query()
        {
            // In production, this would return DbSet<CustomGroup> or similar
            return _groups.Values.AsQueryable();
        }

        public Task<CustomGroup> CreateAsync(CustomGroup group)
        {
            if (string.IsNullOrEmpty(group.Id))
            {
                group.Id = Guid.NewGuid().ToString();
            }

            group.CreatedAt = DateTime.UtcNow;
            group.ModifiedAt = DateTime.UtcNow;

            _groups[group.Id] = group;
            return Task.FromResult(group);
        }

        public Task<CustomGroup?> UpdateAsync(string id, CustomGroup group)
        {
            if (!_groups.ContainsKey(id))
            {
                return Task.FromResult<CustomGroup?>(null);
            }

            group.Id = id;
            group.ModifiedAt = DateTime.UtcNow;
            _groups[id] = group;

            return Task.FromResult<CustomGroup?>(group);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return Task.FromResult(_groups.Remove(id));
        }
    }
}

