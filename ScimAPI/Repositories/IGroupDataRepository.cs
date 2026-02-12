namespace ScimAPI.Repositories
{
    /// <summary>
    /// Generic repository interface for group data sources.
    /// Implement this interface to connect your existing group storage to SCIM.
    /// TGroup properties should be annotated with [ScimProperty] for automatic mapping.
    /// </summary>
    /// <typeparam name="TGroup">Your group class with SCIM property attributes</typeparam>
    public interface IGroupDataRepository<TGroup> where TGroup : class
    {
        /// <summary>
        /// Gets a group by unique identifier.
        /// </summary>
        Task<TGroup?> GetAsync(string id);

        /// <summary>
        /// Returns a queryable source of all groups.
        /// This will be used with IScimFilterTranslator to apply SCIM filters server-side.
        /// </summary>
        IQueryable<TGroup> Query();

        /// <summary>
        /// Creates a new group.
        /// </summary>
        Task<TGroup> CreateAsync(TGroup group);

        /// <summary>
        /// Updates an existing group.
        /// </summary>
        Task<TGroup?> UpdateAsync(string id, TGroup group);

        /// <summary>
        /// Deletes a group.
        /// </summary>
        Task<bool> DeleteAsync(string id);
    }
}

