using ScimAPI.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ScimAPI.Repositories
{
    public class InMemoryScimRepository : IScimRepository
    {
        private readonly ConcurrentDictionary<string, ScimUser> _users = new();
        private readonly ConcurrentDictionary<string, ScimGroup> _groups = new();
        private readonly List<ScimSchema> _customSchemas = new();
        private readonly object _schemaLock = new();

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

        public Task<ScimListResponse<ScimUser>> GetUsersAsync(string? filter = null, int startIndex = 1, int count = 100)
        {
            var users = _users.Values.AsEnumerable();

            if (!string.IsNullOrEmpty(filter))
            {
                users = ApplyUserFilter(users, filter);
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

        public Task<ScimListResponse<ScimGroup>> GetGroupsAsync(string? filter = null, int startIndex = 1, int count = 100)
        {
            var groups = _groups.Values.AsEnumerable();

            if (!string.IsNullOrEmpty(filter))
            {
                groups = ApplyGroupFilter(groups, filter);
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

        public Task<List<ScimSchema>> GetCustomSchemasAsync()
        {
            lock (_schemaLock)
            {
                return Task.FromResult(new List<ScimSchema>(_customSchemas));
            }
        }

        public Task AddCustomSchemaAsync(ScimSchema schema)
        {
            lock (_schemaLock)
            {
                var existing = _customSchemas.FirstOrDefault(s => s.Id == schema.Id);
                if (existing != null)
                    _customSchemas.Remove(existing);
                _customSchemas.Add(schema);
            }
            return Task.CompletedTask;
        }

        private IEnumerable<ScimUser> ApplyUserFilter(IEnumerable<ScimUser> users, string filter)
        {
            // Suppression des parenthèses externes si présentes (IMPORTANT: avant les opérateurs logiques)
            filter = filter.Trim();
            while (filter.StartsWith("(") && filter.EndsWith(")"))
            {
                // Vérifier que ce sont bien des parenthèses externes correspondantes
                int depth = 0;
                bool isOuterParenthesis = true;
                for (int i = 0; i < filter.Length - 1; i++)
                {
                    if (filter[i] == '(') depth++;
                    else if (filter[i] == ')') depth--;
                    if (depth == 0)
                    {
                        isOuterParenthesis = false;
                        break;
                    }
                }
                if (isOuterParenthesis)
                {
                    filter = filter.Substring(1, filter.Length - 2).Trim();
                }
                else
                {
                    break;
                }
            }

            // Gestion des opérateurs logiques AND
            if (filter.Contains(" and ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = SplitFilterByLogicalOperator(filter, "and");
                var result = users;
                foreach (var part in parts)
                {
                    result = ApplyUserFilter(result, part.Trim());
                }
                return result;
            }

            // Gestion des opérateurs logiques OR
            if (filter.Contains(" or ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = SplitFilterByLogicalOperator(filter, "or");
                var usersList = users.ToList();
                var result = new List<ScimUser>();
                foreach (var part in parts)
                {
                    result.AddRange(ApplyUserFilter(usersList, part.Trim()));
                }
                return result.Distinct();
            }

            // Gestion de l'opérateur NOT
            if (filter.StartsWith("not ", StringComparison.OrdinalIgnoreCase))
            {
                var innerFilter = filter.Substring(4).Trim();
                var usersList = users.ToList();
                var filtered = ApplyUserFilter(usersList, innerFilter).ToList();
                return usersList.Where(u => !filtered.Contains(u));
            }


            // userName
            if (filter.Contains("userName eq", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.UserName.Equals(value, StringComparison.OrdinalIgnoreCase));
            }
            if (filter.Contains("userName sw", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.UserName.StartsWith(value, StringComparison.OrdinalIgnoreCase));
            }
            if (filter.Contains("userName co", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.UserName.Contains(value, StringComparison.OrdinalIgnoreCase));
            }

            // externalId
            if (filter.Contains("externalId eq", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.ExternalId.Equals(value, StringComparison.OrdinalIgnoreCase));
            }
            if (filter.Contains("externalId sw", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.ExternalId.StartsWith(value, StringComparison.OrdinalIgnoreCase));
            }

            // displayName
            if (filter.Contains("displayName eq", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.DisplayName?.Equals(value, StringComparison.OrdinalIgnoreCase) == true);
            }
            if (filter.Contains("displayName co", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.DisplayName?.Contains(value, StringComparison.OrdinalIgnoreCase) == true);
            }

            // active
            if (filter.Contains("active eq", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                var isActive = bool.Parse(value);
                return users.Where(u => u.Active == isActive);
            }

            // name.givenName
            if (filter.Contains("name.givenName eq", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.Name.GivenName?.Equals(value, StringComparison.OrdinalIgnoreCase) == true);
            }
            if (filter.Contains("name.givenName co", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.Name.GivenName?.Contains(value, StringComparison.OrdinalIgnoreCase) == true);
            }

            // name.familyName
            if (filter.Contains("name.familyName eq", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.Name.FamilyName?.Equals(value, StringComparison.OrdinalIgnoreCase) == true);
            }
            if (filter.Contains("name.familyName co", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.Name.FamilyName?.Contains(value, StringComparison.OrdinalIgnoreCase) == true);
            }

            // emails[type eq "work"].value
            if (filter.Contains("emails", StringComparison.OrdinalIgnoreCase) && filter.Contains("value", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return users.Where(u => u.Emails.Any(e => e.Value.Equals(value, StringComparison.OrdinalIgnoreCase)));
            }

            // Opérateur pr (present) - vérifie si un attribut est présent
            if (filter.Contains(" pr", StringComparison.OrdinalIgnoreCase))
            {
                var attribute = filter.Replace(" pr", "", StringComparison.OrdinalIgnoreCase).Trim();
                if (attribute.Equals("userName", StringComparison.OrdinalIgnoreCase))
                    return users.Where(u => !string.IsNullOrEmpty(u.UserName));
                if (attribute.Equals("displayName", StringComparison.OrdinalIgnoreCase))
                    return users.Where(u => !string.IsNullOrEmpty(u.DisplayName));
                if (attribute.Equals("externalId", StringComparison.OrdinalIgnoreCase))
                    return users.Where(u => !string.IsNullOrEmpty(u.ExternalId));
            }

            return users;
        }

        private List<string> SplitFilterByLogicalOperator(string filter, string logicalOperator)
        {
            var parts = new List<string>();
            var current = "";
            var depth = 0;
            var keyword = $" {logicalOperator} ";
            
            for (int i = 0; i < filter.Length; i++)
            {
                if (filter[i] == '(')
                    depth++;
                else if (filter[i] == ')')
                    depth--;

                if (depth == 0 && i + keyword.Length <= filter.Length)
                {
                    var substring = filter.Substring(i, keyword.Length);
                    if (substring.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        parts.Add(current.Trim());
                        current = "";
                        i += keyword.Length - 1;
                        continue;
                    }
                }

                current += filter[i];
            }

            if (!string.IsNullOrWhiteSpace(current))
                parts.Add(current.Trim());

            return parts;
        }

        private IEnumerable<ScimGroup> ApplyGroupFilter(IEnumerable<ScimGroup> groups, string filter)
        {
            // Suppression des parenthèses externes si présentes (IMPORTANT: avant les opérateurs logiques)
            filter = filter.Trim();
            while (filter.StartsWith("(") && filter.EndsWith(")"))
            {
                int depth = 0;
                bool isOuterParenthesis = true;
                for (int i = 0; i < filter.Length - 1; i++)
                {
                    if (filter[i] == '(') depth++;
                    else if (filter[i] == ')') depth--;
                    if (depth == 0)
                    {
                        isOuterParenthesis = false;
                        break;
                    }
                }
                if (isOuterParenthesis)
                {
                    filter = filter.Substring(1, filter.Length - 2).Trim();
                }
                else
                {
                    break;
                }
            }

            // Gestion des opérateurs logiques AND
            if (filter.Contains(" and ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = SplitFilterByLogicalOperator(filter, "and");
                var result = groups;
                foreach (var part in parts)
                {
                    result = ApplyGroupFilter(result, part.Trim());
                }
                return result;
            }

            // Gestion des opérateurs logiques OR
            if (filter.Contains(" or ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = SplitFilterByLogicalOperator(filter, "or");
                var groupsList = groups.ToList();
                var result = new List<ScimGroup>();
                foreach (var part in parts)
                {
                    result.AddRange(ApplyGroupFilter(groupsList, part.Trim()));
                }
                return result.Distinct();
            }


            // displayName
            if (filter.Contains("displayName eq", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return groups.Where(g => g.DisplayName.Equals(value, StringComparison.OrdinalIgnoreCase));
            }
            if (filter.Contains("displayName co", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return groups.Where(g => g.DisplayName.Contains(value, StringComparison.OrdinalIgnoreCase));
            }
            if (filter.Contains("displayName sw", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return groups.Where(g => g.DisplayName.StartsWith(value, StringComparison.OrdinalIgnoreCase));
            }

            // externalId
            if (filter.Contains("externalId eq", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return groups.Where(g => g.ExternalId.Equals(value, StringComparison.OrdinalIgnoreCase));
            }

            // members - recherche par membre
            if (filter.Contains("members", StringComparison.OrdinalIgnoreCase) && filter.Contains("value", StringComparison.OrdinalIgnoreCase))
            {
                var value = ExtractFilterValue(filter);
                return groups.Where(g => g.Members.Any(m => m.Value.Equals(value, StringComparison.OrdinalIgnoreCase)));
            }

            // Opérateur pr (present)
            if (filter.Contains(" pr", StringComparison.OrdinalIgnoreCase))
            {
                var attribute = filter.Replace(" pr", "", StringComparison.OrdinalIgnoreCase).Trim();
                if (attribute.Equals("displayName", StringComparison.OrdinalIgnoreCase))
                    return groups.Where(g => !string.IsNullOrEmpty(g.DisplayName));
                if (attribute.Equals("externalId", StringComparison.OrdinalIgnoreCase))
                    return groups.Where(g => !string.IsNullOrEmpty(g.ExternalId));
            }

            return groups;
        }

        private string ExtractFilterValue(string filter)
        {
            // Chercher la valeur entre guillemets
            var startIndex = filter.IndexOf('"');
            var endIndex = filter.LastIndexOf('"');
            if (startIndex >= 0 && endIndex > startIndex)
                return filter.Substring(startIndex + 1, endIndex - startIndex - 1);
            
            // Si pas de guillemets, extraire la valeur après l'opérateur (pour les booléens)
            var operators = new[] { " eq ", " ne ", " co ", " sw ", " ew ", " gt ", " ge ", " lt ", " le " };
            foreach (var op in operators)
            {
                var opIndex = filter.IndexOf(op, StringComparison.OrdinalIgnoreCase);
                if (opIndex >= 0)
                {
                    var valueStart = opIndex + op.Length;
                    var value = filter.Substring(valueStart).Trim();
                    // Nettoyer les parenthèses de fin
                    value = value.TrimEnd(')').Trim();
                    return value;
                }
            }
            
            return string.Empty;
        }

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
    }
}

