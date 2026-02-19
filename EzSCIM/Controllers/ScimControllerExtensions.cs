using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    }

    /// <summary>
    /// Convention to apply custom route prefix to SCIM controllers
    /// </summary>
    internal class ScimRouteConvention : IApplicationModelConvention
    {
        private readonly ScimControllerOptions _options;

        public ScimRouteConvention(ScimControllerOptions options)
        {
            _options = options;
        }

        public void Apply(ApplicationModel application)
        {
            for (int i = application.Controllers.Count - 1; i >= 0; i--)
            {
                var controller = application.Controllers[i];
                
                // Only apply to SCIM controllers
                if (controller.ControllerType.Namespace != "EzSCIM.Controllers")
                    continue;

                // Check if we should exclude this controller
                if (!_options.IncludeGroups && controller.ControllerName == "ScimGroups")
                {
                    application.Controllers.RemoveAt(i);
                    continue;
                }

                if (!_options.IncludeServiceProviderConfig && controller.ControllerName == "ScimConfig")
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
                                template.Replace("scim/", $"{_options.RoutePrefix}/");
                        }
                        else if (template == "scim")
                        {
                            selector.AttributeRouteModel.Template = _options.RoutePrefix;
                        }
                    }
                }
            }
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
            // Configure options
            var options = new ScimControllerOptions();
            configure(options);
            services.Configure(configure);

            // Add controllers with custom convention
            services.AddControllers(mvcOptions =>
            {
                mvcOptions.Conventions.Add(new ScimRouteConvention(options));
            })
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
            // Configure options
            var options = new ScimControllerOptions();
            configure(options);
            builder.Services.Configure(configure);

            // Add convention
            builder.Services.Configure<MvcOptions>(mvcOptions =>
            {
                mvcOptions.Conventions.Add(new ScimRouteConvention(options));
            });

            return builder
                .AddApplicationPart(typeof(ScimUsersController).Assembly)
                .AddJsonOptions(jsonOptions =>
                {
                    jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });
        }
    }
}


