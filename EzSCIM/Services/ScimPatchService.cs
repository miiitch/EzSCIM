using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using EzSCIM.Attributes;
using EzSCIM.Helpers;
using EzSCIM.Models;

namespace EzSCIM.Services;

/// <summary>
/// Canonical SCIM PATCH operation engine (RFC 7644 §3.5.2).
/// Applies PATCH operations to ScimUser and ScimGroup resources.
/// This is the single source of truth for PATCH logic in the library.
/// </summary>
public static class ScimPatchService
{
    #region Public API

    /// <summary>
    /// Applies a SCIM PATCH request to a user resource.
    /// Handles scalar attributes, complex attributes (name), and multi-valued attributes
    /// (emails, phoneNumbers, addresses) including filtered paths.
    /// </summary>
    /// <param name="user">The user to patch</param>
    /// <param name="patchRequest">The SCIM PATCH request</param>
    /// <returns>True if any property was modified</returns>
    public static bool ApplyPatch(ScimUser user, ScimPatchRequest patchRequest)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (patchRequest?.Operations == null) return false;

        bool modified = false;

        foreach (var operation in patchRequest.Operations)
        {
            if (ApplyUserOperation(user, operation))
                modified = true;
        }

        if (modified)
            user.Meta.LastModified = DateTime.UtcNow;

        return modified;
    }

    /// <summary>
    /// Applies a SCIM PATCH request to a group resource.
    /// Handles scalar attributes (displayName, externalId) and members array operations.
    /// </summary>
    /// <param name="group">The group to patch</param>
    /// <param name="patchRequest">The SCIM PATCH request</param>
    /// <returns>True if any property was modified</returns>
    public static bool ApplyPatch(ScimGroup group, ScimPatchRequest patchRequest)
    {
        if (group == null) throw new ArgumentNullException(nameof(group));
        if (patchRequest?.Operations == null) return false;

        bool modified = false;

        foreach (var operation in patchRequest.Operations)
        {
            if (ApplyGroupOperation(group, operation))
                modified = true;
        }

        if (modified)
            group.Meta.LastModified = DateTime.UtcNow;

        return modified;
    }

    /// <summary>
    /// Applies PATCH operations to any entity using [ScimProperty] attribute reflection.
    /// Handles scalar attributes only. Multi-valued attributes on ScimUser/ScimGroup
    /// should use the typed overloads instead.
    /// </summary>
    /// <typeparam name="TEntity">Entity type annotated with [ScimProperty] attributes</typeparam>
    /// <param name="entity">The entity to patch</param>
    /// <param name="operations">SCIM PATCH operations</param>
    /// <returns>True if any property was modified</returns>
    public static bool ApplyPatch<TEntity>(TEntity entity, IEnumerable<ScimPatchOperation> operations)
        where TEntity : class
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (operations == null) return false;

        var mappings = GetPropertyMappings(typeof(TEntity));
        bool modified = false;

        foreach (var op in operations)
        {
            if (ApplyGenericOperation(entity, op, mappings))
                modified = true;
        }

        return modified;
    }

    #endregion

    #region User PATCH Operations

    private static bool ApplyUserOperation(ScimUser user, ScimPatchOperation operation)
    {
        var op = operation.Op?.ToLowerInvariant() ?? "replace";
        var path = operation.Path?.Trim();
        var normalizedPath = path?.ToLowerInvariant() ?? string.Empty;

        // RFC 7644: If path is omitted, value must be a complex object
        if (string.IsNullOrWhiteSpace(normalizedPath) && (op == "replace" || op == "add") && operation.Value != null)
        {
            return ApplyUserBulkValue(user, operation.Value);
        }

        // Route to multi-valued attribute handlers
        if (normalizedPath.StartsWith("emails"))
            return ApplyMultiValuedOperation(user.Emails, op, normalizedPath, "emails", operation.Value, ParseEmails);

        if (normalizedPath.StartsWith("phonenumbers"))
            return ApplyMultiValuedOperation(user.PhoneNumbers, op, normalizedPath, "phonenumbers", operation.Value, ParsePhoneNumbers);

        if (normalizedPath.StartsWith("addresses"))
            return ApplyMultiValuedOperation(user.Addresses, op, normalizedPath, "addresses", operation.Value, ParseAddresses);

        // Complex attribute: name or name.*
        if (normalizedPath == "name" && op == "replace" && operation.Value is JsonElement nameElement && nameElement.ValueKind == JsonValueKind.Object)
        {
            ApplyNameObject(user, nameElement);
            return true;
        }

        if (normalizedPath.StartsWith("name."))
        {
            return ApplyNameScalar(user, op, normalizedPath, operation.Value);
        }

        // Scalar attributes
        return ApplyUserScalar(user, op, normalizedPath, operation.Value);
    }

    private static bool ApplyUserBulkValue(ScimUser user, object value)
    {
        if (value is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            bool modified = false;
            foreach (var property in element.EnumerateObject())
            {
                var name = property.Name.ToLowerInvariant();

                // Handle complex sub-objects
                if (name == "name" && property.Value.ValueKind == JsonValueKind.Object)
                {
                    ApplyNameObject(user, property.Value);
                    modified = true;
                    continue;
                }

                if (name == "emails")
                {
                    user.Emails = ParseEmails(property.Value);
                    modified = true;
                    continue;
                }

                if (name == "phonenumbers")
                {
                    user.PhoneNumbers = ParsePhoneNumbers(property.Value);
                    modified = true;
                    continue;
                }

                if (name == "addresses")
                {
                    user.Addresses = ParseAddresses(property.Value);
                    modified = true;
                    continue;
                }

                if (name.StartsWith("name."))
                {
                    ApplyNameScalar(user, "replace", name, property.Value);
                    modified = true;
                    continue;
                }

                // Scalar properties
                if (ApplyUserScalar(user, "replace", name, property.Value))
                    modified = true;
            }
            return modified;
        }

        if (value is Dictionary<string, object> dict)
        {
            bool modified = false;
            foreach (var entry in dict)
            {
                if (ApplyUserScalar(user, "replace", entry.Key.ToLowerInvariant(), entry.Value))
                    modified = true;
            }
            return modified;
        }

        return false;
    }

    private static bool ApplyUserScalar(ScimUser user, string op, string normalizedPath, object? value)
    {
        if (op == "remove")
            return RemoveUserAttribute(user, normalizedPath);

        if (op != "replace" && op != "add")
            return false;

        switch (normalizedPath)
        {
            case "active":
                user.Active = AttributeFilterHelper.ExtractBooleanValue(value);
                return true;
            case "username":
                user.UserName = AttributeFilterHelper.ExtractStringValue(value) ?? string.Empty;
                return true;
            case "displayname":
                user.DisplayName = AttributeFilterHelper.ExtractStringValue(value);
                return true;
            case "externalid":
                user.ExternalId = AttributeFilterHelper.ExtractStringValue(value);
                return true;
            case "nickname":
                user.NickName = AttributeFilterHelper.ExtractStringValue(value);
                return true;
            case "profileurl":
                user.ProfileUrl = AttributeFilterHelper.ExtractStringValue(value);
                return true;
            case "title":
                user.Title = AttributeFilterHelper.ExtractStringValue(value);
                return true;
            case "usertype":
                user.UserType = AttributeFilterHelper.ExtractStringValue(value);
                return true;
            case "preferredlanguage":
                user.PreferredLanguage = AttributeFilterHelper.ExtractStringValue(value);
                return true;
            case "locale":
                user.Locale = AttributeFilterHelper.ExtractStringValue(value);
                return true;
            case "timezone":
                user.Timezone = AttributeFilterHelper.ExtractStringValue(value);
                return true;
            default:
                // Store in custom attributes
                if (value != null)
                {
                    user.CustomAttributes[normalizedPath] = value;
                    return true;
                }
                return false;
        }
    }

    private static bool RemoveUserAttribute(ScimUser user, string normalizedPath)
    {
        switch (normalizedPath)
        {
            case "externalid": user.ExternalId = null; return true;
            case "displayname": user.DisplayName = null; return true;
            case "nickname": user.NickName = null; return true;
            case "profileurl": user.ProfileUrl = null; return true;
            case "title": user.Title = null; return true;
            case "usertype": user.UserType = null; return true;
            case "preferredlanguage": user.PreferredLanguage = null; return true;
            case "locale": user.Locale = null; return true;
            case "timezone": user.Timezone = null; return true;
            case "active": user.Active = false; return true;
            case "name": user.Name = new ScimName(); return true;
            case "name.formatted": if (user.Name != null) user.Name.Formatted = null; return true;
            case "name.givenname": if (user.Name != null) user.Name.GivenName = null; return true;
            case "name.familyname": if (user.Name != null) user.Name.FamilyName = null; return true;
            case "name.middlename": if (user.Name != null) user.Name.MiddleName = null; return true;
            case "name.honorificprefix": if (user.Name != null) user.Name.HonorificPrefix = null; return true;
            case "name.honorificsuffix": if (user.Name != null) user.Name.HonorificSuffix = null; return true;
            case "emails": user.Emails.Clear(); return true;
            case "phonenumbers": user.PhoneNumbers.Clear(); return true;
            case "addresses": user.Addresses.Clear(); return true;
            default:
                // Check for filtered remove on multi-valued attributes
                var filteredPath = AttributeFilterHelper.ParseFilteredPath(normalizedPath);
                if (filteredPath.HasValue)
                {
                    var (arrayProp, filterExpr, _) = filteredPath.Value;
                    return RemoveFilteredItems(user, arrayProp, filterExpr);
                }
                return user.CustomAttributes.Remove(normalizedPath);
        }
    }

    private static bool RemoveFilteredItems(ScimUser user, string arrayProp, string filterExpr)
    {
        switch (arrayProp)
        {
            case "emails":
                return RemoveMatchingItems(user.Emails, filterExpr);
            case "phonenumbers":
                return RemoveMatchingItems(user.PhoneNumbers, filterExpr);
            case "addresses":
                return RemoveMatchingItems(user.Addresses, filterExpr);
            default:
                return false;
        }
    }

    private static bool RemoveMatchingItems<T>(List<T> items, string filterExpr) where T : class
    {
        var toRemove = items.Where(item => AttributeFilterHelper.EvaluateSimpleFilter(item, filterExpr)).ToList();
        foreach (var item in toRemove)
            items.Remove(item);
        return toRemove.Count > 0;
    }

    #endregion

    #region Name Operations

    private static void ApplyNameObject(ScimUser user, JsonElement nameElement)
    {
        user.Name ??= new ScimName();

        if (nameElement.TryGetProperty("formatted", out var formatted))
            user.Name.Formatted = AttributeFilterHelper.ExtractStringValue(formatted);
        if (nameElement.TryGetProperty("familyName", out var familyName))
            user.Name.FamilyName = AttributeFilterHelper.ExtractStringValue(familyName);
        if (nameElement.TryGetProperty("givenName", out var givenName))
            user.Name.GivenName = AttributeFilterHelper.ExtractStringValue(givenName);
        if (nameElement.TryGetProperty("middleName", out var middleName))
            user.Name.MiddleName = AttributeFilterHelper.ExtractStringValue(middleName);
        if (nameElement.TryGetProperty("honorificPrefix", out var prefix))
            user.Name.HonorificPrefix = AttributeFilterHelper.ExtractStringValue(prefix);
        if (nameElement.TryGetProperty("honorificSuffix", out var suffix))
            user.Name.HonorificSuffix = AttributeFilterHelper.ExtractStringValue(suffix);
    }

    private static bool ApplyNameScalar(ScimUser user, string op, string normalizedPath, object? value)
    {
        if (op == "remove")
            return RemoveUserAttribute(user, normalizedPath);

        user.Name ??= new ScimName();
        var stringValue = AttributeFilterHelper.ExtractStringValue(value);

        switch (normalizedPath)
        {
            case "name.formatted": user.Name.Formatted = stringValue; return true;
            case "name.givenname": user.Name.GivenName = stringValue; return true;
            case "name.familyname": user.Name.FamilyName = stringValue; return true;
            case "name.middlename": user.Name.MiddleName = stringValue; return true;
            case "name.honorificprefix": user.Name.HonorificPrefix = stringValue; return true;
            case "name.honorificsuffix": user.Name.HonorificSuffix = stringValue; return true;
            default: return false;
        }
    }

    #endregion

    #region Multi-Valued Attribute Operations

    /// <summary>
    /// Handles add/replace/remove on multi-valued attributes (emails, phoneNumbers, addresses)
    /// including filtered paths like "emails[primary eq true].value".
    /// </summary>
    private static bool ApplyMultiValuedOperation<T>(
        List<T> items,
        string op,
        string normalizedPath,
        string attrName,
        object? value,
        Func<JsonElement, List<T>> parser) where T : class, new()
    {
        // Direct path (e.g., "emails") — replace or add entire array
        if (normalizedPath == attrName)
        {
            if (op == "replace" && value is JsonElement replaceElement)
            {
                var newItems = parser(replaceElement);
                items.Clear();
                items.AddRange(newItems);
                return true;
            }

            if (op == "add" && value is JsonElement addElement)
            {
                var newItems = parser(addElement);
                foreach (var item in newItems)
                {
                    if (!ContainsDuplicate(items, item))
                        items.Add(item);
                }
                return newItems.Count > 0;
            }

            if (op == "remove")
            {
                if (value is JsonElement removeElement)
                {
                    var toRemove = parser(removeElement);
                    foreach (var item in toRemove)
                        RemoveByValue(items, item);
                    return toRemove.Count > 0;
                }

                items.Clear();
                return true;
            }

            return false;
        }

        // Filtered path (e.g., "emails[primary eq true].value")
        var filteredPath = ParseMultiValuedPath(normalizedPath);
        if (filteredPath == null)
            return false;

        var (filterExpr, subAttr) = filteredPath.Value;

        if (op == "replace" || op == "add")
        {
            var matching = FindMatchingItems(items, filterExpr);

            // If no match found for "primary eq true", create a new item
            if (matching.Count == 0 && filterExpr.Contains("primary eq true"))
            {
                var newItem = new T();
                SetProperty(newItem, "Primary", true);
                items.Add(newItem);
                matching.Add(newItem);
            }

            if (subAttr != null)
            {
                var stringValue = AttributeFilterHelper.ExtractStringValue(value);
                foreach (var item in matching)
                    SetProperty(item, subAttr, stringValue);
            }

            return matching.Count > 0;
        }

        if (op == "remove")
        {
            var matching = FindMatchingItems(items, filterExpr);
            foreach (var item in matching)
                items.Remove(item);
            return matching.Count > 0;
        }

        return false;
    }

    private static (string filterExpr, string? subAttr)? ParseMultiValuedPath(string path)
    {
        // Match "attrName[filter].subAttr" or "attrName[filter]"
        var match = Regex.Match(path, @"^\w+\[([^\]]+)\](?:\.(\w+))?$");
        if (!match.Success)
            return null;

        var filterExpr = match.Groups[1].Value;
        var subAttr = match.Groups[2].Success ? match.Groups[2].Value : null;
        return (filterExpr, subAttr);
    }

    private static List<T> FindMatchingItems<T>(List<T> items, string filterExpr) where T : class
    {
        // Index access: "0", "1", etc.
        if (int.TryParse(filterExpr, out var index))
        {
            if (index >= 0 && index < items.Count)
                return new List<T> { items[index] };
            return new List<T>();
        }

        // Filter expression: "primary eq true", "type eq \"work\""
        return items.Where(item => AttributeFilterHelper.EvaluateSimpleFilter(item, filterExpr)).ToList();
    }

    private static void SetProperty(object item, string propertyName, object? value)
    {
        var prop = item.GetType().GetProperty(propertyName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (prop?.CanWrite == true)
        {
            if (value is string strVal && prop.PropertyType == typeof(bool))
                prop.SetValue(item, bool.TryParse(strVal, out var b) && b);
            else
                prop.SetValue(item, value);
        }
    }

    private static bool ContainsDuplicate<T>(List<T> items, T newItem) where T : class
    {
        var valueProp = typeof(T).GetProperty("Value");
        if (valueProp == null) return false;

        var newValue = valueProp.GetValue(newItem)?.ToString();
        return items.Any(existing =>
            valueProp.GetValue(existing)?.ToString()?.Equals(newValue, StringComparison.OrdinalIgnoreCase) == true);
    }

    private static void RemoveByValue<T>(List<T> items, T target) where T : class
    {
        var valueProp = typeof(T).GetProperty("Value");
        if (valueProp == null) return;

        var targetValue = valueProp.GetValue(target)?.ToString();
        var existing = items.FirstOrDefault(item =>
            valueProp.GetValue(item)?.ToString()?.Equals(targetValue, StringComparison.OrdinalIgnoreCase) == true);
        if (existing != null)
            items.Remove(existing);
    }

    #endregion

    #region Group PATCH Operations

    private static bool ApplyGroupOperation(ScimGroup group, ScimPatchOperation operation)
    {
        var op = operation.Op?.ToLowerInvariant() ?? "replace";
        var path = operation.Path?.Trim();
        var normalizedPath = path?.ToLowerInvariant() ?? string.Empty;

        // Bulk value (no path)
        if (string.IsNullOrWhiteSpace(normalizedPath) && (op == "replace" || op == "add") && operation.Value != null)
        {
            return ApplyGroupBulkValue(group, op, operation.Value);
        }

        // Members operations
        if (normalizedPath == "members" || normalizedPath.StartsWith("members["))
        {
            return ApplyMembersOperation(group, op, normalizedPath, operation);
        }

        // Scalar attributes
        if (op == "remove")
        {
            switch (normalizedPath)
            {
                case "externalid": group.ExternalId = null; return true;
                case "displayname": group.DisplayName = string.Empty; return true;
                default: return group.CustomAttributes.Remove(normalizedPath);
            }
        }

        if (op == "replace" || op == "add")
        {
            switch (normalizedPath)
            {
                case "externalid":
                    group.ExternalId = AttributeFilterHelper.ExtractStringValue(operation.Value);
                    return true;
                case "displayname":
                    group.DisplayName = AttributeFilterHelper.ExtractStringValue(operation.Value) ?? string.Empty;
                    return true;
                default:
                    if (operation.Value != null)
                    {
                        group.CustomAttributes[normalizedPath] = operation.Value;
                        return true;
                    }
                    return false;
            }
        }

        return false;
    }

    private static bool ApplyGroupBulkValue(ScimGroup group, string op, object value)
    {
        if (value is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            bool modified = false;
            foreach (var property in element.EnumerateObject())
            {
                var name = property.Name.ToLowerInvariant();
                switch (name)
                {
                    case "externalid":
                        group.ExternalId = AttributeFilterHelper.ExtractStringValue(property.Value);
                        modified = true;
                        break;
                    case "displayname":
                        group.DisplayName = AttributeFilterHelper.ExtractStringValue(property.Value) ?? string.Empty;
                        modified = true;
                        break;
                    case "members":
                        if (op == "add")
                        {
                            var members = ParseMembers(property.Value);
                            var current = EnsureMembers(group);
                            foreach (var m in members)
                                if (!current.Any(e => e.Value == m.Value))
                                    current.Add(m);
                        }
                        else
                        {
                            group.Members = ParseMembers(property.Value);
                        }
                        modified = true;
                        break;
                }
            }
            return modified;
        }
        return false;
    }

    private static bool ApplyMembersOperation(ScimGroup group, string op, string normalizedPath, ScimPatchOperation operation)
    {
        var members = EnsureMembers(group);

        if (op == "add" && operation.Value != null)
        {
            var newMembers = ParseMembers(operation.Value);
            foreach (var m in newMembers)
                if (!members.Any(e => e.Value == m.Value))
                    members.Add(m);
            return newMembers.Count > 0;
        }

        if (op == "replace" && operation.Value != null)
        {
            group.Members = ParseMembers(operation.Value);
            return true;
        }

        if (op == "remove")
        {
            // "members[value eq \"id\"]"
            if (normalizedPath.Contains("[value eq", StringComparison.OrdinalIgnoreCase))
            {
                var startIdx = operation.Path!.IndexOf('"') + 1;
                var endIdx = operation.Path!.LastIndexOf('"');
                if (startIdx > 0 && endIdx > startIdx)
                {
                    var memberId = operation.Path!.Substring(startIdx, endIdx - startIdx);
                    var existing = members.FirstOrDefault(m => m.Value == memberId);
                    if (existing != null)
                    {
                        members.Remove(existing);
                        return true;
                    }
                }
            }
            // Members in Value
            else if (operation.Value != null)
            {
                var toRemove = ParseMembers(operation.Value);
                bool removed = false;
                foreach (var m in toRemove)
                {
                    var existing = members.FirstOrDefault(e => e.Value == m.Value);
                    if (existing != null)
                    {
                        members.Remove(existing);
                        removed = true;
                    }
                }
                return removed;
            }
        }

        return false;
    }

    private static List<ScimMember> EnsureMembers(ScimGroup group)
    {
        group.Members ??= new List<ScimMember>();
        return group.Members;
    }

    #endregion

    #region JSON Parsers

    private static List<ScimEmail> ParseEmails(JsonElement element)
    {
        var result = new List<ScimEmail>();

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                result.Add(ParseSingleEmail(item));
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            result.Add(ParseSingleEmail(element));
        }

        return result;
    }

    private static ScimEmail ParseSingleEmail(JsonElement item)
    {
        var email = new ScimEmail();
        if (item.TryGetProperty("value", out var v)) email.Value = v.GetString() ?? string.Empty;
        if (item.TryGetProperty("type", out var t)) email.Type = t.GetString();
        if (item.TryGetProperty("primary", out var p)) email.Primary = p.ValueKind == JsonValueKind.True;
        return email;
    }

    private static List<ScimPhoneNumber> ParsePhoneNumbers(JsonElement element)
    {
        var result = new List<ScimPhoneNumber>();
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var phone = new ScimPhoneNumber();
                if (item.TryGetProperty("value", out var v)) phone.Value = v.GetString() ?? string.Empty;
                if (item.TryGetProperty("type", out var t)) phone.Type = t.GetString();
                if (item.TryGetProperty("primary", out var p)) phone.Primary = p.ValueKind == JsonValueKind.True;
                result.Add(phone);
            }
        }
        return result;
    }

    private static List<ScimAddress> ParseAddresses(JsonElement element)
    {
        var result = new List<ScimAddress>();
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var addr = new ScimAddress();
                if (item.TryGetProperty("streetAddress", out var sa)) addr.StreetAddress = sa.GetString();
                if (item.TryGetProperty("locality", out var l)) addr.Locality = l.GetString();
                if (item.TryGetProperty("region", out var r)) addr.Region = r.GetString();
                if (item.TryGetProperty("postalCode", out var pc)) addr.PostalCode = pc.GetString();
                if (item.TryGetProperty("country", out var c)) addr.Country = c.GetString();
                if (item.TryGetProperty("type", out var t)) addr.Type = t.GetString();
                if (item.TryGetProperty("primary", out var p)) addr.Primary = p.ValueKind == JsonValueKind.True;
                if (item.TryGetProperty("formatted", out var f)) addr.Formatted = f.GetString();
                result.Add(addr);
            }
        }
        return result;
    }

    private static List<ScimMember> ParseMembers(object value)
    {
        var result = new List<ScimMember>();

        if (value is JsonElement element && element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.TryGetProperty("value", out var v))
                {
                    var member = new ScimMember { Value = v.GetString() ?? string.Empty };
                    if (item.TryGetProperty("display", out var d)) member.Display = d.GetString();
                    result.Add(member);
                }
            }
        }
        else if (value is List<Dictionary<string, string>> dictList)
        {
            foreach (var dict in dictList)
            {
                var member = new ScimMember();
                if (dict.TryGetValue("value", out var mv)) member.Value = mv;
                if (dict.TryGetValue("display", out var md)) member.Display = md;
                if (!string.IsNullOrEmpty(member.Value))
                    result.Add(member);
            }
        }

        return result;
    }

    #endregion

    #region Generic Reflection-Based PATCH (for custom entity types)

    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyMapping>> _propertyCache = new();

    private static Dictionary<string, PropertyMapping> GetPropertyMappings(Type type)
    {
        return _propertyCache.GetOrAdd(type, t =>
        {
            var mappings = new Dictionary<string, PropertyMapping>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
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

                    mappings[scimAttr.Name] = mapping;

                    var lowerName = scimAttr.Name.ToLowerInvariant();
                    if (!mappings.ContainsKey(lowerName))
                        mappings[lowerName] = mapping;

                    if (!mappings.ContainsKey(property.Name))
                        mappings[property.Name] = mapping;
                }
            }

            return mappings;
        });
    }

    private static bool ApplyGenericOperation<TEntity>(TEntity entity, ScimPatchOperation op, Dictionary<string, PropertyMapping> mappings)
        where TEntity : class
    {
        var operation = op.Op?.ToLowerInvariant() ?? "replace";

        // Bulk replace when path is omitted
        if (string.IsNullOrEmpty(op.Path) && (operation == "replace" || operation == "add") && op.Value != null)
        {
            return ApplyGenericBulkReplace(entity, op.Value, mappings);
        }

        var path = op.Path ?? "";

        // Skip members (handled separately)
        if (path.Equals("members", StringComparison.OrdinalIgnoreCase))
            return false;

        // Find mapping
        if (!mappings.TryGetValue(path, out var mapping))
        {
            var normalized = NormalizePath(path);
            if (!mappings.TryGetValue(normalized, out mapping))
                return false;
        }

        switch (operation)
        {
            case "add":
            case "replace":
                SetReflectionValue(entity, mapping, op.Value);
                return true;
            case "remove":
                SetReflectionDefault(entity, mapping);
                return true;
            default:
                return false;
        }
    }

    private static bool ApplyGenericBulkReplace<TEntity>(TEntity entity, object value, Dictionary<string, PropertyMapping> mappings)
        where TEntity : class
    {
        if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
        {
            bool modified = false;
            foreach (var prop in jsonElement.EnumerateObject())
            {
                if (mappings.TryGetValue(prop.Name, out var mapping))
                {
                    SetReflectionValue(entity, mapping, prop.Value);
                    modified = true;
                }
            }
            return modified;
        }
        return false;
    }

    private static void SetReflectionValue<TEntity>(TEntity entity, PropertyMapping mapping, object? value) where TEntity : class
    {
        if (value == null)
        {
            SetReflectionDefault(entity, mapping);
            return;
        }

        var targetType = mapping.Property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            object? converted = ConvertValue(value, underlyingType);
            mapping.Property.SetValue(entity, converted);
        }
        catch
        {
            // Type conversion failed — skip silently
        }
    }

    private static void SetReflectionDefault<TEntity>(TEntity entity, PropertyMapping mapping) where TEntity : class
    {
        var targetType = mapping.Property.PropertyType;
        if (Nullable.GetUnderlyingType(targetType) != null || !targetType.IsValueType)
            mapping.Property.SetValue(entity, null);
        else
            mapping.Property.SetValue(entity, Activator.CreateInstance(targetType));
    }

    private static object? ConvertValue(object value, Type targetType)
    {
        if (value is JsonElement el)
        {
            return el.ValueKind switch
            {
                JsonValueKind.String => ConvertString(el.GetString() ?? "", targetType),
                JsonValueKind.True => targetType == typeof(bool) || targetType == typeof(bool?) ? true : null,
                JsonValueKind.False => targetType == typeof(bool) || targetType == typeof(bool?) ? false : null,
                JsonValueKind.Number when targetType == typeof(int) || targetType == typeof(int?) => el.GetInt32(),
                JsonValueKind.Number when targetType == typeof(long) || targetType == typeof(long?) => el.GetInt64(),
                JsonValueKind.Number when targetType == typeof(double) || targetType == typeof(double?) => el.GetDouble(),
                JsonValueKind.Null => null,
                _ => el.ToString()
            };
        }

        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        if (value is string s)
            return ConvertString(s, targetType);

        try { return Convert.ChangeType(value, targetType); }
        catch { return null; }
    }

    private static object? ConvertString(string value, Type targetType)
    {
        if (targetType == typeof(string)) return value;
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

    private static string NormalizePath(string path)
    {
        var normalized = path.ToLowerInvariant();
        var bracketIndex = normalized.IndexOf('[');
        if (bracketIndex > 0)
        {
            var closeBracket = normalized.IndexOf(']', bracketIndex);
            if (closeBracket > bracketIndex)
            {
                var prefix = normalized[..bracketIndex];
                var suffix = closeBracket < normalized.Length - 1 ? normalized[(closeBracket + 1)..] : "";
                normalized = prefix + "[0]" + suffix;
            }
        }
        return normalized;
    }

    private class PropertyMapping
    {
        public PropertyInfo Property { get; set; } = null!;
        public string ScimName { get; set; } = "";
        public string ScimType { get; set; } = "string";
        public bool IsRequired { get; set; }
    }

    #endregion
}

