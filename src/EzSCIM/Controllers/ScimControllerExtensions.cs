using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EzSCIM.Services;

namespace EzSCIM.Controllers
{
    /// <summary>
    /// Configuration options for SCIM controllers
    /// </summary>
    public class ScimControllerOptions
    {
        /// <summary>
        /// The route prefix for SCIM endpoints. Default is "scim".
        /// Example: "api/scim" will result in routes like "api/scim/Users"
        /// </summary>
        public string RoutePrefix { get; set; } = "scim";

        /// <summary>
        /// Whether to include Group controllers. Default is true.
        /// Set to false if you only need User management.
        /// </summary>
        public bool IncludeGroups { get; set; } = true;

        /// <summary>
        /// Whether to include ServiceProviderConfig endpoint. Default is true.
        /// </summary>
        public bool IncludeServiceProviderConfig { get; set; } = true;

        /// <summary>
        /// Whether to include the token generation endpoint. Default is false.
        /// </summary>
        public bool IncludeTokenEndpoint { get; set; } = false;
    }

    /// <summary>
    /// Convention to apply custom route prefix to SCIM controllers
    /// </summary>
    internal class ScimRouteConvention : IApplicationModelConvention
    {
        private readonly IOptionsMonitor<ScimControllerOptions> _optionsMonitor;

        public ScimRouteConvention(IOptionsMonitor<ScimControllerOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public void Apply(ApplicationModel application)
        {
            var options = _optionsMonitor.CurrentValue;

            for (int i = application.Controllers.Count - 1; i >= 0; i--)
            {
                var controller = application.Controllers[i];

                // Only apply to SCIM controllers
                if (controller.ControllerType.Namespace != "EzSCIM.Controllers")
                    continue;

                // Check if we should exclude this controller
                if (!options.IncludeGroups && controller.ControllerName == "ScimGroups")
                {
                    application.Controllers.RemoveAt(i);
                    continue;
                }

                if (!options.IncludeServiceProviderConfig && controller.ControllerName == "ScimConfig")
                {
                    application.Controllers.RemoveAt(i);
                    continue;
                }

                if (!options.IncludeTokenEndpoint && controller.ControllerName == "ScimToken")
                {
                    application.Controllers.RemoveAt(i);
                    continue;
                }

                // Replace route prefix
                foreach (var selector in controller.Selectors)
                {
                    if (selector.AttributeRouteModel?.Template != null)
                    {
                        var template = selector.AttributeRouteModel.Template;
                        if (template.StartsWith("scim/"))
                        {
                            selector.AttributeRouteModel.Template = 
                                template.Replace("scim/", $"{options.RoutePrefix}/");
                        }
                        else if (template == "scim")
                        {
                            selector.AttributeRouteModel.Template = options.RoutePrefix;
                        }
                    }
                }
            }
        }
    }

    internal sealed class ScimMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly IOptionsMonitor<ScimControllerOptions> _optionsMonitor;

        public ScimMvcOptionsSetup(IOptionsMonitor<ScimControllerOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public void Configure(MvcOptions options)
        {
            options.Conventions.Add(new ScimRouteConvention(_optionsMonitor));
        }
    }

    /// <summary>
    /// Extension methods for configuring SCIM controllers
    /// </summary>
    public static class ScimControllerExtensions
    {
        /// <summary>
        /// Adds SCIM controllers with default configuration (scim/Users, scim/Groups)
        /// </summary>
        public static IServiceCollection AddScimControllers(this IServiceCollection services)
        {
            return services.AddScimControllers(options => { });
        }

        /// <summary>
        /// Adds SCIM controllers with custom configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configure">Action to configure SCIM options</param>
        /// <example>
        /// <code>
        /// // Custom route prefix
        /// services.AddScimControllers(options => 
        /// {
        ///     options.RoutePrefix = "api/v1/scim";
        /// });
        /// 
        /// // Users only, no Groups
        /// services.AddScimControllers(options => 
        /// {
        ///     options.IncludeGroups = false;
        /// });
        /// </code>
        /// </example>
        public static IServiceCollection AddScimControllers(
            this IServiceCollection services, 
            Action<ScimControllerOptions> configure)
        {
            services.Configure(configure);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, ScimMvcOptionsSetup>());

            services.AddControllers()
                .AddApplicationPart(typeof(ScimUsersController).Assembly)
                .AddJsonOptions(jsonOptions =>
                {
                    jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            return services;
        }

        /// <summary>
        /// Adds SCIM controllers to an existing MvcBuilder with default configuration
        /// </summary>
        public static IMvcBuilder AddScimControllers(this IMvcBuilder builder)
        {
            return builder.AddScimControllers(options => { });
        }

        /// <summary>
        /// Adds SCIM controllers to an existing MvcBuilder with custom configuration
        /// </summary>
        /// <param name="builder">The MvcBuilder</param>
        /// <param name="configure">Action to configure SCIM options</param>
        /// <example>
        /// <code>
        /// services.AddControllers()
        ///     .AddScimControllers(options => 
        ///     {
        ///         options.RoutePrefix = "customroute";
        ///         options.IncludeGroups = false;
        ///     });
        /// </code>
        /// </example>
        public static IMvcBuilder AddScimControllers(
            this IMvcBuilder builder, 
            Action<ScimControllerOptions> configure)
        {
            builder.Services.Configure(configure);
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, ScimMvcOptionsSetup>());

            return builder
                .AddApplicationPart(typeof(ScimUsersController).Assembly)
                .AddJsonOptions(jsonOptions =>
                {
                    jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });
        }

        /// <summary>
        /// Enables the token generation endpoint for SCIM API testing.
        /// </summary>
        public static IServiceCollection AddScimTokenGeneratorEndpoint(this IServiceCollection services)
        {
            return services.AddScimTokenGeneratorEndpoint(true);
        }

        /// <summary>
        /// Enables or disables the token generation endpoint for SCIM API testing.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="enabled">Whether the token endpoint is enabled.</param>
        public static IServiceCollection AddScimTokenGeneratorEndpoint(this IServiceCollection services, bool enabled)
        {
            services.AddSingleton<ITokenEndpointFeature>(new TokenEndpointFeature(enabled));

            services.PostConfigure<ScimControllerOptions>(options =>
            {
                options.IncludeTokenEndpoint = enabled;
            });

            return services;
        }

        /// <summary>
        /// Enables the token generation endpoint for SCIM API testing.
        /// </summary>
        public static IMvcBuilder AddScimTokenGeneratorEndpoint(this IMvcBuilder builder)
        {
            return builder.AddScimTokenGeneratorEndpoint(true);
        }

        /// <summary>
        /// Enables or disables the token generation endpoint for SCIM API testing.
        /// </summary>
        /// <param name="builder">The MVC builder.</param>
        /// <param name="enabled">Whether the token endpoint is enabled.</param>
        public static IMvcBuilder AddScimTokenGeneratorEndpoint(this IMvcBuilder builder, bool enabled)
        {
            builder.Services.AddSingleton<ITokenEndpointFeature>(new TokenEndpointFeature(enabled));

            builder.Services.PostConfigure<ScimControllerOptions>(options =>
            {
                options.IncludeTokenEndpoint = enabled;
            });

            return builder;
        }
    }
}
