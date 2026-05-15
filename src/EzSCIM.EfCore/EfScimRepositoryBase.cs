using EzSCIM.DataRepositories;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.EfCore;

/// <summary>
/// Generic Entity Framework Core base repository for SCIM user and group management.
/// Inherit this class and expose your DbSets via <see cref="Users"/> and <see cref="Groups"/>
/// to get a fully functional <see cref="IUserGroupDataRepository{TUser,TGroup}"/> with zero boilerplate.
/// </summary>
/// <typeparam name="TUser">EF entity class for users. Must implement <see cref="IScimEntity"/>.</typeparam>
/// <typeparam name="TGroup">EF entity class for groups. Must implement <see cref="IScimEntity"/>.</typeparam>
/// <typeparam name="TContext">Your <see cref="DbContext"/> subclass.</typeparam>
/// <example>
/// <code>
/// public class MyScimRepository : EfScimRepositoryBase&lt;AppUser, AppGroup, AppDbContext&gt;
/// {
///     public MyScimRepository(AppDbContext ctx) : base(ctx) { }
///     protected override DbSet&lt;AppUser&gt;  Users  =&gt; Context.Users;
///     protected override DbSet&lt;AppGroup&gt; Groups =&gt; Context.Groups;
/// }
/// </code>
/// </example>
public abstract class EfScimRepositoryBase<TUser, TGroup, TContext>
    : IUserGroupDataRepository<TUser, TGroup>
    where TUser    : class, IScimEntity
    where TGroup   : class, IScimEntity
    where TContext : DbContext
{
    /// <summary>The underlying DbContext instance.</summary>
    protected readonly TContext Context;

    protected EfScimRepositoryBase(TContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Override to return the DbSet that maps to your user table.
    /// Example: <c>protected override DbSet&lt;AppUser&gt; Users =&gt; Context.Users;</c>
    /// </summary>
    protected abstract DbSet<TUser> Users { get; }

    /// <summary>
    /// Override to return the DbSet that maps to your group table.
    /// Example: <c>protected override DbSet&lt;AppGroup&gt; Groups =&gt; Context.Groups;</c>
    /// </summary>
    protected abstract DbSet<TGroup> Groups { get; }

    // -------------------------------------------------------------------------
    // User Operations
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<TUser?> GetUserAsync(string id)
        => await Users.FindAsync(id);

    /// <inheritdoc />
    public IQueryable<TUser> QueryUsers()
        => Users.AsQueryable();

    /// <inheritdoc />
    public async Task<TUser> CreateUserAsync(TUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            user.Id = Guid.NewGuid().ToString();

        user.CreatedAt  = DateTime.UtcNow;
        user.ModifiedAt = DateTime.UtcNow;

        Users.Add(user);

        try
        {
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (IsUniqueConstraintViolation(ex))
        {
            throw new InvalidOperationException(
                $"A user with the same unique attribute already exists.", ex);
        }

        return user;
    }

    /// <summary>
    /// Returns true when the <see cref="Microsoft.EntityFrameworkCore.DbUpdateException"/>
    /// is caused by a unique/primary-key constraint violation (SQL Server, SQLite, PostgreSQL).
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // SQL Server: 2601 (unique index), 2627 (unique constraint / PK)
        // SQLite: error code 19 (SQLITE_CONSTRAINT), message contains "UNIQUE"
        // PostgreSQL (Npgsql): SqlState "23505"
        var inner = ex.InnerException;
        if (inner is null) return false;

        var msg = inner.Message;
        if (msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("unique constraint", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // PostgreSQL (Npgsql): SqlState "23505" — checked via reflection to avoid hard dependency
        var sqlStateProp = inner.GetType().GetProperty("SqlState");
        if (sqlStateProp != null && sqlStateProp.GetValue(inner) is string sqlState && sqlState == "23505")
            return true;

        return false;
    }

    /// <inheritdoc />
    public async Task<TUser?> UpdateUserAsync(string id, TUser user)
    {
        var existing = await Users.FindAsync(id);
        if (existing is null)
            return null;

        user.Id         = id;
        user.CreatedAt  = existing.CreatedAt;
        user.ModifiedAt = DateTime.UtcNow;

        await OnBeforeUpdateUserAsync(existing, user);

        await Context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await Users.FindAsync(id);
        if (user is null)
            return false;

        Users.Remove(user);
        await Context.SaveChangesAsync();
        return true;
    }

    // -------------------------------------------------------------------------
    // Group Operations
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<TGroup?> GetGroupAsync(string id)
        => await Groups.FindAsync(id);

    /// <inheritdoc />
    public IQueryable<TGroup> QueryGroups()
        => Groups.AsQueryable();

    /// <inheritdoc />
    public async Task<TGroup> CreateGroupAsync(TGroup group)
    {
        if (string.IsNullOrEmpty(group.Id))
            group.Id = Guid.NewGuid().ToString();

        group.CreatedAt  = DateTime.UtcNow;
        group.ModifiedAt = DateTime.UtcNow;

        Groups.Add(group);
        await Context.SaveChangesAsync();
        return group;
    }

    /// <inheritdoc />
    public async Task<TGroup?> UpdateGroupAsync(string id, TGroup group)
    {
        var existing = await Groups.FindAsync(id);
        if (existing is null)
            return null;

        group.Id         = id;
        group.CreatedAt  = existing.CreatedAt;
        group.ModifiedAt = DateTime.UtcNow;

        await OnBeforeUpdateGroupAsync(existing, group);

        await Context.SaveChangesAsync();
        return group;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteGroupAsync(string id)
    {
        var group = await Groups.FindAsync(id);
        if (group is null)
            return false;

        Groups.Remove(group);
        await Context.SaveChangesAsync();
        return true;
    }

    // -------------------------------------------------------------------------
    // Extension hooks
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called during <see cref="UpdateUserAsync"/> before SaveChanges.
    /// Default behaviour: copies all scalar columns via <c>CurrentValues.SetValues</c>.
    /// Override this method when your entity has EF navigation properties that
    /// must be handled manually (e.g. related collections).
    /// </summary>
    /// <param name="existing">The tracked entity from the database.</param>
    /// <param name="updated">The incoming entity with new values.</param>
    protected virtual Task OnBeforeUpdateUserAsync(TUser existing, TUser updated)
    {
        Context.Entry(existing).CurrentValues.SetValues(updated);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called during <see cref="UpdateGroupAsync"/> before SaveChanges.
    /// Default behaviour: copies all scalar columns via <c>CurrentValues.SetValues</c>.
    /// Override this method when your entity has EF navigation properties that
    /// must be handled manually (e.g. member collections).
    /// </summary>
    /// <param name="existing">The tracked entity from the database.</param>
    /// <param name="updated">The incoming entity with new values.</param>
    protected virtual Task OnBeforeUpdateGroupAsync(TGroup existing, TGroup updated)
    {
        Context.Entry(existing).CurrentValues.SetValues(updated);
        return Task.CompletedTask;
    }
}

