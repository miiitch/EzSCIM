namespace EzSCIM.Observability
{
    /// <summary>
    /// Optional callbacks a host application can register to observe SCIM repository operations
    /// (last read, last update, etc.) and capture errors for its own monitoring/logging.
    /// </summary>
    /// <remarks>
    /// This is purely additive observability. It does not replace the standard <c>ILogger</c> logging
    /// already performed inside EzSCIM's controllers — both mechanisms run independently.
    /// Registering an implementation is entirely optional: see
    /// <c>ScimObservabilityServiceCollectionExtensions.AddScimOperationCallback</c>.
    /// </remarks>
    public interface IScimOperationCallbacks
    {
        /// <summary>
        /// Invoked after a SCIM repository operation completes successfully.
        /// </summary>
        Task OnOperationCompletedAsync(ScimOperationContext context);

        /// <summary>
        /// Invoked when a SCIM repository operation throws. The exception is provided for the host
        /// application to relay into its own logging mechanism; it is always rethrown unchanged by
        /// the caller after every registered callback has observed it.
        /// </summary>
        Task OnErrorAsync(ScimOperationContext context, Exception exception);
    }

    /// <summary>
    /// Convenience base class for <see cref="IScimOperationCallbacks"/> implementations that only
    /// care about a subset of events.
    /// </summary>
    public abstract class ScimOperationCallbacksBase : IScimOperationCallbacks
    {
        public virtual Task OnOperationCompletedAsync(ScimOperationContext context) => Task.CompletedTask;

        public virtual Task OnErrorAsync(ScimOperationContext context, Exception exception) => Task.CompletedTask;
    }
}
