using System.Diagnostics;
using Microsoft.Extensions.Logging;
using EzSCIM.Filtering.AST;
using EzSCIM.Models;
using EzSCIM.Observability;

namespace EzSCIM.Repositories
{
    /// <summary>
    /// Decorates an <see cref="IScimRepository"/> implementation to notify registered
    /// <see cref="IScimOperationCallbacks"/> after every operation, without altering the wrapped
    /// repository's results or exceptions in any way.
    /// </summary>
    /// <remarks>
    /// Only instantiated when at least one <see cref="IScimOperationCallbacks"/> is registered via
    /// <c>ScimObservabilityServiceCollectionExtensions.AddScimOperationCallback</c> — hosts that don't
    /// use the feature never see this type.
    /// </remarks>
    internal sealed class ObservableScimRepository(
        IScimRepository inner,
        IEnumerable<IScimOperationCallbacks> callbacks,
        ILogger<ObservableScimRepository> logger) : IScimRepository
    {
        public async Task<ScimUser?> GetUserAsync(string id) =>
            await ExecuteAsync(ScimResourceKind.User, ScimOperationKind.Read, id, () => inner.GetUserAsync(id));

        public async Task<ScimUser?> GetUserByUserNameAsync(string userName) =>
            await ExecuteAsync(ScimResourceKind.User, ScimOperationKind.Read, null, () => inner.GetUserByUserNameAsync(userName));

        public async Task<ScimListResponse<ScimUser>> GetUsersAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100) =>
            await ExecuteAsync(ScimResourceKind.User, ScimOperationKind.Search, null, () => inner.GetUsersAsync(filter, startIndex, count));

        public async Task<ScimUser> CreateUserAsync(ScimUser user) =>
            await ExecuteAsync(ScimResourceKind.User, ScimOperationKind.Create, null, () => inner.CreateUserAsync(user));

        public async Task<ScimUser?> UpdateUserAsync(string id, ScimUser user) =>
            await ExecuteAsync(ScimResourceKind.User, ScimOperationKind.Update, id, () => inner.UpdateUserAsync(id, user));

        public async Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest) =>
            await ExecuteAsync(ScimResourceKind.User, ScimOperationKind.Patch, id, () => inner.PatchUserAsync(id, patchRequest));

        public async Task<bool> DeleteUserAsync(string id) =>
            await ExecuteAsync(ScimResourceKind.User, ScimOperationKind.Delete, id, () => inner.DeleteUserAsync(id));

        public async Task<ScimGroup?> GetGroupAsync(string id) =>
            await ExecuteAsync(ScimResourceKind.Group, ScimOperationKind.Read, id, () => inner.GetGroupAsync(id));

        public async Task<ScimGroup?> GetGroupByDisplayNameAsync(string displayName) =>
            await ExecuteAsync(ScimResourceKind.Group, ScimOperationKind.Read, null, () => inner.GetGroupByDisplayNameAsync(displayName));

        public async Task<ScimListResponse<ScimGroup>> GetGroupsAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100) =>
            await ExecuteAsync(ScimResourceKind.Group, ScimOperationKind.Search, null, () => inner.GetGroupsAsync(filter, startIndex, count));

        public async Task<ScimGroup> CreateGroupAsync(ScimGroup group) =>
            await ExecuteAsync(ScimResourceKind.Group, ScimOperationKind.Create, null, () => inner.CreateGroupAsync(group));

        public async Task<ScimGroup?> UpdateGroupAsync(string id, ScimGroup group) =>
            await ExecuteAsync(ScimResourceKind.Group, ScimOperationKind.Update, id, () => inner.UpdateGroupAsync(id, group));

        public async Task<ScimGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest) =>
            await ExecuteAsync(ScimResourceKind.Group, ScimOperationKind.Patch, id, () => inner.PatchGroupAsync(id, patchRequest));

        public async Task<bool> DeleteGroupAsync(string id) =>
            await ExecuteAsync(ScimResourceKind.Group, ScimOperationKind.Delete, id, () => inner.DeleteGroupAsync(id));

        private async Task<T> ExecuteAsync<T>(
            ScimResourceKind resourceKind,
            ScimOperationKind operationKind,
            string? resourceId,
            Func<Task<T>> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await operation();
                stopwatch.Stop();
                await NotifyCompletedAsync(new ScimOperationContext
                {
                    ResourceKind = resourceKind,
                    OperationKind = operationKind,
                    ResourceId = resourceId,
                    Result = result,
                    Duration = stopwatch.Elapsed,
                    Timestamp = DateTimeOffset.UtcNow
                });
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await NotifyErrorAsync(new ScimOperationContext
                {
                    ResourceKind = resourceKind,
                    OperationKind = operationKind,
                    ResourceId = resourceId,
                    Result = null,
                    Duration = stopwatch.Elapsed,
                    Timestamp = DateTimeOffset.UtcNow
                }, ex);
                throw;
            }
        }

        private async Task NotifyCompletedAsync(ScimOperationContext context)
        {
            foreach (var callback in callbacks)
            {
                try
                {
                    await callback.OnOperationCompletedAsync(context);
                }
                catch (Exception callbackException)
                {
                    logger.LogError(callbackException,
                        "IScimOperationCallbacks.OnOperationCompletedAsync threw for {ResourceKind} {OperationKind}",
                        context.ResourceKind, context.OperationKind);
                }
            }
        }

        private async Task NotifyErrorAsync(ScimOperationContext context, Exception exception)
        {
            foreach (var callback in callbacks)
            {
                try
                {
                    await callback.OnErrorAsync(context, exception);
                }
                catch (Exception callbackException)
                {
                    logger.LogError(callbackException,
                        "IScimOperationCallbacks.OnErrorAsync threw for {ResourceKind} {OperationKind}",
                        context.ResourceKind, context.OperationKind);
                }
            }
        }
    }
}
