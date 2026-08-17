using Microsoft.Extensions.DependencyInjection;
using EzSCIM.Repositories;

namespace EzSCIM.Observability
{
    /// <summary>
    /// Extension methods for registering optional <see cref="IScimOperationCallbacks"/> subscribers.
    /// </summary>
    /// <remarks>
    /// Calling <c>AddScimOperationCallback</c> is entirely opt-in. Hosts that never call it get the
    /// exact same <see cref="IScimRepository"/> instance they registered themselves — no decorator,
    /// no extra allocation, no behavior change.
    /// </remarks>
    public static class ScimObservabilityServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <typeparamref name="TCallback"/> as a SCIM operation observer and, on first use,
        /// wraps the already-registered <see cref="IScimRepository"/> so it notifies every registered
        /// callback. Must be called after <see cref="IScimRepository"/> has been registered.
        /// Safe to call multiple times to register several independent callback subscribers.
        /// </summary>
        /// <example>
        /// <code>
        /// builder.Services.AddScoped&lt;IScimRepository, MyScimRepository&gt;();
        /// builder.Services.AddScimOperationCallback&lt;MyMonitoringCallbacks&gt;();
        /// </code>
        /// </example>
        public static IServiceCollection AddScimOperationCallback<TCallback>(this IServiceCollection services)
            where TCallback : class, IScimOperationCallbacks
        {
            services.AddSingleton<IScimOperationCallbacks, TCallback>();
            DecorateScimRepositoryIfNeeded(services);
            return services;
        }

        /// <summary>
        /// Registers a pre-built <see cref="IScimOperationCallbacks"/> instance as a SCIM operation
        /// observer. See <see cref="AddScimOperationCallback{TCallback}"/> for details.
        /// </summary>
        public static IServiceCollection AddScimOperationCallback(this IServiceCollection services, IScimOperationCallbacks callbacks)
        {
            services.AddSingleton(callbacks);
            DecorateScimRepositoryIfNeeded(services);
            return services;
        }

        private static void DecorateScimRepositoryIfNeeded(IServiceCollection services)
        {
            if (services.Any(d => d.ServiceType == typeof(ScimRepositoryDecorationMarker)))
                return;

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IScimRepository));
            if (descriptor is null)
            {
                throw new InvalidOperationException(
                    $"{nameof(AddScimOperationCallback)} must be called after registering {nameof(IScimRepository)} in the service collection.");
            }

            services.Remove(descriptor);
            services.Add(new ServiceDescriptor(
                typeof(IScimRepository),
                sp =>
                {
                    var inner = (IScimRepository)CreateInstance(descriptor, sp);
                    return ActivatorUtilities.CreateInstance<ObservableScimRepository>(sp, inner);
                },
                descriptor.Lifetime));

            services.AddSingleton<ScimRepositoryDecorationMarker>();
        }

        private static object CreateInstance(ServiceDescriptor descriptor, IServiceProvider serviceProvider)
        {
            if (descriptor.ImplementationInstance is not null)
                return descriptor.ImplementationInstance;
            if (descriptor.ImplementationFactory is not null)
                return descriptor.ImplementationFactory(serviceProvider);
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, descriptor.ImplementationType!);
        }

        private sealed class ScimRepositoryDecorationMarker;
    }
}
