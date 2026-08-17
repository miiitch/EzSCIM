using EzSCIM.Repositories;

namespace EzSCIM.Observability
{
    /// <summary>
    /// The SCIM resource a <see cref="ScimOperationContext"/> refers to.
    /// </summary>
    public enum ScimResourceKind
    {
        User,
        Group
    }

    /// <summary>
    /// The kind of operation performed against an <see cref="IScimRepository"/>-implementing
    /// repository, as observed by <see cref="IScimOperationCallbacks"/>.
    /// </summary>
    public enum ScimOperationKind
    {
        Read,
        Search,
        Create,
        Update,
        Patch,
        Delete
    }

    /// <summary>
    /// Describes a single SCIM repository operation, passed to <see cref="IScimOperationCallbacks"/>
    /// after the operation completes (successfully or with an error).
    /// </summary>
    public sealed class ScimOperationContext
    {
        /// <summary>
        /// The resource type the operation was performed on.
        /// </summary>
        public required ScimResourceKind ResourceKind { get; init; }

        /// <summary>
        /// The kind of operation performed.
        /// </summary>
        public required ScimOperationKind OperationKind { get; init; }

        /// <summary>
        /// The identifier of the resource involved, when known.
        /// Null for <see cref="ScimOperationKind.Search"/> and for operations where the resource
        /// could not be resolved (e.g. a failed <see cref="ScimOperationKind.Create"/>).
        /// </summary>
        public string? ResourceId { get; init; }

        /// <summary>
        /// The result returned by the repository on success (the resource, a list response, or a
        /// boolean for deletes). Null when the operation failed.
        /// </summary>
        public object? Result { get; init; }

        /// <summary>
        /// How long the underlying repository call took.
        /// </summary>
        public required TimeSpan Duration { get; init; }

        /// <summary>
        /// When the operation completed.
        /// </summary>
        public required DateTimeOffset Timestamp { get; init; }
    }
}
