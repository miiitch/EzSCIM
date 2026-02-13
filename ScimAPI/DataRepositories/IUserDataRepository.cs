namespace ScimAPI.DataRepositories
{
    /// <summary>
    /// Generic repository interface for user data sources.
    /// Implement this interface to connect your existing user storage to SCIM.
    /// TUser properties should be annotated with [ScimProperty] for automatic mapping.
    /// </summary>
    /// <typeparam name="TUser">Your user class with SCIM property attributes</typeparam>
    public interface IUserDataRepository<TUser> where TUser : class
    {
        /// <summary>
        /// Gets a user by unique identifier.
        /// </summary>
        Task<TUser?> GetAsync(string id);

        /// <summary>
        /// Returns a queryable source of all users.
        /// This will be used with IScimFilterTranslator to apply SCIM filters server-side.
        /// </summary>
        IQueryable<TUser> Query();

        /// <summary>
        /// Creates a new user.
        /// </summary>
        Task<TUser> CreateAsync(TUser user);

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        Task<TUser?> UpdateAsync(string id, TUser user);

        /// <summary>
        /// Deletes a user.
        /// </summary>
        Task<bool> DeleteAsync(string id);
    }
}

