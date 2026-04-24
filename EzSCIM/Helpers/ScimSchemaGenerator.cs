using EzSCIM.Attributes;
using EzSCIM.Models;
using System.Reflection;

namespace EzSCIM.Helpers
{
    /// <summary>
    /// Static helper class that generates SCIM schemas from classes annotated with ScimResource and ScimProperty attributes.
    /// Schemas are computed once in a thread-safe static constructor.
    /// </summary>
    public static class ScimSchemaGenerator
    {
        private static readonly ILogger _logger;

        /// <summary>
        /// Pre-generated schema for ScimUser.
        /// </summary>
        public static ScimSchema UserSchema { get; }

        /// <summary>
        /// Pre-generated schema for ScimGroup.
        /// </summary>
        public static ScimSchema GroupSchema { get; }

        /// <summary>
        /// Static constructor - executes once and is thread-safe.
        /// Generates schemas for User and Group resources.
        /// </summary>
        static ScimSchemaGenerator()
        {
            // Create a logger factory for static context
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });
            _logger = loggerFactory.CreateLogger(nameof(ScimSchemaGenerator));

            UserSchema = GenerateSchema<ScimUser>();
            GroupSchema = GenerateSchema<ScimGroup>();

            // Log initialization
            Console.WriteLine($"[ScimSchemaGenerator] User schema initialized: {UserSchema.Attributes.Count} attributes");
            Console.WriteLine($"[ScimSchemaGenerator] Group schema initialized: {GroupSchema.Attributes.Count} attributes");
        }

        /// <summary>
        /// Generates a SCIM schema for a custom resource type.
        /// </summary>
        /// <typeparam name="T">The resource type annotated with ScimResource attribute</typeparam>
        /// <returns>The generated SCIM schema</returns>
        public static ScimSchema GetSchema<T>()
        {
            return GenerateSchema<T>();
        }

        /// <summary>
        /// Generates a SCIM schema from a type annotated with ScimResource and ScimProperty attributes.
        /// </summary>
        private static ScimSchema GenerateSchema<T>()
        {
            var type = typeof(T);
            var resourceAttr = type.GetCustomAttribute<ScimResourceAttribute>(inherit: true);

            if (resourceAttr == null)
            {
                Console.WriteLine($"[ScimSchemaGenerator] WARNING: Type {type.Name} does not have [ScimResource] attribute. Returning empty schema.");
                return new ScimSchema
                {
                    Id = $"urn:unknown:{type.Name}",
                    Name = type.Name,
                    Description = $"No schema defined for {type.Name}",
                    Attributes = new List<ScimSchemaAttribute>()
                };
            }

            var schema = new ScimSchema
            {
                Id = resourceAttr.Schema,
                Name = resourceAttr.Name,
                Description = resourceAttr.Description,
                Attributes = new List<ScimSchemaAttribute>()
            };

            // Get all properties including inherited ones
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            foreach (var property in properties)
            {
                var scimProp = property.GetCustomAttribute<ScimPropertyAttribute>(inherit: true);
                if (scimProp == null)
                    continue; // Opt-in: only include properties with [ScimProperty]

                var schemaAttr = new ScimSchemaAttribute
                {
                    Name = scimProp.Name,
                    Type = scimProp.Type,
                    Required = scimProp.Required,
                    MultiValued = scimProp.MultiValued,
                    Description = scimProp.Description,
                    Uniqueness = scimProp.Uniqueness,
                    Mutability = scimProp.Mutability,
                    Returned = scimProp.Returned,
                    CaseExact = scimProp.CaseExact
                };

                // If the type is complex, recursively discover sub-attributes
                if (scimProp.Type.Equals("complex", StringComparison.OrdinalIgnoreCase))
                {
                    var propertyType = property.PropertyType;

                    // Handle multi-valued complex types (List<T>)
                    if (scimProp.MultiValued && propertyType.IsGenericType && 
                        propertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        propertyType = propertyType.GetGenericArguments()[0];
                    }

                    schemaAttr.SubAttributes = GenerateSubAttributes(propertyType);
                }

                schema.Attributes.Add(schemaAttr);
            }

            return schema;
        }

        /// <summary>
        /// Generates sub-attributes for a complex type.
        /// </summary>
        private static List<ScimSchemaAttribute> GenerateSubAttributes(Type complexType)
        {
            var subAttributes = new List<ScimSchemaAttribute>();
            var properties = complexType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            foreach (var property in properties)
            {
                var scimProp = property.GetCustomAttribute<ScimPropertyAttribute>(inherit: true);
                if (scimProp == null)
                    continue; // Opt-in: only include properties with [ScimProperty]

                var subAttr = new ScimSchemaAttribute
                {
                    Name = scimProp.Name,
                    Type = scimProp.Type,
                    Required = scimProp.Required,
                    MultiValued = scimProp.MultiValued,
                    Description = scimProp.Description,
                    Uniqueness = scimProp.Uniqueness,
                    Mutability = scimProp.Mutability,
                    Returned = scimProp.Returned,
                    CaseExact = scimProp.CaseExact
                };

                // Handle nested complex types (recursive)
                if (scimProp.Type.Equals("complex", StringComparison.OrdinalIgnoreCase))
                {
                    var propertyType = property.PropertyType;
                    if (scimProp.MultiValued && propertyType.IsGenericType && 
                        propertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        propertyType = propertyType.GetGenericArguments()[0];
                    }
                    subAttr.SubAttributes = GenerateSubAttributes(propertyType);
                }

                subAttributes.Add(subAttr);
            }

            return subAttributes;
        }
    }
}
