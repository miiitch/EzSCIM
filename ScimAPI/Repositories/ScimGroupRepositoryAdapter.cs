using ScimAPI.Constants;
using ScimAPI.Filtering;
using ScimAPI.Filtering.AST;
using ScimAPI.Models;
using System.Reflection;
using ScimAPI.Attributes;
using ScimAPI.DataRepositories;

namespace ScimAPI.Repositories
{
    /// <summary>
    /// Adapter that bridges IGroupDataRepository to IScimGroupRepository.
    /// Maps between TGroup (your domain model) and ScimGroup using [ScimProperty] attributes.
    /// Uses IScimFilterTranslator to execute filters server-side on IQueryable.
    /// </summary>
    /// <typeparam name="TGroup">Your group class annotated with [ScimProperty] attributes</typeparam>
    public class ScimGroupRepositoryAdapter<TGroup> : IScimGroupRepository<ScimGroup> 
        where TGroup : class
    {
        private readonly IGroupDataRepository<TGroup> _dataRepository;
        private readonly IScimFilterTranslator<TGroup> _filterTranslator;
        private readonly GroupMapper<TGroup> _mapper;

        public ScimGroupRepositoryAdapter(
            IGroupDataRepository<TGroup> dataRepository,
            IScimFilterTranslator<TGroup> filterTranslator)
        {
            _dataRepository = dataRepository;
            _filterTranslator = filterTranslator;
            _mapper = new GroupMapper<TGroup>();
        }

        public async Task<ScimGroup?> GetGroupAsync(string id)
        {
            var group = await _dataRepository.GetAsync(id);
            return group == null ? null : _mapper.ToScimGroup(group);
        }

        public Task<ScimGroup?> GetGroupByDisplayNameAsync(string displayName)
        {
            // Use SCIM filter translator to find by displayName
            var filter = new ComparisonFilter(
                ScimAttributeNames.Group.DisplayName, 
                FilterOperator.Equals, 
                new StringValue(displayName));
            var predicate = _filterTranslator.BuildPredicate(filter);
            
            if (predicate == null)
                return Task.FromResult<ScimGroup?>(null);

            var group = _dataRepository.Query().Where(predicate).FirstOrDefault();
            var result = group == null ? null : _mapper.ToScimGroup(group);
            return Task.FromResult(result);
        }

        public Task<ScimListResponse<ScimGroup>> GetGroupsAsync(FilterExpression? filter = null, int startIndex = 1, int count = 100)
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
            var groups = query
                .Skip(startIndex - 1)
                .Take(count)
                .ToList();

            // Map to ScimGroup
            var scimGroups = groups.Select(g => _mapper.ToScimGroup(g)).ToList();

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
            var domainGroup = _mapper.FromScimGroup(group);
            var created = await _dataRepository.CreateAsync(domainGroup);
            return _mapper.ToScimGroup(created);
        }

        public async Task<ScimGroup?> UpdateGroupAsync(string id, ScimGroup group)
        {
            var domainGroup = _mapper.FromScimGroup(group);
            var updated = await _dataRepository.UpdateAsync(id, domainGroup);
            return updated == null ? null : _mapper.ToScimGroup(updated);
        }

        public Task<ScimGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest)
        {
            // TODO: Implement PATCH mapping
            throw new NotImplementedException("PATCH operations require custom implementation per domain model");
        }

        public async Task<bool> DeleteGroupAsync(string id)
        {
            return await _dataRepository.DeleteAsync(id);
        }
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

