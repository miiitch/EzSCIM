using ScimAPI.Filtering;
using ScimAPI.Filtering.AST;
using ScimAPI.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ScimAPI.Repositories
{
    public class InMemoryScimRepository : IScimRepository
    {
        private readonly ConcurrentDictionary<string, ScimUser> _users = new();
        private readonly ConcurrentDictionary<string, ScimGroup> _groups = new();

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


        // Obsolete string-based filter methods removed - now using FilterExpression AST pattern
        // See ApplyUserFilter(IEnumerable<ScimUser>, FilterExpression) at line ~792

        private void ApplyUserPatchOperation(ScimUser user, ScimPatchOperation operation)
        {
            var path = operation.Path?.ToLower() ?? string.Empty;
            var op = operation.Op.ToLower();

            if (op == "replace" && operation.Value != null)
            {
                if (path == "active")
                {
                    user.Active = Convert.ToBoolean(operation.Value);
                }
                else if (path == "displayname")
                {
                    user.DisplayName = operation.Value.ToString();
                }
                else if (path == "username")
                {
                    user.UserName = operation.Value.ToString() ?? string.Empty;
                }
                else if (path == "externalid")
                {
                    user.ExternalId = operation.Value.ToString() ?? string.Empty;
                }
                else if (path == "name.givenname")
                {
                    user.Name.GivenName = operation.Value.ToString();
                }
                else if (path == "name.familyname")
                {
                    user.Name.FamilyName = operation.Value.ToString();
                }
                else if (path == "title")
                {
                    user.Title = operation.Value.ToString();
                }
                else if (path.StartsWith("emails"))
                {
                    // Gérer les emails
                    if (operation.Value is JsonElement jsonElement)
                    {
                        user.Emails = ParseEmails(jsonElement);
                    }
                }
                else if (path.StartsWith("phonenumbers"))
                {
                    // Gérer les numéros de téléphone
                    if (operation.Value is JsonElement jsonElement)
                    {
                        user.PhoneNumbers = ParsePhoneNumbers(jsonElement);
                    }
                }
                else if (path.StartsWith("addresses"))
                {
                    // Gérer les adresses
                    if (operation.Value is JsonElement jsonElement)
                    {
                        user.Addresses = ParseAddresses(jsonElement);
                    }
                }
                else
                {
                    // Attributs personnalisés
                    user.CustomAttributes[path] = operation.Value;
                }
            }
            else if (op == "add" && operation.Value != null)
            {
                // Ajouter des valeurs (par exemple des emails ou groupes)
                if (path.StartsWith("emails"))
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
            }
            else if (op == "remove")
            {
                // Supprimer des valeurs
                if (path.StartsWith("emails"))
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

        private void ApplyGroupPatchOperation(ScimGroup group, ScimPatchOperation operation)
        {
            var op = operation.Op.ToLower();

            if (op == "add" && operation.Value != null)
            {
                var members = ParseMembers(operation.Value);
                foreach (var member in members)
                {
                    if (!group.Members.Any(m => m.Value == member.Value))
                        group.Members.Add(member);
                }
            }
            else if (op == "remove" && operation.Path != null)
            {
                // Gérer le cas "members[value eq "id"]"
                if (operation.Path.Contains("[value eq", StringComparison.OrdinalIgnoreCase))
                {
                    var startIdx = operation.Path.IndexOf("\"") + 1;
                    var endIdx = operation.Path.LastIndexOf("\"");
                    if (startIdx > 0 && endIdx > startIdx)
                    {
                        var memberId = operation.Path.Substring(startIdx, endIdx - startIdx);
                        var existing = group.Members.FirstOrDefault(m => m.Value == memberId);
                        if (existing != null)
                            group.Members.Remove(existing);
                    }
                }
                // Gérer le cas où les membres sont dans Value
                else if (operation.Value != null)
                {
                    var members = ParseMembers(operation.Value);
                    foreach (var member in members)
                    {
                        var existing = group.Members.FirstOrDefault(m => m.Value == member.Value);
                        if (existing != null)
                            group.Members.Remove(existing);
                    }
                }
            }
        }

        private List<ScimMember> ParseMembers(object value)
        {
            var members = new List<ScimMember>();
            
            // Gérer le cas List<Dictionary<string, string>>
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
            // Gérer le cas JsonElement
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

        // Filter methods moved to FilterExtensions class for reusability
        // Use: users.Where(filter) or groups.Where(filter)
    }
}


