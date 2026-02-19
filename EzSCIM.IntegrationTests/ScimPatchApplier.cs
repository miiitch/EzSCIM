using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using EzSCIM.Attributes;
using EzSCIM.Models;

namespace EzSCIM.IntegrationTests;

/// <summary>
/// Generic SCIM PATCH operation applier using reflection and ScimProperty attributes.
/// Uses a static cache for thread-safe property mapping.
/// </summary>
public static class ScimPatchApplier
{
    /// <summary>
    /// Thread-safe cache of property mappings per entity type.
    /// Key: Entity type, Value: Dictionary of SCIM path -> PropertyInfo
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyMapping>> _propertyCache;

    /// <summary>
    /// Static constructor - initializes the cache (thread-safe).
    /// </summary>
    static ScimPatchApplier()
    {
        _propertyCache = new ConcurrentDictionary<Type, Dictionary<string, PropertyMapping>>();
    }

    /// <summary>
    /// Applies PATCH operations to an entity using ScimProperty attribute mappings.
    /// </summary>
    /// <typeparam name="TEntity">Entity type with ScimProperty attributes</typeparam>
    /// <param name="entity">The entity to patch</param>
    /// <param name="operations">SCIM PATCH operations</param>
    /// <returns>True if any property was modified</returns>
    public static bool ApplyPatch<TEntity>(TEntity entity, IEnumerable<ScimPatchOperation> operations) where TEntity : class
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (operations == null) return false;

        var mappings = GetPropertyMappings<TEntity>();
        bool modified = false;

        foreach (var op in operations)
        {
            if (ApplyOperation(entity, op, mappings))
            {
                modified = true;
            }
        }

        return modified;
    }

    /// <summary>
    /// Gets or creates the property mappings for an entity type.
    /// </summary>
    private static Dictionary<string, PropertyMapping> GetPropertyMappings<TEntity>() where TEntity : class
    {
        return _propertyCache.GetOrAdd(typeof(TEntity), type =>
        {
            var mappings = new Dictionary<string, PropertyMapping>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var scimAttr = property.GetCustomAttribute<ScimPropertyAttribute>();
                if (scimAttr != null)
                {
                    var mapping = new PropertyMapping
                    {
                        Property = property,
                        ScimName = scimAttr.Name,
                        ScimType = scimAttr.Type,
                        IsRequired = scimAttr.Required
                    };

                    // Add mapping by SCIM name
                    mappings[scimAttr.Name] = mapping;

                    // Also add simplified mappings for common paths
                    // e.g., "name.givenName" also accessible as "name.givenname"
                    var lowerName = scimAttr.Name.ToLowerInvariant();
                    if (!mappings.ContainsKey(lowerName))
                    {
                        mappings[lowerName] = mapping;
                    }

                    // Add property name as fallback
                    if (!mappings.ContainsKey(property.Name))
                    {
                        mappings[property.Name] = mapping;
                    }
                }
            }

            return mappings;
        });
    }

    /// <summary>
    /// Applies a single PATCH operation to an entity.
    /// </summary>
    private static bool ApplyOperation<TEntity>(TEntity entity, ScimPatchOperation op, Dictionary<string, PropertyMapping> mappings) where TEntity : class
    {
        if (string.IsNullOrEmpty(op.Path) && op.Op?.ToLowerInvariant() == "replace" && op.Value != null)
        {
            // RFC 7644: If path is omitted, value must be a complex object with attribute names
            return ApplyBulkReplace(entity, op.Value, mappings);
        }

        var path = op.Path ?? "";
        var operation = op.Op?.ToLowerInvariant() ?? "replace";

        // Special handling for "members" path (array operations)
        if (path.Equals("members", StringComparison.OrdinalIgnoreCase))
        {
            return false; // Handled separately by CompositeScimRepository
        }

        // Find the property mapping
        if (!mappings.TryGetValue(path, out var mapping))
        {
            // Try with normalized path (remove brackets, lowercase)
            var normalizedPath = NormalizePath(path);
            if (!mappings.TryGetValue(normalizedPath, out mapping))
            {
                // Property not found - skip silently or could throw
                return false;
            }
        }

        switch (operation)
        {
            case "add":
            case "replace":
                SetPropertyValue(entity, mapping, op.Value);
                return true;

            case "remove":
                SetPropertyToDefault(entity, mapping);
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Applies bulk replace when path is omitted.
    /// </summary>
    private static bool ApplyBulkReplace<TEntity>(TEntity entity, object value, Dictionary<string, PropertyMapping> mappings) where TEntity : class
    {
        if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
        {
            bool modified = false;
            foreach (var prop in jsonElement.EnumerateObject())
            {
                if (mappings.TryGetValue(prop.Name, out var mapping))
                {
                    SetPropertyValue(entity, mapping, prop.Value);
                    modified = true;
                }
            }
            return modified;
        }
        return false;
    }

    /// <summary>
    /// Sets a property value handling type conversion.
    /// </summary>
    private static void SetPropertyValue<TEntity>(TEntity entity, PropertyMapping mapping, object? value) where TEntity : class
    {
        if (value == null)
        {
            SetPropertyToDefault(entity, mapping);
            return;
        }

        var targetType = mapping.Property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            object? convertedValue = ConvertValue(value, underlyingType, mapping.ScimType);
            mapping.Property.SetValue(entity, convertedValue);
        }
        catch (Exception)
        {
            // Type conversion failed - skip silently or could throw
        }
    }

    /// <summary>
    /// Converts a SCIM value to the target property type.
    /// </summary>
    private static object? ConvertValue(object value, Type targetType, string scimType)
    {
        // Handle JsonElement from System.Text.Json
        if (value is JsonElement jsonElement)
        {
            return ConvertJsonElement(jsonElement, targetType);
        }

        // Direct type match
        if (targetType.IsAssignableFrom(value.GetType()))
        {
            return value;
        }

        // String to target type
        if (value is string stringValue)
        {
            return ConvertString(stringValue, targetType);
        }

        // Boolean handling
        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            if (value is bool boolValue)
                return boolValue;
            if (bool.TryParse(value.ToString(), out var parsed))
                return parsed;
            return false;
        }

        // DateTime handling
        if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
        {
            if (value is DateTime dateValue)
                return dateValue;
            if (DateTime.TryParse(value.ToString(), out var parsed))
                return parsed;
            return null;
        }

        // Integer handling
        if (targetType == typeof(int) || targetType == typeof(int?))
        {
            if (value is int intValue)
                return intValue;
            if (int.TryParse(value.ToString(), out var parsed))
                return parsed;
            return null;
        }

        // Fallback: try Convert.ChangeType
        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a JsonElement to the target type.
    /// </summary>
    private static object? ConvertJsonElement(JsonElement element, Type targetType)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => ConvertString(element.GetString() ?? "", targetType),
            JsonValueKind.True => targetType == typeof(bool) || targetType == typeof(bool?) ? true : null,
            JsonValueKind.False => targetType == typeof(bool) || targetType == typeof(bool?) ? false : null,
            JsonValueKind.Number when targetType == typeof(int) || targetType == typeof(int?) => element.GetInt32(),
            JsonValueKind.Number when targetType == typeof(long) || targetType == typeof(long?) => element.GetInt64(),
            JsonValueKind.Number when targetType == typeof(double) || targetType == typeof(double?) => element.GetDouble(),
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    /// <summary>
    /// Converts a string to the target type.
    /// </summary>
    private static object? ConvertString(string value, Type targetType)
    {
        if (targetType == typeof(string))
            return value;

        if (targetType == typeof(bool) || targetType == typeof(bool?))
            return bool.TryParse(value, out var b) ? b : null;

        if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            return DateTime.TryParse(value, out var d) ? d : null;

        if (targetType == typeof(int) || targetType == typeof(int?))
            return int.TryParse(value, out var i) ? i : null;

        if (targetType == typeof(Guid) || targetType == typeof(Guid?))
            return Guid.TryParse(value, out var g) ? g : null;

        return value;
    }

    /// <summary>
    /// Sets a property to its default value (for remove operations).
    /// </summary>
    private static void SetPropertyToDefault<TEntity>(TEntity entity, PropertyMapping mapping) where TEntity : class
    {
        var targetType = mapping.Property.PropertyType;

        // For nullable types, set to null
        if (Nullable.GetUnderlyingType(targetType) != null || !targetType.IsValueType)
        {
            mapping.Property.SetValue(entity, null);
            return;
        }

        // For value types, set to default
        mapping.Property.SetValue(entity, Activator.CreateInstance(targetType));
    }

    /// <summary>
    /// Normalizes a SCIM path for matching.
    /// </summary>
    private static string NormalizePath(string path)
    {
        // Remove array notation like [0], lowercase
        var normalized = path.ToLowerInvariant();
        
        // Handle common variations
        // e.g., "emails[type eq \"work\"].value" -> "emails[0].value"
        var bracketIndex = normalized.IndexOf('[');
        if (bracketIndex > 0)
        {
            var closeBracket = normalized.IndexOf(']', bracketIndex);
            if (closeBracket > bracketIndex)
            {
                // Simplify to [0] for matching
                var prefix = normalized.Substring(0, bracketIndex);
                var suffix = closeBracket < normalized.Length - 1 ? normalized.Substring(closeBracket + 1) : "";
                normalized = prefix + "[0]" + suffix;
            }
        }

        return normalized;
    }

    /// <summary>
    /// Property mapping information.
    /// </summary>
    private class PropertyMapping
    {
        public PropertyInfo Property { get; set; } = null!;
        public string ScimName { get; set; } = "";
        public string ScimType { get; set; } = "string";
        public bool IsRequired { get; set; }
    }
}

