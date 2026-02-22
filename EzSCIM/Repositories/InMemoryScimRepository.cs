﻿using EzSCIM.Filtering;
using EzSCIM.Filtering.AST;
using EzSCIM.Helpers;
using EzSCIM.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace EzSCIM.Repositories
{
    /// <summary>
    /// In-memory implementation of SCIM repository for both Users and Groups.
    /// This is a reference implementation suitable for testing and development.
    /// </summary>
    public class InMemoryScimRepository : IScimRepository
    {
        private readonly ConcurrentDictionary<string, ScimUser> _users = new();
        private readonly ConcurrentDictionary<string, ScimGroup> _groups = new();

        #region User Operations

        public Task<ScimUser?> GetUserAsync(string id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<ScimUser?> GetUserByUserNameAsync(string userName)
        {
            var user = _users.Values.FirstOrDefault(u => 
                u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<ScimListResponse<ScimUser>> GetUsersAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100)
        {
            var users = _users.Values.AsEnumerable();

            if (filter != null)
            {
                users = users.Where(filter);
            }

            var usersList = users.ToList();
            var totalResults = usersList.Count;
            var pagedUsers = usersList.Skip(startIndex - 1).Take(count).ToList();

            return Task.FromResult(new ScimListResponse<ScimUser>
            {
                TotalResults = totalResults,
                StartIndex = startIndex,
                ItemsPerPage = pagedUsers.Count,
                Resources = pagedUsers
            });
        }

        public Task<ScimUser> CreateUserAsync(ScimUser user)
        {
            user.Id = Guid.NewGuid().ToString();
            user.Meta = new ScimMeta
            {
                ResourceType = "User",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Location = $"/scim/Users/{user.Id}"
            };

            _users[user.Id] = user;
            return Task.FromResult(user);
        }

        public Task<ScimUser?> UpdateUserAsync(string id, ScimUser user)
        {
            if (!_users.ContainsKey(id))
                return Task.FromResult<ScimUser?>(null);

            user.Id = id;
            user.Meta.LastModified = DateTime.UtcNow;
            user.Meta.Location = $"/scim/Users/{id}";
            user.Meta.ResourceType = "User";

            _users[id] = user;
            return Task.FromResult<ScimUser?>(user);
        }

        public Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest)
        {
            if (!_users.TryGetValue(id, out var user))
                return Task.FromResult<ScimUser?>(null);

            foreach (var operation in patchRequest.Operations)
            {
                ApplyUserPatchOperation(user, operation);
            }

            user.Meta.LastModified = DateTime.UtcNow;
            return Task.FromResult<ScimUser?>(user);
        }

        public Task<bool> DeleteUserAsync(string id)
        {
            return Task.FromResult(_users.TryRemove(id, out _));
        }

        #endregion

        #region Group Operations

        public Task<ScimGroup?> GetGroupAsync(string id)
        {
            _groups.TryGetValue(id, out var group);
            return Task.FromResult(group);
        }

        public Task<ScimGroup?> GetGroupByDisplayNameAsync(string displayName)
        {
            var group = _groups.Values.FirstOrDefault(g => 
                g.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(group);
        }

        public Task<ScimListResponse<ScimGroup>> GetGroupsAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100)
        {
            var groups = _groups.Values.AsEnumerable();

            if (filter != null)
            {
                groups = groups.Where(filter);
            }

            var groupsList = groups.ToList();
            var totalResults = groupsList.Count;
            var pagedGroups = groupsList.Skip(startIndex - 1).Take(count).ToList();

            return Task.FromResult(new ScimListResponse<ScimGroup>
            {
                TotalResults = totalResults,
                StartIndex = startIndex,
                ItemsPerPage = pagedGroups.Count,
                Resources = pagedGroups
            });
        }

        public Task<ScimGroup> CreateGroupAsync(ScimGroup group)
        {
            group.Id = Guid.NewGuid().ToString();
            group.Meta = new ScimMeta
            {
                ResourceType = "Group",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Location = $"/scim/Groups/{group.Id}"
            };

            _groups[group.Id] = group;
            return Task.FromResult(group);
        }

        public Task<ScimGroup?> UpdateGroupAsync(string id, ScimGroup group)
        {
            if (!_groups.ContainsKey(id))
                return Task.FromResult<ScimGroup?>(null);

            group.Id = id;
            group.Meta.LastModified = DateTime.UtcNow;
            group.Meta.Location = $"/scim/Groups/{id}";
            group.Meta.ResourceType = "Group";

            _groups[id] = group;
            return Task.FromResult<ScimGroup?>(group);
        }

        public Task<ScimGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest)
        {
            if (!_groups.TryGetValue(id, out var group))
                return Task.FromResult<ScimGroup?>(null);

            foreach (var operation in patchRequest.Operations)
            {
                ApplyGroupPatchOperation(group, operation);
            }

            group.Meta.LastModified = DateTime.UtcNow;
            return Task.FromResult<ScimGroup?>(group);
        }

        public Task<bool> DeleteGroupAsync(string id)
        {
            return Task.FromResult(_groups.TryRemove(id, out _));
        }

        #endregion

        #region PATCH Operations Helpers

        private void ApplyUserPatchOperation(ScimUser user, ScimPatchOperation operation)
        {
            var path = operation.Path?.Trim();
            var normalizedPath = path?.ToLowerInvariant() ?? string.Empty;
            var op = operation.Op.ToLowerInvariant();

            if (op == "replace" && operation.Value != null)
            {
                if (string.IsNullOrWhiteSpace(normalizedPath))
                {
                    ApplyUserPatchValueObject(user, operation.Value);
                    return;
                }

                if (normalizedPath == "name" && operation.Value is JsonElement nameElement && nameElement.ValueKind == JsonValueKind.Object)
                {
                    ApplyNameObject(user, nameElement);
                    return;
                }

                if (normalizedPath.StartsWith("emails") && normalizedPath.Contains("[primary eq true]"))
                {
                    ApplyPrimaryEmailValue(user, normalizedPath, operation.Value);
                    return;
                }

                if (normalizedPath.StartsWith("phonenumbers") && normalizedPath.Contains("[primary eq true]"))
                {
                    ApplyPrimaryPhoneValue(user, normalizedPath, operation.Value);
                    return;
                }

                if (normalizedPath.StartsWith("addresses") && normalizedPath.Contains("[primary eq true]"))
                {
                    ApplyPrimaryAddressValue(user, normalizedPath, operation.Value);
                    return;
                }

                if (normalizedPath == "active")
                {
                    user.Active = ExtractBooleanValue(operation.Value);
                }
                else if (normalizedPath == "displayname")
                {
                    user.DisplayName = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "nickname")
                {
                    user.NickName = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "profileurl")
                {
                    user.ProfileUrl = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "title")
                {
                    user.Title = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "usertype")
                {
                    user.UserType = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "preferredlanguage")
                {
                    user.PreferredLanguage = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "locale")
                {
                    user.Locale = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "timezone")
                {
                    user.Timezone = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "username")
                {
                    user.UserName = ExtractStringValue(operation.Value) ?? string.Empty;
                }
                else if (normalizedPath == "externalid")
                {
                    user.ExternalId = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "name.formatted")
                {
                    EnsureName(user).Formatted = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "name.givenname")
                {
                    EnsureName(user).GivenName = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "name.familyname")
                {
                    EnsureName(user).FamilyName = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "name.middlename")
                {
                    EnsureName(user).MiddleName = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "name.honorificprefix")
                {
                    EnsureName(user).HonorificPrefix = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "name.honorificsuffix")
                {
                    EnsureName(user).HonorificSuffix = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath.StartsWith("emails"))
                {
                    if (operation.Value is JsonElement jsonElement)
                    {
                        user.Emails = ParseEmails(jsonElement);
                    }
                }
                else if (normalizedPath.StartsWith("phonenumbers"))
                {
                    if (operation.Value is JsonElement jsonElement)
                    {
                        user.PhoneNumbers = ParsePhoneNumbers(jsonElement);
                    }
                }
                else if (normalizedPath.StartsWith("addresses"))
                {
                    if (operation.Value is JsonElement jsonElement)
                    {
                        user.Addresses = ParseAddresses(jsonElement);
                    }
                }
                else
                {
                    // Custom attributes
                    user.CustomAttributes[normalizedPath] = operation.Value;
                }
            }
            else if (op == "add" && operation.Value != null)
            {
                // When path is empty/null, "add" with a value object behaves like "replace" for each property
                if (string.IsNullOrWhiteSpace(normalizedPath))
                {
                    ApplyUserPatchValueObject(user, operation.Value);
                    return;
                }

                if (normalizedPath.StartsWith("emails"))
                {
                    if (operation.Value is JsonElement jsonElement)
                    {
                        var newEmails = ParseEmails(jsonElement);
                        foreach (var email in newEmails)
                        {
                            if (!user.Emails.Any(e => e.Value.Equals(email.Value, StringComparison.OrdinalIgnoreCase)))
                            {
                                user.Emails.Add(email);
                            }
                        }
                    }
                }
                else if (normalizedPath.StartsWith("phonenumbers"))
                {
                    if (operation.Value is JsonElement jsonElement)
                    {
                        var newPhones = ParsePhoneNumbers(jsonElement);
                        foreach (var phone in newPhones)
                        {
                            if (!user.PhoneNumbers.Any(p => p.Value.Equals(phone.Value, StringComparison.OrdinalIgnoreCase)))
                            {
                                user.PhoneNumbers.Add(phone);
                            }
                        }
                    }
                }
                else if (normalizedPath.StartsWith("addresses"))
                {
                    if (operation.Value is JsonElement jsonElement)
                    {
                        var newAddresses = ParseAddresses(jsonElement);
                        user.Addresses.AddRange(newAddresses);
                    }
                }
            }
            else if (op == "remove")
            {
                // Handle filtered remove operations like "members[value eq \"id\"]"
                if (!string.IsNullOrWhiteSpace(normalizedPath))
                {
                    var filteredPath = AttributeFilterHelper.ParseFilteredPath(normalizedPath);
                    if (filteredPath.HasValue)
                    {
                        var (arrayProperty, filterExpression, targetProperty) = filteredPath.Value;
                        
                        if (arrayProperty == "emails")
                        {
                            var toRemove = new List<ScimEmail>();
                            foreach (var email in user.Emails)
                            {
                                if (AttributeFilterHelper.EvaluateSimpleFilter(email, filterExpression))
                                {
                                    toRemove.Add(email);
                                }
                            }
                            foreach (var email in toRemove)
                            {
                                user.Emails.Remove(email);
                            }
                        }
                        else if (arrayProperty == "phonenumbers")
                        {
                            var toRemove = new List<ScimPhoneNumber>();
                            foreach (var phone in user.PhoneNumbers)
                            {
                                if (AttributeFilterHelper.EvaluateSimpleFilter(phone, filterExpression))
                                {
                                    toRemove.Add(phone);
                                }
                            }
                            foreach (var phone in toRemove)
                            {
                                user.PhoneNumbers.Remove(phone);
                            }
                        }
                        else if (arrayProperty == "addresses")
                        {
                            var toRemove = new List<ScimAddress>();
                            foreach (var address in user.Addresses)
                            {
                                if (AttributeFilterHelper.EvaluateSimpleFilter(address, filterExpression))
                                {
                                    toRemove.Add(address);
                                }
                            }
                            foreach (var address in toRemove)
                            {
                                user.Addresses.Remove(address);
                            }
                        }
                        return;
                    }

                    // Handle remove for simple scalar attributes
                    RemoveUserAttribute(user, normalizedPath);
                }

                // Handle simple path remove operations with value (e.g., remove specific emails)
                if (normalizedPath.StartsWith("emails"))
                {
                    if (operation.Value is JsonElement jsonElement)
                    {
                        var emailsToRemove = ParseEmails(jsonElement);
                        foreach (var email in emailsToRemove)
                        {
                            var existing = user.Emails.FirstOrDefault(e => e.Value.Equals(email.Value, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                user.Emails.Remove(existing);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes (sets to null/default) a simple scalar user attribute by path name.
        /// Per SCIM RFC 7644, "remove" on a single-valued attribute sets it to default/null.
        /// </summary>
        private void RemoveUserAttribute(ScimUser user, string normalizedPath)
        {
            switch (normalizedPath)
            {
                case "externalid":
                    user.ExternalId = null;
                    break;
                case "displayname":
                    user.DisplayName = null;
                    break;
                case "nickname":
                    user.NickName = null;
                    break;
                case "profileurl":
                    user.ProfileUrl = null;
                    break;
                case "title":
                    user.Title = null;
                    break;
                case "usertype":
                    user.UserType = null;
                    break;
                case "preferredlanguage":
                    user.PreferredLanguage = null;
                    break;
                case "locale":
                    user.Locale = null;
                    break;
                case "timezone":
                    user.Timezone = null;
                    break;
                case "active":
                    user.Active = false;
                    break;
                case "name":
                    user.Name = new ScimName();
                    break;
                case "name.formatted":
                    if (user.Name != null) user.Name.Formatted = null;
                    break;
                case "name.givenname":
                    if (user.Name != null) user.Name.GivenName = null;
                    break;
                case "name.familyname":
                    if (user.Name != null) user.Name.FamilyName = null;
                    break;
                case "name.middlename":
                    if (user.Name != null) user.Name.MiddleName = null;
                    break;
                case "name.honorificprefix":
                    if (user.Name != null) user.Name.HonorificPrefix = null;
                    break;
                case "name.honorificsuffix":
                    if (user.Name != null) user.Name.HonorificSuffix = null;
                    break;
                case "emails":
                    user.Emails.Clear();
                    break;
                case "phonenumbers":
                    user.PhoneNumbers.Clear();
                    break;
                case "addresses":
                    user.Addresses.Clear();
                    break;
                default:
                    // Remove from custom attributes if present
                    user.CustomAttributes.Remove(normalizedPath);
                    break;
            }
        }

        private void ApplyUserPatchValueObject(ScimUser user, object value)
        {
            if (value is JsonElement element && element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    ApplyUserPatchValueProperty(user, property.Name, property.Value);
                }
            }
            else if (value is Dictionary<string, object> dict)
            {
                foreach (var entry in dict)
                {
                    ApplyUserPatchValueProperty(user, entry.Key, entry.Value);
                }
            }
        }

        private void ApplyUserPatchValueProperty(ScimUser user, string propertyName, object? propertyValue)
        {
            if (propertyValue is JsonElement jsonElement)
            {
                ApplyUserPatchValueProperty(user, propertyName, jsonElement);
                return;
            }

            var normalizedName = propertyName.ToLowerInvariant();
            var stringValue = ExtractStringValue(propertyValue);

            switch (normalizedName)
            {
                case "externalid":
                    user.ExternalId = stringValue;
                    return;
                case "username":
                    user.UserName = stringValue ?? string.Empty;
                    return;
                case "displayname":
                    user.DisplayName = stringValue;
                    return;
                case "nickname":
                    user.NickName = stringValue;
                    return;
                case "profileurl":
                    user.ProfileUrl = stringValue;
                    return;
                case "title":
                    user.Title = stringValue;
                    return;
                case "usertype":
                    user.UserType = stringValue;
                    return;
                case "preferredlanguage":
                    user.PreferredLanguage = stringValue;
                    return;
                case "locale":
                    user.Locale = stringValue;
                    return;
                case "timezone":
                    user.Timezone = stringValue;
                    return;
                case "active":
                    user.Active = ExtractBooleanValue(propertyValue);
                    return;
            }

            if (normalizedName.StartsWith("name."))
            {
                ApplyNameProperty(user, normalizedName, stringValue);
                return;
            }

            user.CustomAttributes[normalizedName] = propertyValue ?? string.Empty;
        }

        private void ApplyUserPatchValueProperty(ScimUser user, string propertyName, JsonElement jsonValue)
        {
            var normalizedName = propertyName.ToLowerInvariant();

            switch (normalizedName)
            {
                case "externalid":
                    user.ExternalId = ExtractStringValue(jsonValue);
                    return;
                case "username":
                    user.UserName = ExtractStringValue(jsonValue) ?? string.Empty;
                    return;
                case "displayname":
                    user.DisplayName = ExtractStringValue(jsonValue);
                    return;
                case "nickname":
                    user.NickName = ExtractStringValue(jsonValue);
                    return;
                case "profileurl":
                    user.ProfileUrl = ExtractStringValue(jsonValue);
                    return;
                case "title":
                    user.Title = ExtractStringValue(jsonValue);
                    return;
                case "usertype":
                    user.UserType = ExtractStringValue(jsonValue);
                    return;
                case "preferredlanguage":
                    user.PreferredLanguage = ExtractStringValue(jsonValue);
                    return;
                case "locale":
                    user.Locale = ExtractStringValue(jsonValue);
                    return;
                case "timezone":
                    user.Timezone = ExtractStringValue(jsonValue);
                    return;
                case "active":
                    user.Active = ExtractBooleanValue(jsonValue);
                    return;
                case "name":
                    if (jsonValue.ValueKind == JsonValueKind.Object)
                    {
                        ApplyNameObject(user, jsonValue);
                        return;
                    }
                    break;
                case "emails":
                    user.Emails = ParseEmails(jsonValue);
                    return;
                case "phonenumbers":
                    user.PhoneNumbers = ParsePhoneNumbers(jsonValue);
                    return;
                case "addresses":
                    user.Addresses = ParseAddresses(jsonValue);
                    return;
            }

            if (normalizedName.StartsWith("name."))
            {
                ApplyNameProperty(user, normalizedName, ExtractStringValue(jsonValue));
                return;
            }

            user.CustomAttributes[normalizedName] = jsonValue;
        }

        private void ApplyNameObject(ScimUser user, JsonElement nameElement)
        {
            var name = EnsureName(user);

            if (nameElement.TryGetProperty("formatted", out var formatted))
                name.Formatted = ExtractStringValue(formatted);
            if (nameElement.TryGetProperty("familyName", out var familyName))
                name.FamilyName = ExtractStringValue(familyName);
            if (nameElement.TryGetProperty("givenName", out var givenName))
                name.GivenName = ExtractStringValue(givenName);
            if (nameElement.TryGetProperty("middleName", out var middleName))
                name.MiddleName = ExtractStringValue(middleName);
            if (nameElement.TryGetProperty("honorificPrefix", out var honorificPrefix))
                name.HonorificPrefix = ExtractStringValue(honorificPrefix);
            if (nameElement.TryGetProperty("honorificSuffix", out var honorificSuffix))
                name.HonorificSuffix = ExtractStringValue(honorificSuffix);
        }

        private void ApplyNameProperty(ScimUser user, string normalizedName, string? value)
        {
            var name = EnsureName(user);

            switch (normalizedName)
            {
                case "name.formatted":
                    name.Formatted = value;
                    break;
                case "name.familyname":
                    name.FamilyName = value;
                    break;
                case "name.givenname":
                    name.GivenName = value;
                    break;
                case "name.middlename":
                    name.MiddleName = value;
                    break;
                case "name.honorificprefix":
                    name.HonorificPrefix = value;
                    break;
                case "name.honorificsuffix":
                    name.HonorificSuffix = value;
                    break;
            }
        }

        private ScimName EnsureName(ScimUser user)
        {
            user.Name ??= new ScimName();
            return user.Name;
        }

        private void ApplyPrimaryEmailValue(ScimUser user, string normalizedPath, object? value)
        {
            var newValue = ExtractStringValue(value);
            if (string.IsNullOrWhiteSpace(newValue))
                return;

            var email = user.Emails.FirstOrDefault(e => e.Primary);
            if (email == null)
            {
                email = new ScimEmail { Primary = true };
                user.Emails.Add(email);
            }

            if (normalizedPath.EndsWith(".value"))
            {
                email.Value = newValue;
            }
            else if (normalizedPath.EndsWith(".type"))
            {
                email.Type = newValue;
            }
        }

        private void ApplyPrimaryPhoneValue(ScimUser user, string normalizedPath, object? value)
        {
            var newValue = ExtractStringValue(value);
            if (string.IsNullOrWhiteSpace(newValue))
                return;

            var phone = user.PhoneNumbers.FirstOrDefault(p => p.Primary);
            if (phone == null)
            {
                phone = new ScimPhoneNumber { Primary = true };
                user.PhoneNumbers.Add(phone);
            }

            if (normalizedPath.EndsWith(".value"))
            {
                phone.Value = newValue;
            }
            else if (normalizedPath.EndsWith(".type"))
            {
                phone.Type = newValue;
            }
        }

        private void ApplyPrimaryAddressValue(ScimUser user, string normalizedPath, object? value)
        {
            var newValue = ExtractStringValue(value);
            if (string.IsNullOrWhiteSpace(newValue))
                return;

            var address = user.Addresses.FirstOrDefault(a => a.Primary);
            if (address == null)
            {
                address = new ScimAddress { Primary = true };
                user.Addresses.Add(address);
            }

            if (normalizedPath.EndsWith(".formatted"))
            {
                address.Formatted = newValue;
            }
            else if (normalizedPath.EndsWith(".streetaddress"))
            {
                address.StreetAddress = newValue;
            }
            else if (normalizedPath.EndsWith(".locality"))
            {
                address.Locality = newValue;
            }
            else if (normalizedPath.EndsWith(".region"))
            {
                address.Region = newValue;
            }
            else if (normalizedPath.EndsWith(".postalcode"))
            {
                address.PostalCode = newValue;
            }
            else if (normalizedPath.EndsWith(".country"))
            {
                address.Country = newValue;
            }
        }

        private void ApplyGroupPatchOperation(ScimGroup group, ScimPatchOperation operation)
        {
            var path = operation.Path?.Trim();
            var normalizedPath = path?.ToLowerInvariant() ?? string.Empty;
            var op = operation.Op.ToLowerInvariant();

            if (op == "replace" && operation.Value != null)
            {
                if (string.IsNullOrWhiteSpace(normalizedPath))
                {
                    ApplyGroupPatchValueObject(group, operation.Value);
                    return;
                }

                if (normalizedPath == "externalid")
                {
                    group.ExternalId = ExtractStringValue(operation.Value);
                }
                else if (normalizedPath == "displayname")
                {
                    group.DisplayName = ExtractStringValue(operation.Value) ?? string.Empty;
                }
                else if (normalizedPath == "members")
                {
                    // Replace the entire members list
                    group.Members = ParseMembers(operation.Value);
                }
            }
            else if (op == "add" && operation.Value != null)
            {
                if (string.IsNullOrWhiteSpace(normalizedPath) || normalizedPath == "members")
                {
                    var members = ParseMembers(operation.Value);
                    var currentMembers = EnsureMembers(group);
                    foreach (var member in members)
                    {
                        if (!currentMembers.Any(m => m.Value == member.Value))
                            currentMembers.Add(member);
                    }
                }
            }
            else if (op == "remove" && operation.Path != null)
            {
                var currentMembers = EnsureMembers(group);
                // Handle "members[value eq \"id\"]" case
                if (operation.Path.Contains("[value eq", StringComparison.OrdinalIgnoreCase))
                {
                    var startIdx = operation.Path.IndexOf("\"") + 1;
                    var endIdx = operation.Path.LastIndexOf("\"");
                    if (startIdx > 0 && endIdx > startIdx)
                    {
                        var memberId = operation.Path.Substring(startIdx, endIdx - startIdx);
                        var existing = currentMembers.FirstOrDefault(m => m.Value == memberId);
                        if (existing != null)
                            currentMembers.Remove(existing);
                    }
                }
                // Handle case where members are in Value
                else if (operation.Value != null)
                {
                    var members = ParseMembers(operation.Value);
                    foreach (var member in members)
                    {
                        var existing = currentMembers.FirstOrDefault(m => m.Value == member.Value);
                        if (existing != null)
                            currentMembers.Remove(existing);
                    }
                }
            }
        }

        /// <summary>
        /// Ensures the group's Members list is initialized (not null) and returns it.
        /// </summary>
        private List<ScimMember> EnsureMembers(ScimGroup group)
        {
            group.Members ??= new List<ScimMember>();
            return group.Members;
        }

        private void ApplyGroupPatchValueObject(ScimGroup group, object value)
        {
            if (value is JsonElement element && element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    var normalizedName = property.Name.ToLowerInvariant();
                    if (normalizedName == "externalid")
                    {
                        group.ExternalId = ExtractStringValue(property.Value);
                    }
                    else if (normalizedName == "displayname")
                    {
                        group.DisplayName = ExtractStringValue(property.Value) ?? string.Empty;
                    }
                }
            }
        }

        private List<ScimEmail> ParseEmails(JsonElement jsonElement)
        {
            var emails = new List<ScimEmail>();
            
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    if (item.TryGetProperty("value", out var valueProperty))
                    {
                        var email = new ScimEmail { Value = valueProperty.GetString() ?? string.Empty };
                        if (item.TryGetProperty("type", out var type))
                            email.Type = type.GetString();
                        if (item.TryGetProperty("primary", out var primary))
                            email.Primary = primary.GetBoolean();
                        emails.Add(email);
                    }
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                if (jsonElement.TryGetProperty("value", out var valueProperty))
                {
                    var email = new ScimEmail { Value = valueProperty.GetString() ?? string.Empty };
                    if (jsonElement.TryGetProperty("type", out var type))
                        email.Type = type.GetString();
                    if (jsonElement.TryGetProperty("primary", out var primary))
                        email.Primary = primary.GetBoolean();
                    emails.Add(email);
                }
            }

            return emails;
        }

        private List<ScimPhoneNumber> ParsePhoneNumbers(JsonElement jsonElement)
        {
            var phoneNumbers = new List<ScimPhoneNumber>();
            
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    if (item.TryGetProperty("value", out var valueProperty))
                    {
                        var phone = new ScimPhoneNumber { Value = valueProperty.GetString() ?? string.Empty };
                        if (item.TryGetProperty("type", out var type))
                            phone.Type = type.GetString();
                        if (item.TryGetProperty("primary", out var primary))
                            phone.Primary = primary.GetBoolean();
                        phoneNumbers.Add(phone);
                    }
                }
            }

            return phoneNumbers;
        }

        private List<ScimAddress> ParseAddresses(JsonElement jsonElement)
        {
            var addresses = new List<ScimAddress>();
            
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    var address = new ScimAddress();
                    if (item.TryGetProperty("streetAddress", out var streetAddress))
                        address.StreetAddress = streetAddress.GetString();
                    if (item.TryGetProperty("locality", out var locality))
                        address.Locality = locality.GetString();
                    if (item.TryGetProperty("region", out var region))
                        address.Region = region.GetString();
                    if (item.TryGetProperty("postalCode", out var postalCode))
                        address.PostalCode = postalCode.GetString();
                    if (item.TryGetProperty("country", out var country))
                        address.Country = country.GetString();
                    if (item.TryGetProperty("type", out var type))
                        address.Type = type.GetString();
                    if (item.TryGetProperty("primary", out var primary))
                        address.Primary = primary.GetBoolean();
                    addresses.Add(address);
                }
            }

            return addresses;
        }

        private List<ScimMember> ParseMembers(object value)
        {
            var members = new List<ScimMember>();
            
            // Handle List<Dictionary<string, string>>
            if (value is List<Dictionary<string, string>> dictList)
            {
                foreach (var dict in dictList)
                {
                    var member = new ScimMember();
                    if (dict.TryGetValue("value", out var memberValue))
                        member.Value = memberValue;
                    if (dict.TryGetValue("display", out var display))
                        member.Display = display;
                    if (!string.IsNullOrEmpty(member.Value))
                        members.Add(member);
                }
            }
            // Handle JsonElement
            else if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    if (item.TryGetProperty("value", out var valueProperty))
                    {
                        var member = new ScimMember { Value = valueProperty.GetString() ?? string.Empty };
                        if (item.TryGetProperty("display", out var display))
                            member.Display = display.GetString();
                        members.Add(member);
                    }
                }
            }

            return members;
        }

        private bool ExtractBooleanValue(object? value)
        {
            if (value == null)
                return false;

            if (value is bool boolValue)
                return boolValue;

            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.String => bool.TryParse(jsonElement.GetString(), out var result) ? result : false,
                    JsonValueKind.Number => jsonElement.GetInt32() != 0,
                    _ => false
                };
            }

            if (value is string stringValue)
                return bool.TryParse(stringValue, out var result) ? result : false;

            return false;
        }

        private string? ExtractStringValue(object? value)
        {
            if (value == null)
                return null;

            if (value is string stringValue)
                return stringValue;

            if (value is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.String)
                    return jsonElement.GetString();
                if (jsonElement.ValueKind == JsonValueKind.Number)
                    return jsonElement.ToString();
                if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                    return jsonElement.GetBoolean().ToString();
            }

            return value.ToString();
        }

        #endregion
    }
}

