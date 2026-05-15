namespace EzSCIM.EfCore;

/// <summary>
/// Marker interface required by <see cref="EfScimRepositoryBase{TUser,TGroup,TContext}"/>.
/// Implement this interface on your EF entity classes to enable automatic
/// Id generation and timestamp management (CreatedAt, ModifiedAt).
/// </summary>
public interface IScimEntity
{
    /// <summary>
    /// Unique identifier. Auto-generated (GUID) if empty on creation.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// UTC timestamp set once when the entity is first created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp updated on every write operation.
    /// </summary>
    DateTime ModifiedAt { get; set; }
}

