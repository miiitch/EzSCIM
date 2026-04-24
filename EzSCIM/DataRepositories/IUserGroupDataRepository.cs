namespace EzSCIM.DataRepositories
{
    /// <summary>
    /// Generic repository interface for combined user and group data sources.
    /// Inherits user operations from IUserDataRepository and adds group operations.
    /// In SCIM, groups always reference users, so a group-only repository has no meaning.
    /// </summary>
    /// <typeparam name="TUser">Your user class with SCIM property attributes</typeparam>
    /// <typeparam name="TGroup">Your group class with SCIM property attributes</typeparam>
    public interface IUserGroupDataRepository<TUser, TGroup> : IUserDataRepository<TUser>
        where TUser : class
        where TGroup : class
    {
        /// <summary>
        /// Gets a group by unique identifier.
        /// </summary>
        Task<TGroup?> GetGroupAsync(string id);

        /// <summary>
        /// Returns a queryable source of all groups.
        /// This will be used with IScimFilterTranslator to apply SCIM filters server-side.
        /// </summary>
        IQueryable<TGroup> QueryGroups();

        /// <summary>
        /// Creates a new group.
        /// </summary>
        Task<TGroup> CreateGroupAsync(TGroup group);

        /// <summary>
        /// Updates an existing group.
        /// </summary>
        Task<TGroup?> UpdateGroupAsync(string id, TGroup group);

        /// <summary>
        /// Deletes a group.
        /// </summary>
        Task<bool> DeleteGroupAsync(string id);
    }
}

