using System.Text.RegularExpressions;
using System.Text.Json;
using EzSCIM.Models;

namespace EzSCIM.Helpers
{
    /// <summary>
    /// Helper class for filtering SCIM resource attributes based on SCIM query parameters
    /// and handling complex filter expressions in PATCH operations.
    /// </summary>
    public static class AttributeFilterHelper
    {
        /// <summary>
        /// Parses a comma-separated list of attribute names from query parameter
        /// </summary>
        public static HashSet<string> ParseAttributeList(string? attributeString)
        {
            if (string.IsNullOrWhiteSpace(attributeString))
                return new HashSet<string>();

            return new HashSet<string>(
                attributeString.Split(',')
                    .Select(a => a.Trim().ToLowerInvariant())
                    .Where(a => !string.IsNullOrEmpty(a))
            );
        }

        /// <summary>
        /// Filters a User resource to exclude specified attributes
        /// </summary>
        public static ScimUser FilterUserAttributes(ScimUser user, HashSet<string> attributesToExclude)
        {
            if (attributesToExclude.Count == 0)
                return user;

            if (attributesToExclude.Contains("emails"))
                user.Emails = new List<ScimEmail>();

            if (attributesToExclude.Contains("phonenumbers"))
                user.PhoneNumbers = new List<ScimPhoneNumber>();

            if (attributesToExclude.Contains("addresses"))
                user.Addresses = new List<ScimAddress>();

            if (attributesToExclude.Contains("name"))
                user.Name = new ScimName();

            if (attributesToExclude.Contains("groups"))
                user.Groups = new List<ScimGroupMembership>();

            return user;
        }

        /// <summary>
        /// Filters a Group resource to exclude specified attributes
        /// </summary>
        public static ScimGroup FilterGroupAttributes(ScimGroup group, HashSet<string> attributesToExclude)
        {
            if (attributesToExclude.Count == 0)
                return group;

            if (attributesToExclude.Contains("members"))
                group.Members = null;

            return group;
        }

        /// <summary>
        /// Parses a complex PATCH filter expression like "emails[primary eq true].value"
        /// Returns tuple of (arrayProperty, filterExpression, targetProperty)
        /// Example: "emails[primary eq true].value" -> ("emails", "primary eq true", "value")
        /// </summary>
        public static (string arrayProperty, string filterExpression, string targetProperty)? 
            ParseFilteredPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // Pattern: property[filter].property
            // Example: emails[primary eq true].value
            var match = Regex.Match(path, @"^(\w+)\[([^\]]+)\]\.(.+)$", RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                var arrayProperty = match.Groups[1].Value.ToLowerInvariant();
                var filterExpression = match.Groups[2].Value;
                var targetProperty = match.Groups[3].Value.ToLowerInvariant();
                
                return (arrayProperty, filterExpression, targetProperty);
            }

            return null;
        }

        /// <summary>
        /// Evaluates a simple filter expression like "primary eq true"
        /// </summary>
        public static bool EvaluateSimpleFilter(object? item, string filterExpression)
        {
            if (item == null || string.IsNullOrWhiteSpace(filterExpression))
                return false;

            // Parse simple equality filters: "property eq value"
            var parts = filterExpression.Split(new[] { " eq " }, StringSplitOptions.None);
            if (parts.Length != 2)
                return false;

            var propertyName = parts[0].Trim().ToLowerInvariant();
            var expectedValue = parts[1].Trim().ToLowerInvariant();

            // Get the property value
            var itemType = item.GetType();
            var property = itemType.GetProperty(propertyName, 
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (property == null)
                return false;

            var actualValue = property.GetValue(item)?.ToString()?.ToLowerInvariant() ?? "";
            return actualValue == expectedValue;
        }

        /// <summary>
        /// Extracts a boolean value from a JSON element or object
        /// </summary>
        public static bool ExtractBooleanValue(object? value)
        {
            if (value is JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.String => bool.TryParse(element.GetString(), out var b) && b,
                    _ => false
                };
            }

            if (value is bool boolValue)
                return boolValue;

            if (value is string stringValue)
                return bool.TryParse(stringValue, out var b) && b;

            return false;
        }

        /// <summary>
        /// Extracts a string value from a JSON element or object
        /// </summary>
        public static string? ExtractStringValue(object? value)
        {
            if (value is JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Number => element.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => null
                };
            }

            return value?.ToString();
        }

        /// <summary>
        /// Applies a replace operation to a multi-valued attribute with a filter expression
        /// Example: path="emails[primary eq true].value", value="new@example.com"
        /// This will find the primary email and replace its value
        /// </summary>
        public static void ApplyFilteredReplaceOperation<T>(
            List<T> items,
            string filterExpression,
            string targetProperty,
            object? newValue) where T : class
        {
            foreach (var item in items)
            {
                if (EvaluateSimpleFilter(item, filterExpression))
                {
                    var property = item.GetType().GetProperty(targetProperty,
                        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);

                    if (property?.CanWrite == true)
                    {
                        var stringValue = ExtractStringValue(newValue);
                        property.SetValue(item, stringValue);
                    }
                }
            }
        }
    }
}

