using System.Linq.Expressions;
using ScimAPI.Filtering.AST;

namespace ScimAPI.Filtering
{
    /// <summary>
    /// Interface for translating SCIM FilterExpression AST to LINQ Expression predicates.
    /// This allows filters to be executed server-side (e.g., in EF Core, SQL, etc.)
    /// instead of in-memory enumeration.
    /// </summary>
    /// <typeparam name="TUser">The user type to filter</typeparam>
    public interface IScimFilterTranslator<TUser>
    {
        /// <summary>
        /// Converts a SCIM FilterExpression to a LINQ Expression predicate.
        /// </summary>
        /// <param name="filter">The SCIM filter AST</param>
        /// <returns>A LINQ expression that can be used with IQueryable.Where()</returns>
        Expression<Func<TUser, bool>>? BuildPredicate(FilterExpression? filter);

        /// <summary>
        /// Applies a SCIM filter to an IQueryable source.
        /// </summary>
        /// <param name="source">The queryable source</param>
        /// <param name="filter">The SCIM filter AST</param>
        /// <returns>Filtered queryable</returns>
        IQueryable<TUser> Apply(IQueryable<TUser> source, FilterExpression? filter);
    }
}

