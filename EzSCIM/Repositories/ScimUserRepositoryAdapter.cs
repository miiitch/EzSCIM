using EzSCIM.Filtering;
using EzSCIM.Filtering.AST;
using EzSCIM.Models;
using System.Reflection;
using EzSCIM.Attributes;
using EzSCIM.Constants;
using EzSCIM.DataRepositories;

namespace EzSCIM.Repositories
{
    /// <summary>
    /// Adapter that bridges IUserDataRepository to IScimUserRepository.
    /// Maps between TUser (your domain model) and ScimUser using [ScimProperty] attributes.
    /// Uses IScimFilterTranslator to execute filters server-side on IQueryable.
    /// </summary>
    /// <typeparam name="TUser">Your user class annotated with [ScimProperty] attributes</typeparam>
    public class ScimUserRepositoryAdapter<TUser> : IScimUserRepository<ScimUser> 
        where TUser : class
    {
        private readonly IUserDataRepository<TUser> _dataRepository;
        private readonly IScimFilterTranslator<TUser> _filterTranslator;
        private readonly UserMapper<TUser> _mapper;

        public ScimUserRepositoryAdapter(
            IUserDataRepository<TUser> dataRepository,
            IScimFilterTranslator<TUser> filterTranslator)
        {
            _dataRepository = dataRepository;
            _filterTranslator = filterTranslator;
            _mapper = new UserMapper<TUser>();
        }

        public async Task<ScimUser?> GetUserAsync(string id)
        {
            var user = await _dataRepository.GetAsync(id);
            return user == null ? null : _mapper.ToScimUser(user);
        }

        public Task<ScimUser?> GetUserByUserNameAsync(string userName)
        {
            // Use SCIM filter translator to find by userName
            var filter = new ComparisonFilter(
                ScimAttributeNames.User.UserName, 
                FilterOperator.Equals, 
                new StringValue(userName));
            var predicate = _filterTranslator.BuildPredicate(filter);
            
            if (predicate == null)
                return Task.FromResult<ScimUser?>(null);

            var user = _dataRepository.Query().Where(predicate).FirstOrDefault();
            var result = user == null ? null : _mapper.ToScimUser(user);
            return Task.FromResult(result);
        }
        public Task<ScimListResponse<ScimUser>> GetUsersAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100)
        {
            // Start with queryable source
            var query = _dataRepository.Query();

            // Apply SCIM filter using translator (server-side filtering)
            if (filter != null)
            {
                query = _filterTranslator.Apply(query, filter);
            }

            // Get total count before pagination
            var totalResults = query.Count();

            // Apply pagination
            var users = query
                .Skip(startIndex - 1)
                .Take(count)
                .ToList();

            // Map to ScimUser
            var scimUsers = users.Select(u => _mapper.ToScimUser(u)).ToList();

            var response = new ScimListResponse<ScimUser>
            {
                TotalResults = totalResults,
                StartIndex = startIndex,
                ItemsPerPage = scimUsers.Count,
                Resources = scimUsers
            };

            return Task.FromResult(response);
        }

        public async Task<ScimUser> CreateUserAsync(ScimUser user)
        {
            var domainUser = _mapper.FromScimUser(user);
            var created = await _dataRepository.CreateAsync(domainUser);
            return _mapper.ToScimUser(created);
        }

        public async Task<ScimUser?> UpdateUserAsync(string id, ScimUser user)
        {
            var domainUser = _mapper.FromScimUser(user);
            var updated = await _dataRepository.UpdateAsync(id, domainUser);
            return updated == null ? null : _mapper.ToScimUser(updated);
        }

        public Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest)
        {
            // TODO: Implement PATCH mapping
            throw new NotImplementedException("PATCH operations require custom implementation per domain model");
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _dataRepository.DeleteAsync(id);
        }
    }

    /// <summary>
    /// Maps between TUser (domain model) and ScimUser using [ScimProperty] attributes.
    /// </summary>
    internal class UserMapper<TUser> where TUser : class
    {
        private readonly Dictionary<string, PropertyInfo> _scimPropertyMap;
        private readonly Dictionary<string, PropertyInfo> _domainPropertyMap;

        public UserMapper()
        {
            _scimPropertyMap = BuildPropertyMap(typeof(ScimUser));
            _domainPropertyMap = BuildPropertyMap(typeof(TUser));
        }

        /// <summary>
        /// Builds a map of SCIM attribute names to PropertyInfo using [ScimProperty] attributes.
        /// </summary>
        private Dictionary<string, PropertyInfo> BuildPropertyMap(Type type)
        {
            var map = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                var scimAttr = prop.GetCustomAttribute<ScimPropertyAttribute>();
                if (scimAttr != null)
                {
                    map[scimAttr.Name] = prop;
                }
                else
                {
                    // Fallback: use property name itself
                    map[prop.Name] = prop;
                }
            }

            return map;
        }

        /// <summary>
        /// Converts TUser to ScimUser by mapping properties with matching [ScimProperty] names.
        /// </summary>
        public ScimUser ToScimUser(TUser user)
        {
            var scimUser = new ScimUser();

            // Map Id if exists
            var idProp = typeof(TUser).GetProperty("Id");
            if (idProp != null)
            {
                scimUser.Id = idProp.GetValue(user)?.ToString() ?? string.Empty;
            }

            // Map all SCIM properties
            foreach (var kvp in _domainPropertyMap)
            {
                var scimAttrName = kvp.Key;
                var domainProp = kvp.Value;
                var value = domainProp.GetValue(user);
                
                if (value == null) continue;

                // Handle array-indexed SCIM properties like "emails[0].value"
                if (scimAttrName.Contains("[0]"))
                {
                    MapArrayIndexedProperty(scimUser, scimAttrName, value);
                    continue;
                }

                // Handle nested SCIM attributes like "name.givenName"
                if (scimAttrName.Contains('.'))
                {
                    var parts = scimAttrName.Split('.');
                    if (parts.Length == 2)
                    {
                        // Get the parent property (e.g., "name" → ScimUser.Name)
                        var parentPropName = NormalizePropertyName(parts[0]);
                        var childPropName = NormalizePropertyName(parts[1]);
                        
                        var parentProp = typeof(ScimUser).GetProperty(parentPropName);
                        if (parentProp != null)
                        {
                            // Get or create parent object
                            var parentObj = parentProp.GetValue(scimUser);
                            if (parentObj == null)
                            {
                                parentObj = Activator.CreateInstance(parentProp.PropertyType);
                                parentProp.SetValue(scimUser, parentObj);
                            }
                            
                            // Set child property
                            var childProp = parentProp.PropertyType.GetProperty(childPropName);
                            if (childProp != null && childProp.CanWrite)
                            {
                                childProp.SetValue(parentObj, value);
                            }
                        }
                    }
                }
                else if (_scimPropertyMap.TryGetValue(scimAttrName, out var scimProp))
                {
                    // Simple property mapping
                    if (IsCompatibleType(domainProp.PropertyType, scimProp.PropertyType))
                    {
                        scimProp.SetValue(scimUser, value);
                    }
                }
            }

            // Set metadata
            scimUser.Meta = new ScimMeta
            {
                ResourceType = "User",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Location = $"/scim/Users/{scimUser.Id}"
            };

            return scimUser;
        }

        /// <summary>
        /// Maps array-indexed SCIM properties (e.g., "emails[0].value") to ScimUser multi-valued attributes.
        /// </summary>
        private void MapArrayIndexedProperty(ScimUser scimUser, string scimAttrName, object value)
        {
            // Parse: "emails[0].value" → arrayProp="emails", index=0, subAttr="value"
            var match = System.Text.RegularExpressions.Regex.Match(
                scimAttrName, 
                @"^(\w+)\[(\d+)\]\.(\w+)$");
            
            if (!match.Success)
                return;

            var arrayPropName = match.Groups[1].Value; // "emails"
            var index = int.Parse(match.Groups[2].Value); // 0
            var subAttrName = match.Groups[3].Value; // "value"

            // Map to the corresponding List<T> property on ScimUser
            var normalizedArrayProp = NormalizePropertyName(arrayPropName);
            var listProp = typeof(ScimUser).GetProperty(normalizedArrayProp);
            
            if (listProp == null || !listProp.PropertyType.IsGenericType)
                return;

            var listValue = listProp.GetValue(scimUser);
            if (listValue == null)
            {
                listValue = Activator.CreateInstance(listProp.PropertyType);
                listProp.SetValue(scimUser, listValue);
            }

            var listType = listProp.PropertyType;
            var itemType = listType.GetGenericArguments()[0];
            
            // Get the list as IList
            var list = listValue as System.Collections.IList;
            if (list == null)
                return;

            // Ensure the list has enough items
            while (list.Count <= index)
            {
                var newItem = Activator.CreateInstance(itemType);
                
                // For index 0, set Primary = true
                if (list.Count == 0)
                {
                    var primaryProp = itemType.GetProperty("Primary");
                    if (primaryProp != null && primaryProp.CanWrite)
                    {
                        primaryProp.SetValue(newItem, true);
                    }
                }
                
                list.Add(newItem);
            }

            // Set the sub-attribute on the list item
            var item = list[index];
            if (item != null)
            {
                var normalizedSubAttr = NormalizePropertyName(subAttrName);
                var subProp = itemType.GetProperty(normalizedSubAttr);
                if (subProp != null && subProp.CanWrite)
                {
                    subProp.SetValue(item, value);
                }
            }
        }

        /// <summary>
        /// Normalizes property names from camelCase to PascalCase.
        /// </summary>
        private string NormalizePropertyName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            var lower = name.ToLower();
            return lower switch
            {
                "name" => "Name",
                "givenname" => "GivenName",
                "familyname" => "FamilyName",
                "middlename" => "MiddleName",
                "honorificprefix" => "HonorificPrefix",
                "honorificsuffix" => "HonorificSuffix",
                "username" => "UserName",
                "displayname" => "DisplayName",
                "externalid" => "ExternalId",
                "active" => "Active",
                "title" => "Title",
                "emails" => "Emails",
                "phonenumbers" => "PhoneNumbers",
                "addresses" => "Addresses",
                "value" => "Value",
                "type" => "Type",
                "primary" => "Primary",
                "formatted" => "Formatted",
                "streetaddress" => "StreetAddress",
                "locality" => "Locality",
                "region" => "Region",
                "postalcode" => "PostalCode",
                "country" => "Country",
                _ => char.ToUpper(name[0]) + name.Substring(1)
            };
        }

        /// <summary>
        /// Converts ScimUser to TUser by mapping properties with matching [ScimProperty] names.
        /// </summary>
        public TUser FromScimUser(ScimUser scimUser)
        {
            var user = Activator.CreateInstance<TUser>();

            // Map Id if exists
            var idProp = typeof(TUser).GetProperty("Id");
            if (idProp != null && idProp.CanWrite)
            {
                idProp.SetValue(user, scimUser.Id);
            }

            // Map all properties from domain to SCIM
            foreach (var kvp in _domainPropertyMap)
            {
                var scimAttrName = kvp.Key;
                var domainProp = kvp.Value;
                
                if (!domainProp.CanWrite) continue;

                object? value = null;

                // Handle array-indexed SCIM properties like "emails[0].value"
                if (scimAttrName.Contains("[0]"))
                {
                    value = GetArrayIndexedPropertyValue(scimUser, scimAttrName);
                }
                // Handle nested SCIM attributes like "name.givenName"
                else if (scimAttrName.Contains('.'))
                {
                    var parts = scimAttrName.Split('.');
                    if (parts.Length == 2)
                    {
                        var parentPropName = NormalizePropertyName(parts[0]);
                        var childPropName = NormalizePropertyName(parts[1]);
                        
                        var parentProp = typeof(ScimUser).GetProperty(parentPropName);
                        if (parentProp != null)
                        {
                            var parentObj = parentProp.GetValue(scimUser);
                            if (parentObj != null)
                            {
                                var childProp = parentProp.PropertyType.GetProperty(childPropName);
                                if (childProp != null)
                                {
                                    value = childProp.GetValue(parentObj);
                                }
                            }
                        }
                    }
                }
                else if (_scimPropertyMap.TryGetValue(scimAttrName, out var scimProp))
                {
                    value = scimProp.GetValue(scimUser);
                }

                // Set value if found and compatible
                if (value != null && IsCompatibleType(value.GetType(), domainProp.PropertyType))
                {
                    domainProp.SetValue(user, value);
                }
            }

            return user;
        }

        /// <summary>
        /// Gets value from array-indexed SCIM properties (e.g., "emails[0].value").
        /// </summary>
        private object? GetArrayIndexedPropertyValue(ScimUser scimUser, string scimAttrName)
        {
            // Parse: "emails[0].value" → arrayProp="emails", index=0, subAttr="value"
            var match = System.Text.RegularExpressions.Regex.Match(
                scimAttrName, 
                @"^(\w+)\[(\d+)\]\.(\w+)$");
            
            if (!match.Success)
                return null;

            var arrayPropName = match.Groups[1].Value; // "emails"
            var index = int.Parse(match.Groups[2].Value); // 0
            var subAttrName = match.Groups[3].Value; // "value"

            // Get the List<T> property from ScimUser
            var normalizedArrayProp = NormalizePropertyName(arrayPropName);
            var listProp = typeof(ScimUser).GetProperty(normalizedArrayProp);
            
            if (listProp == null)
                return null;

            var listValue = listProp.GetValue(scimUser);
            if (listValue == null)
                return null;

            var list = listValue as System.Collections.IList;
            if (list == null || list.Count <= index)
                return null;

            var item = list[index];
            if (item == null)
                return null;

            // Get the sub-attribute value
            var itemType = item.GetType();
            var normalizedSubAttr = NormalizePropertyName(subAttrName);
            var subProp = itemType.GetProperty(normalizedSubAttr);
            
            return subProp?.GetValue(item);
        }

        /// <summary>
        /// Checks if two types are compatible for assignment.
        /// </summary>
        private bool IsCompatibleType(Type sourceType, Type targetType)
        {
            if (sourceType == targetType)
                return true;

            if (targetType.IsAssignableFrom(sourceType))
                return true;

            // Handle nullable conversions
            var underlyingTarget = Nullable.GetUnderlyingType(targetType);
            if (underlyingTarget != null && underlyingTarget == sourceType)
                return true;

            return false;
        }
    }
}



