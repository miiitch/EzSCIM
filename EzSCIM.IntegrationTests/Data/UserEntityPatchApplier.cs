using EzSCIM.IntegrationTests.Data.Entities;
using EzSCIM.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EzSCIM.IntegrationTests.Data;

/// <summary>
/// PATCH applier for UserEntity with JSON multi-valued attributes
/// </summary>
public static class UserEntityPatchApplier
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static bool ApplyPatch(UserEntity entity, IEnumerable<ScimPatchOperation> operations)
    {
        if (entity == null || operations == null)
            return false;

        bool modified = false;

        foreach (var op in operations)
        {
            if (ApplyOperation(entity, op))
                modified = true;
        }

        return modified;
    }

    private static bool ApplyOperation(UserEntity entity, ScimPatchOperation op)
    {
        var operation = op.Op?.ToLowerInvariant() ?? "replace";
        var path = op.Path?.Trim() ?? "";

        // If path is empty and operation is replace or add, apply bulk replace
        if (string.IsNullOrEmpty(path) && (operation == "replace" || operation == "add") && op.Value != null)
        {
            return ApplyBulkReplace(entity, op.Value);
        }

        // Normalize path
        var normalizedPath = path.ToLowerInvariant();

        // Check if it's a multi-valued attribute path
        if (normalizedPath.StartsWith("emails"))
        {
            return ApplyEmailOperation(entity, operation, normalizedPath, op.Value);
        }
        else if (normalizedPath.StartsWith("phonenumbers"))
        {
            return ApplyPhoneOperation(entity, operation, normalizedPath, op.Value);
        }
        else if (normalizedPath.StartsWith("addresses"))
        {
            return ApplyAddressOperation(entity, operation, normalizedPath, op.Value);
        }
        else
        {
            // Handle scalar attributes
            return ApplyScalarOperation(entity, operation, normalizedPath, op.Value);
        }
    }

    private static bool ApplyBulkReplace(UserEntity entity, object value)
    {
        if (value is not JsonElement jsonElement || jsonElement.ValueKind != JsonValueKind.Object)
            return false;

        bool modified = false;

        foreach (var prop in jsonElement.EnumerateObject())
        {
            var propName = prop.Name.ToLowerInvariant();

            switch (propName)
            {
                case "externalid":
                    entity.ExternalId = ExtractString(prop.Value);
                    modified = true;
                    break;
                case "displayname":
                    entity.DisplayName = ExtractString(prop.Value);
                    modified = true;
                    break;
                case "nickname":
                    entity.NickName = ExtractString(prop.Value);
                    modified = true;
                    break;
                case "profileurl":
                    entity.ProfileUrl = ExtractString(prop.Value);
                    modified = true;
                    break;
                case "active":
                    entity.Active = ExtractBool(prop.Value);
                    modified = true;
                    break;
                case "title":
                    entity.Title = ExtractString(prop.Value);
                    modified = true;
                    break;
                case "usertype":
                    entity.UserType = ExtractString(prop.Value);
                    modified = true;
                    break;
                case "preferredlanguage":
                    entity.PreferredLanguage = ExtractString(prop.Value);
                    modified = true;
                    break;
                case "locale":
                    entity.Locale = ExtractString(prop.Value);
                    modified = true;
                    break;
                case "timezone":
                    entity.Timezone = ExtractString(prop.Value);
                    modified = true;
                    break;
                // Add more scalar properties as needed
                default:
                    if (propName.StartsWith("name."))
                    {
                        modified |= ApplyNameProperty(entity, propName, prop.Value);
                    }
                    break;
            }
        }

        return modified;
    }

    private static bool ApplyEmailOperation(UserEntity entity, string operation, string path, object? value)
    {
        var emails = MultiValuedAttributeHelper.DeserializeEmails(entity.EmailsJson);

        if (operation == "replace")
        {
            // Parse filtered path like "emails[primary eq true].value" or "emails[0].value"
            var (index, filter, subAttr) = ParseFilteredPath(path);

            if (subAttr != null)
            {
                // Find matching email(s)
                var matchingEmails = FindMatchingItems(emails, index, filter);

                foreach (var email in matchingEmails)
                {
                    switch (subAttr.ToLowerInvariant())
                    {
                        case "value":
                            email.Value = ExtractString(value) ?? "";
                            break;
                        case "type":
                            email.Type = ExtractString(value);
                            break;
                        case "primary":
                            email.Primary = ExtractBool(value);
                            break;
                    }
                }

                entity.EmailsJson = MultiValuedAttributeHelper.SerializeEmails(emails);
                return matchingEmails.Count > 0;
            }
        }
        else if (operation == "add")
        {
            // Add new email(s)
            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    var email = new EmailData
                    {
                        Value = item.TryGetProperty("value", out var v) ? ExtractString(v) ?? "" : "",
                        Type = item.TryGetProperty("type", out var t) ? ExtractString(t) : null,
                        Primary = item.TryGetProperty("primary", out var p) && ExtractBool(p)
                    };
                    emails.Add(email);
                }

                entity.EmailsJson = MultiValuedAttributeHelper.SerializeEmails(emails);
                return true;
            }
        }
        else if (operation == "remove")
        {
            var (index, filter, subAttr) = ParseFilteredPath(path);
            var matchingEmails = FindMatchingItems(emails, index, filter);

            foreach (var email in matchingEmails)
            {
                if (subAttr == null || subAttr.ToLowerInvariant() == "value")
                {
                    // Remove entire email
                    emails.Remove(email);
                }
                else
                {
                    // Clear specific sub-attribute
                    switch (subAttr.ToLowerInvariant())
                    {
                        case "value":
                            email.Value = "";
                            break;
                        case "type":
                            email.Type = null;
                            break;
                    }
                }
            }

            entity.EmailsJson = MultiValuedAttributeHelper.SerializeEmails(emails);
            return matchingEmails.Count > 0;
        }

        return false;
    }

    private static bool ApplyPhoneOperation(UserEntity entity, string operation, string path, object? value)
    {
        var phones = MultiValuedAttributeHelper.DeserializePhones(entity.PhoneNumbersJson);

        if (operation == "replace")
        {
            var (index, filter, subAttr) = ParseFilteredPath(path);

            if (subAttr != null)
            {
                var matchingPhones = FindMatchingItems(phones, index, filter);

                foreach (var phone in matchingPhones)
                {
                    switch (subAttr.ToLowerInvariant())
                    {
                        case "value":
                            phone.Value = ExtractString(value) ?? "";
                            break;
                        case "type":
                            phone.Type = ExtractString(value);
                            break;
                        case "primary":
                            phone.Primary = ExtractBool(value);
                            break;
                    }
                }

                entity.PhoneNumbersJson = MultiValuedAttributeHelper.SerializePhones(phones);
                return matchingPhones.Count > 0;
            }
        }
        else if (operation == "add")
        {
            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    var phone = new PhoneNumberData
                    {
                        Value = item.TryGetProperty("value", out var v) ? ExtractString(v) ?? "" : "",
                        Type = item.TryGetProperty("type", out var t) ? ExtractString(t) : null,
                        Primary = item.TryGetProperty("primary", out var p) && ExtractBool(p)
                    };
                    phones.Add(phone);
                }

                entity.PhoneNumbersJson = MultiValuedAttributeHelper.SerializePhones(phones);
                return true;
            }
        }

        return false;
    }

    private static bool ApplyAddressOperation(UserEntity entity, string operation, string path, object? value)
    {
        var addresses = MultiValuedAttributeHelper.DeserializeAddresses(entity.AddressesJson);

        if (operation == "replace")
        {
            var (index, filter, subAttr) = ParseFilteredPath(path);

            if (subAttr != null)
            {
                var matchingAddresses = FindMatchingItems(addresses, index, filter);

                foreach (var address in matchingAddresses)
                {
                    switch (subAttr.ToLowerInvariant())
                    {
                        case "formatted":
                            address.Formatted = ExtractString(value);
                            break;
                        case "streetaddress":
                            address.StreetAddress = ExtractString(value);
                            break;
                        case "locality":
                            address.Locality = ExtractString(value);
                            break;
                        case "region":
                            address.Region = ExtractString(value);
                            break;
                        case "postalcode":
                            address.PostalCode = ExtractString(value);
                            break;
                        case "country":
                            address.Country = ExtractString(value);
                            break;
                        case "type":
                            address.Type = ExtractString(value);
                            break;
                        case "primary":
                            address.Primary = ExtractBool(value);
                            break;
                    }
                }

                entity.AddressesJson = MultiValuedAttributeHelper.SerializeAddresses(addresses);
                return matchingAddresses.Count > 0;
            }
        }
        else if (operation == "add")
        {
            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    var address = new AddressData
                    {
                        Formatted = item.TryGetProperty("formatted", out var f) ? ExtractString(f) : null,
                        StreetAddress = item.TryGetProperty("streetAddress", out var sa) ? ExtractString(sa) : null,
                        Locality = item.TryGetProperty("locality", out var l) ? ExtractString(l) : null,
                        Region = item.TryGetProperty("region", out var r) ? ExtractString(r) : null,
                        PostalCode = item.TryGetProperty("postalCode", out var pc) ? ExtractString(pc) : null,
                        Country = item.TryGetProperty("country", out var c) ? ExtractString(c) : null,
                        Type = item.TryGetProperty("type", out var t) ? ExtractString(t) : null,
                        Primary = item.TryGetProperty("primary", out var p) && ExtractBool(p)
                    };
                    addresses.Add(address);
                }

                entity.AddressesJson = MultiValuedAttributeHelper.SerializeAddresses(addresses);
                return true;
            }
        }

        return false;
    }

    private static bool ApplyScalarOperation(UserEntity entity, string operation, string path, object? value)
    {
        // Handle remove operation
        if (operation == "remove")
        {
            return RemoveScalarAttribute(entity, path);
        }
        
        if (operation != "replace" && operation != "add")
            return false;

        switch (path)
        {
            case "externalid":
                entity.ExternalId = ExtractString(value);
                return true;
            case "displayname":
                entity.DisplayName = ExtractString(value);
                return true;
            case "nickname":
                entity.NickName = ExtractString(value);
                return true;
            case "active":
                entity.Active = ExtractBool(value);
                return true;
            case "title":
                entity.Title = ExtractString(value);
                return true;
            case "usertype":
                entity.UserType = ExtractString(value);
                return true;
            case "profileurl":
                entity.ProfileUrl = ExtractString(value);
                return true;
            case "preferredlanguage":
                entity.PreferredLanguage = ExtractString(value);
                return true;
            case "locale":
                entity.Locale = ExtractString(value);
                return true;
            case "timezone":
                entity.Timezone = ExtractString(value);
                return true;
            default:
                if (path.StartsWith("name."))
                {
                    return ApplyNameProperty(entity, path, value);
                }
                return false;
        }
    }

    private static bool RemoveScalarAttribute(UserEntity entity, string path)
    {
        switch (path)
        {
            case "externalid":
                entity.ExternalId = null;
                return true;
            case "displayname":
                entity.DisplayName = null;
                return true;
            case "nickname":
                entity.NickName = null;
                return true;
            case "profileurl":
                entity.ProfileUrl = null;
                return true;
            case "title":
                entity.Title = null;
                return true;
            case "usertype":
                entity.UserType = null;
                return true;
            case "preferredlanguage":
                entity.PreferredLanguage = null;
                return true;
            case "locale":
                entity.Locale = null;
                return true;
            case "timezone":
                entity.Timezone = null;
                return true;
            default:
                if (path.StartsWith("name."))
                {
                    return RemoveNameProperty(entity, path);
                }
                return false;
        }
    }

    private static bool RemoveNameProperty(UserEntity entity, string path)
    {
        switch (path)
        {
            case "name.formatted":
                entity.NameFormatted = null;
                return true;
            case "name.givenname":
                entity.FirstName = null;
                return true;
            case "name.familyname":
                entity.LastName = null;
                return true;
            case "name.middlename":
                entity.NameMiddleName = null;
                return true;
            case "name.honorificprefix":
                entity.NameHonorificPrefix = null;
                return true;
            case "name.honorificsuffix":
                entity.NameHonorificSuffix = null;
                return true;
            default:
                return false;
        }
    }

    private static bool ApplyNameProperty(UserEntity entity, string path, object? value)
    {
        var stringValue = ExtractString(value);

        switch (path)
        {
            case "name.formatted":
                entity.NameFormatted = stringValue;
                return true;
            case "name.givenname":
                entity.FirstName = stringValue;
                return true;
            case "name.familyname":
                entity.LastName = stringValue;
                return true;
            case "name.middlename":
                entity.NameMiddleName = stringValue;
                return true;
            case "name.honorificprefix":
                entity.NameHonorificPrefix = stringValue;
                return true;
            case "name.honorificsuffix":
                entity.NameHonorificSuffix = stringValue;
                return true;
            default:
                return false;
        }
    }

    private static (int? index, string? filter, string? subAttr) ParseFilteredPath(string path)
    {
        // Try to match "emails[primary eq true].value" or "emails[0].value"
        var match = Regex.Match(path, @"^\w+\[([^\]]+)\]\.(\w+)$");

        if (match.Success)
        {
            var filterPart = match.Groups[1].Value;
            var subAttr = match.Groups[2].Value;

            // Check if it's an index
            if (int.TryParse(filterPart, out var index))
            {
                return (index, null, subAttr);
            }
            else
            {
                // It's a filter expression
                return (null, filterPart, subAttr);
            }
        }

        // Try to match just the path without sub-attribute (for remove operations)
        match = Regex.Match(path, @"^\w+\[([^\]]+)\]$");
        if (match.Success)
        {
            var filterPart = match.Groups[1].Value;
            if (int.TryParse(filterPart, out var index))
            {
                return (index, null, null);
            }
            else
            {
                return (null, filterPart, null);
            }
        }

        return (null, null, null);
    }

    private static List<T> FindMatchingItems<T>(List<T> items, int? index, string? filter)
    {
        var result = new List<T>();

        if (index.HasValue)
        {
            // Direct index access
            if (index.Value >= 0 && index.Value < items.Count)
            {
                result.Add(items[index.Value]);
            }
            // If index doesn't exist, create it
            else if (index.Value == items.Count)
            {
                var newItem = Activator.CreateInstance<T>();
                items.Add(newItem);
                result.Add(newItem);
            }
        }
        else if (filter != null)
        {
            // Filter expression like "primary eq true" or "type eq \"work\""
            var filterLower = filter.ToLowerInvariant();

            foreach (var item in items)
            {
                if (MatchesFilter(item, filterLower))
                {
                    result.Add(item);
                }
            }

            // If no match and it's a primary filter, create one
            if (result.Count == 0 && filterLower.Contains("primary eq true"))
            {
                var newItem = Activator.CreateInstance<T>();
                if (newItem is EmailData email)
                    email.Primary = true;
                else if (newItem is PhoneNumberData phone)
                    phone.Primary = true;
                else if (newItem is AddressData address)
                    address.Primary = true;

                items.Add(newItem);
                result.Add(newItem);
            }
        }

        return result;
    }

    private static bool MatchesFilter<T>(T item, string filter)
    {
        // Simple filter matching
        if (filter.Contains("primary eq true"))
        {
            if (item is EmailData email && email.Primary)
                return true;
            if (item is PhoneNumberData phone && phone.Primary)
                return true;
            if (item is AddressData address && address.Primary)
                return true;
        }
        else if (filter.Contains("type eq"))
        {
            var typeMatch = Regex.Match(filter, @"type eq [""']?(\w+)[""']?");
            if (typeMatch.Success)
            {
                var typeValue = typeMatch.Groups[1].Value;
                if (item is EmailData email && email.Type?.Equals(typeValue, StringComparison.OrdinalIgnoreCase) == true)
                    return true;
                if (item is PhoneNumberData phone && phone.Type?.Equals(typeValue, StringComparison.OrdinalIgnoreCase) == true)
                    return true;
                if (item is AddressData address && address.Type?.Equals(typeValue, StringComparison.OrdinalIgnoreCase) == true)
                    return true;
            }
        }

        return false;
    }

    private static string? ExtractString(object? value)
    {
        if (value == null)
            return null;

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind == JsonValueKind.String ? jsonElement.GetString() : null;
        }

        return value.ToString();
    }

    private static bool ExtractBool(object? value)
    {
        if (value == null)
            return false;

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind == JsonValueKind.True;
        }

        if (value is bool boolValue)
            return boolValue;

        return bool.TryParse(value.ToString(), out var result) && result;
    }
}

