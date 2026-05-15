using EzSCIM.DataRepositories;

namespace EzSCIM.EntraID.Demo.Examples
{
    /// <summary>
    /// Example implementation of IUserGroupDataRepository using in-memory storage.
    /// In production, this would connect to your database (EF Core, Dapper, etc.)
    /// Manages both users and groups in a single repository since SCIM groups always reference users.
    /// </summary>
    public class CustomUserGroupRepository : IUserGroupDataRepository<CustomUser, CustomGroup>
    {
        private readonly Dictionary<string, CustomUser> _users = new();
        private readonly Dictionary<string, CustomGroup> _groups = new();

        #region User Operations

        public Task<CustomUser?> GetUserAsync(string id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public IQueryable<CustomUser> QueryUsers()
        {
            // In production, this would return DbSet<CustomUser> or similar
            return _users.Values.AsQueryable();
        }

        public Task<CustomUser> CreateUserAsync(CustomUser user)
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

        public Task<CustomUser?> UpdateUserAsync(string id, CustomUser user)
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

        public Task<bool> DeleteUserAsync(string id)
        {
            return Task.FromResult(_users.Remove(id));
        }

        #endregion

        #region Group Operations

        public Task<CustomGroup?> GetGroupAsync(string id)
        {
            _groups.TryGetValue(id, out var group);
            return Task.FromResult(group);
        }

        public IQueryable<CustomGroup> QueryGroups()
        {
            // In production, this would return DbSet<CustomGroup> or similar
            return _groups.Values.AsQueryable();
        }

        public Task<CustomGroup> CreateGroupAsync(CustomGroup group)
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

        public Task<CustomGroup?> UpdateGroupAsync(string id, CustomGroup group)
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

        public Task<bool> DeleteGroupAsync(string id)
        {
            return Task.FromResult(_groups.Remove(id));
        }

        #endregion
    }
}

