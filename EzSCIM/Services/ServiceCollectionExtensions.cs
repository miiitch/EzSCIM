using Microsoft.Extensions.DependencyInjection;

namespace EzSCIM.Services
{
    /// <summary>
    /// Extension methods for registering SCIM authentication services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds JWT token service to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        /// <example>
        /// <code>
        /// builder.Services.AddJwtTokenService();
        /// </code>
        /// </example>
        public static IServiceCollection AddJwtTokenService(this IServiceCollection services)
        {
            return services.AddSingleton<IJwtTokenService, JwtTokenService>();
        }

        /// <summary>
        /// Adds JWT token service with custom lifetime
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="lifetime">The service lifetime (Singleton, Scoped, or Transient)</param>
        /// <returns>The service collection for chaining</returns>
        /// <example>
        /// <code>
        /// builder.Services.AddJwtTokenService(ServiceLifetime.Scoped);
        /// </code>
        /// </example>
        public static IServiceCollection AddJwtTokenService(this IServiceCollection services, ServiceLifetime lifetime)
        {
            services.Add(new ServiceDescriptor(typeof(IJwtTokenService), typeof(JwtTokenService), lifetime));
            return services;
        }
    }
}

