using EzSCIM.Constants;
using EzSCIM.Filtering;
using EzSCIM.Filtering.AST;
using EzSCIM.Models;
using System.Reflection;
using EzSCIM.Attributes;
using EzSCIM.DataRepositories;
using EzSCIM.Services;

namespace EzSCIM.Repositories
{
    /// <summary>
    /// Adapter that bridges IUserGroupDataRepository to IScimUserGroupRepository.
    /// Maps between TUser/TGroup (your domain models) and ScimUser/ScimGroup using [ScimProperty] attributes.
    /// Uses IScimFilterTranslator to execute filters server-side on IQueryable.
    /// </summary>
    /// <typeparam name="TUser">Your user class annotated with [ScimProperty] attributes</typeparam>
    /// <typeparam name="TGroup">Your group class annotated with [ScimProperty] attributes</typeparam>
    public class ScimUserGroupRepositoryAdapter<TUser, TGroup> : IScimUserGroupRepository<ScimUser, ScimGroup>
        where TUser : class
        where TGroup : class
    {
        private readonly IUserGroupDataRepository<TUser, TGroup> _dataRepository;
        private readonly IScimFilterTranslator<TUser> _userFilterTranslator;
        private readonly IScimFilterTranslator<TGroup> _groupFilterTranslator;
        private readonly UserMapper<TUser> _userMapper;
        private readonly GroupMapper<TGroup> _groupMapper;

        public ScimUserGroupRepositoryAdapter(
            IUserGroupDataRepository<TUser, TGroup> dataRepository,
            IScimFilterTranslator<TUser> userFilterTranslator,
            IScimFilterTranslator<TGroup> groupFilterTranslator)
        {
            _dataRepository = dataRepository;
            _userFilterTranslator = userFilterTranslator;
            _groupFilterTranslator = groupFilterTranslator;
            _userMapper = new UserMapper<TUser>();
            _groupMapper = new GroupMapper<TGroup>();
        }

        #region User Operations

        public async Task<ScimUser?> GetUserAsync(string id)
        {
            var user = await _dataRepository.GetUserAsync(id);
            return user == null ? null : _userMapper.ToScimUser(user);
        }

        public Task<ScimUser?> GetUserByUserNameAsync(string userName)
        {
            var filter = new ComparisonFilter(
                ScimAttributeNames.User.UserName,
                FilterOperator.Equals,
                new StringValue(userName));
            var predicate = _userFilterTranslator.BuildPredicate(filter);

            if (predicate == null)
                return Task.FromResult<ScimUser?>(null);

            var user = _dataRepository.QueryUsers().Where(predicate).FirstOrDefault();
            var result = user == null ? null : _userMapper.ToScimUser(user);
            return Task.FromResult(result);
        }

        public Task<ScimListResponse<ScimUser>> GetUsersAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100)
        {
            var query = _dataRepository.QueryUsers();

            if (filter != null)
            {
                query = _userFilterTranslator.Apply(query, filter);
            }

            var totalResults = query.Count();

            var users = query
                .Skip(startIndex - 1)
                .Take(count)
                .ToList();

            var scimUsers = users.Select(u => _userMapper.ToScimUser(u)).ToList();

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
            var domainUser = _userMapper.FromScimUser(user);
            var created = await _dataRepository.CreateUserAsync(domainUser);
            return _userMapper.ToScimUser(created);
        }

        public async Task<ScimUser?> UpdateUserAsync(string id, ScimUser user)
        {
            var domainUser = _userMapper.FromScimUser(user);
            var updated = await _dataRepository.UpdateUserAsync(id, domainUser);
            return updated == null ? null : _userMapper.ToScimUser(updated);
        }

        public async Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest)
        {
            var domainUser = await _dataRepository.GetUserAsync(id);
            if (domainUser == null) return null;

            var scimUser = _userMapper.ToScimUser(domainUser);
            ScimPatchService.ApplyPatch(scimUser, patchRequest);

            var updatedDomainUser = _userMapper.FromScimUser(scimUser);
            var saved = await _dataRepository.UpdateUserAsync(id, updatedDomainUser);
            return saved == null ? null : _userMapper.ToScimUser(saved);
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _dataRepository.DeleteUserAsync(id);
        }

        #endregion

        #region Group Operations

        public async Task<ScimGroup?> GetGroupAsync(string id)
        {
            var group = await _dataRepository.GetGroupAsync(id);
            return group == null ? null : _groupMapper.ToScimGroup(group);
        }

        public Task<ScimGroup?> GetGroupByDisplayNameAsync(string displayName)
        {
            var filter = new ComparisonFilter(
                ScimAttributeNames.Group.DisplayName,
                FilterOperator.Equals,
                new StringValue(displayName));
            var predicate = _groupFilterTranslator.BuildPredicate(filter);

            if (predicate == null)
                return Task.FromResult<ScimGroup?>(null);

            var group = _dataRepository.QueryGroups().Where(predicate).FirstOrDefault();
            var result = group == null ? null : _groupMapper.ToScimGroup(group);
            return Task.FromResult(result);
        }

        public Task<ScimListResponse<ScimGroup>> GetGroupsAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100)
        {
            var query = _dataRepository.QueryGroups();

            if (filter != null)
            {
                query = _groupFilterTranslator.Apply(query, filter);
            }

            var totalResults = query.Count();

            var groups = query
                .Skip(startIndex - 1)
                .Take(count)
                .ToList();

            var scimGroups = groups.Select(g => _groupMapper.ToScimGroup(g)).ToList();

            var response = new ScimListResponse<ScimGroup>
            {
                TotalResults = totalResults,
                StartIndex = startIndex,
                ItemsPerPage = scimGroups.Count,
                Resources = scimGroups
            };

            return Task.FromResult(response);
        }

        public async Task<ScimGroup> CreateGroupAsync(ScimGroup group)
        {
            var domainGroup = _groupMapper.FromScimGroup(group);
            var created = await _dataRepository.CreateGroupAsync(domainGroup);
            return _groupMapper.ToScimGroup(created);
        }

        public async Task<ScimGroup?> UpdateGroupAsync(string id, ScimGroup group)
        {
            var domainGroup = _groupMapper.FromScimGroup(group);
            var updated = await _dataRepository.UpdateGroupAsync(id, domainGroup);
            return updated == null ? null : _groupMapper.ToScimGroup(updated);
        }

        public async Task<ScimGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest)
        {
            var domainGroup = await _dataRepository.GetGroupAsync(id);
            if (domainGroup == null) return null;

            var scimGroup = _groupMapper.ToScimGroup(domainGroup);
            ScimPatchService.ApplyPatch(scimGroup, patchRequest);

            var updatedDomainGroup = _groupMapper.FromScimGroup(scimGroup);
            var saved = await _dataRepository.UpdateGroupAsync(id, updatedDomainGroup);
            return saved == null ? null : _groupMapper.ToScimGroup(saved);
        }

        public async Task<bool> DeleteGroupAsync(string id)
        {
            return await _dataRepository.DeleteGroupAsync(id);
        }

        #endregion
    }

    /// <summary>
    /// Maps between TGroup (domain model) and ScimGroup using [ScimProperty] attributes.
    /// </summary>
    internal class GroupMapper<TGroup> where TGroup : class
    {
        private readonly Dictionary<string, PropertyInfo> _scimPropertyMap;
        private readonly Dictionary<string, PropertyInfo> _domainPropertyMap;

        public GroupMapper()
        {
            _scimPropertyMap = BuildPropertyMap(typeof(ScimGroup));
            _domainPropertyMap = BuildPropertyMap(typeof(TGroup));
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
        /// Converts TGroup to ScimGroup by mapping properties with matching [ScimProperty] names.
        /// </summary>
        public ScimGroup ToScimGroup(TGroup group)
        {
            var scimGroup = new ScimGroup();

            // Map Id if exists
            var idProp = typeof(TGroup).GetProperty("Id");
            if (idProp != null)
            {
                scimGroup.Id = idProp.GetValue(group)?.ToString() ?? string.Empty;
            }

            // Map all SCIM properties
            foreach (var kvp in _domainPropertyMap)
            {
                var scimAttrName = kvp.Key;
                var domainProp = kvp.Value;

                if (_scimPropertyMap.TryGetValue(scimAttrName, out var scimProp))
                {
                    var value = domainProp.GetValue(group);
                    
                    // Handle type conversion if needed
                    if (value != null && IsCompatibleType(domainProp.PropertyType, scimProp.PropertyType))
                    {
                        scimProp.SetValue(scimGroup, value);
                    }
                }
            }

            // Handle MembersJson if present (special case for JSON-serialized members)
            var membersJsonProp = typeof(TGroup).GetProperty("MembersJson");
            if (membersJsonProp != null)
            {
                var membersJson = membersJsonProp.GetValue(group) as string;
                if (!string.IsNullOrEmpty(membersJson))
                {
                    try
                    {
                        var memberJsonOptions = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var memberInfos = System.Text.Json.JsonSerializer.Deserialize<List<MemberInfo>>(membersJson, memberJsonOptions);
                        if (memberInfos != null && memberInfos.Count > 0)
                        {
                            scimGroup.Members = memberInfos.Select(m => new ScimMember
                            {
                                Value = m.Value,
                                Display = m.Display
                            }).ToList();
                        }
                    }
                    catch
                    {
                        // If deserialization fails, leave Members empty
                    }
                }
            }

            // Set metadata
            scimGroup.Meta = new ScimMeta
            {
                ResourceType = "Group",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Location = $"/scim/Groups/{scimGroup.Id}"
            };

            return scimGroup;
        }

        /// <summary>
        /// Helper class for JSON deserialization of members.
        /// </summary>
        private class MemberInfo
        {
            public string Value { get; set; } = "";
            public string Display { get; set; } = "";
        }

        /// <summary>
        /// Converts ScimGroup to TGroup by mapping properties with matching [ScimProperty] names.
        /// </summary>
        public TGroup FromScimGroup(ScimGroup scimGroup)
        {
            var group = Activator.CreateInstance<TGroup>();

            // Map Id if exists
            var idProp = typeof(TGroup).GetProperty("Id");
            if (idProp != null && idProp.CanWrite)
            {
                idProp.SetValue(group, scimGroup.Id);
            }

            // Map all SCIM properties
            foreach (var kvp in _scimPropertyMap)
            {
                var scimAttrName = kvp.Key;
                var scimProp = kvp.Value;

                if (_domainPropertyMap.TryGetValue(scimAttrName, out var domainProp) && domainProp.CanWrite)
                {
                    var value = scimProp.GetValue(scimGroup);
                    
                    // Handle type conversion if needed
                    if (value != null && IsCompatibleType(scimProp.PropertyType, domainProp.PropertyType))
                    {
                        domainProp.SetValue(group, value);
                    }
                }
            }

            return group;
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

