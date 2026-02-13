﻿using System.Linq.Expressions;
using System.Reflection;
using ScimAPI.Attributes;
using ScimAPI.Filtering.AST;

namespace ScimAPI.Filtering
{
    /// <summary>
    /// Generic SCIM filter translator that works with any TUser class annotated with [ScimProperty].
    /// Uses reflection to map SCIM attribute names to property names via attributes.
    /// </summary>
    /// <typeparam name="TUser">User type with [ScimProperty] attributes</typeparam>
    public class GenericScimFilterTranslator<TUser> : IScimFilterTranslator<TUser> where TUser : class
    {
        private readonly Dictionary<string, PropertyInfo> _propertyMap;

        public GenericScimFilterTranslator()
        {
            _propertyMap = BuildPropertyMap();
        }

        /// <summary>
        /// Builds a map of SCIM attribute names to PropertyInfo using [ScimProperty] attributes.
        /// </summary>
        private Dictionary<string, PropertyInfo> BuildPropertyMap()
        {
            var map = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            
            var properties = typeof(TUser).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            
            foreach (var prop in properties)
            {
                var scimAttr = prop.GetCustomAttribute<ScimPropertyAttribute>(inherit: true);
                if (scimAttr != null)
                {
                    // Map by SCIM attribute name (full path like "name.givenName")
                    map[scimAttr.Name] = prop;
                    
                    // If the attribute name contains a dot (nested attribute),
                    // also map just the last part (e.g., "givenName" for "name.givenName")
                    // This allows filters like "givenName eq 'John'" to work on CustomUser.FirstName
                    if (scimAttr.Name.Contains('.'))
                    {
                        var parts = scimAttr.Name.Split('.');
                        var lastName = parts[parts.Length - 1];
                        // Only add if not already exists (first one wins)
                        if (!map.ContainsKey(lastName))
                        {
                            map[lastName] = prop;
                        }
                    }
                    
                    // Also support nested attributes for complex types (e.g., when property is a complex object)
                    if (scimAttr.Type == "complex" && prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                    {
                        // Add nested properties
                        var nestedProps = prop.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var nestedProp in nestedProps)
                        {
                            var nestedAttr = nestedProp.GetCustomAttribute<ScimPropertyAttribute>(inherit: true);
                            if (nestedAttr != null)
                            {
                                map[$"{scimAttr.Name}.{nestedAttr.Name}"] = nestedProp;
                            }
                        }
                    }
                }
                else
                {
                    // Fallback: map by property name
                    map[prop.Name] = prop;
                }
            }

            return map;
        }

        /// <summary>
        /// Converts a SCIM FilterExpression to a LINQ Expression predicate.
        /// </summary>
        public Expression<Func<TUser, bool>>? BuildPredicate(FilterExpression? filter)
        {
            if (filter == null)
                return null;

            var parameter = Expression.Parameter(typeof(TUser), "user");
            var body = BuildExpression(filter, parameter);
            return Expression.Lambda<Func<TUser, bool>>(body, parameter);
        }

        /// <summary>
        /// Applies a SCIM filter to an IQueryable source.
        /// </summary>
        public IQueryable<TUser> Apply(IQueryable<TUser> source, FilterExpression? filter)
        {
            var predicate = BuildPredicate(filter);
            return predicate == null ? source : source.Where(predicate);
        }

        /// <summary>
        /// Recursively builds expression tree from FilterExpression AST.
        /// </summary>
        private Expression BuildExpression(FilterExpression filter, ParameterExpression parameter)
        {
            return filter switch
            {
                ComparisonFilter comp => BuildComparisonExpression(comp, parameter),
                PresenceFilter pres => BuildPresenceExpression(pres, parameter),
                AndFilter and => Expression.AndAlso(
                    BuildExpression(and.Left, parameter),
                    BuildExpression(and.Right, parameter)),
                OrFilter or => Expression.OrElse(
                    BuildExpression(or.Left, parameter),
                    BuildExpression(or.Right, parameter)),
                NotFilter not => Expression.Not(BuildExpression(not.Expression, parameter)),
                _ => throw new NotSupportedException($"Filter type {filter.GetType().Name} not supported")
            };
        }

        /// <summary>
        /// Builds comparison expression (eq, ne, co, sw, ew, gt, ge, lt, le).
        /// </summary>
        private Expression BuildComparisonExpression(ComparisonFilter comp, ParameterExpression parameter)
        {
            var attributePath = comp.AttributeName.ToLower();
            var value = comp.Value.GetValue();

            // Get property access expression
            var propertyExpression = GetPropertyExpression(parameter, attributePath);
            
            // Handle null property (not found)
            if (propertyExpression == null)
            {
                // Property not found - return false (no match)
                return Expression.Constant(false);
            }

            var valueExpression = Expression.Constant(value, value.GetType());

            return comp.Operator switch
            {
                FilterOperator.Equals => BuildEqualsExpression(propertyExpression, valueExpression, value),
                FilterOperator.NotEquals => Expression.Not(BuildEqualsExpression(propertyExpression, valueExpression, value)),
                FilterOperator.Contains => BuildContainsExpression(propertyExpression, valueExpression),
                FilterOperator.StartsWith => BuildStartsWithExpression(propertyExpression, valueExpression),
                FilterOperator.EndsWith => BuildEndsWithExpression(propertyExpression, valueExpression),
                FilterOperator.GreaterThan => BuildComparisonExpression(propertyExpression, valueExpression, ExpressionType.GreaterThan),
                FilterOperator.GreaterOrEqual => BuildComparisonExpression(propertyExpression, valueExpression, ExpressionType.GreaterThanOrEqual),
                FilterOperator.LessThan => BuildComparisonExpression(propertyExpression, valueExpression, ExpressionType.LessThan),
                FilterOperator.LessOrEqual => BuildComparisonExpression(propertyExpression, valueExpression, ExpressionType.LessThanOrEqual),
                _ => throw new NotSupportedException($"Operator {comp.Operator} not supported")
            };
        }

        /// <summary>
        /// Builds presence check expression (pr operator).
        /// </summary>
        private Expression BuildPresenceExpression(PresenceFilter pres, ParameterExpression parameter)
        {
            var attributePath = pres.AttributeName.ToLower();
            var propertyExpression = GetPropertyExpression(parameter, attributePath);

            if (propertyExpression == null)
            {
                // Property not found - return false
                return Expression.Constant(false);
            }

            // Check if property is not null and not empty (for strings)
            if (propertyExpression.Type == typeof(string))
            {
                var isNullOrEmptyMethod = typeof(string).GetMethod(nameof(string.IsNullOrEmpty), new[] { typeof(string) })!;
                var isNullOrEmptyCall = Expression.Call(isNullOrEmptyMethod, propertyExpression);
                return Expression.Not(isNullOrEmptyCall);
            }

            // For nullable types, check not null
            if (Nullable.GetUnderlyingType(propertyExpression.Type) != null)
            {
                return Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyExpression.Type));
            }

            // For non-nullable types, always true
            return Expression.Constant(true);
        }

        /// <summary>
        /// Gets property access expression from SCIM attribute path.
        /// Returns null if property not found.
        /// </summary>
        private Expression? GetPropertyExpression(ParameterExpression parameter, string attributePath)
        {
            // Try direct mapping first (handles nested paths like "name.givenName")
            if (_propertyMap.TryGetValue(attributePath, out var directProp))
            {
                // Check if this is a nested property
                if (attributePath.Contains('.'))
                {
                    var parts = attributePath.Split('.');
                    Expression current = parameter;
                    
                    // Navigate to parent object first
                    if (_propertyMap.TryGetValue(parts[0], out var parentProp))
                    {
                        current = Expression.Property(current, parentProp);
                        return Expression.Property(current, directProp);
                    }
                }
                
                return Expression.Property(parameter, directProp);
            }

            // Try splitting and navigating manually
            var pathParts = attributePath.Split('.');
            if (pathParts.Length > 1)
            {
                Expression current = parameter;
                
                foreach (var part in pathParts)
                {
                    if (_propertyMap.TryGetValue(part, out var prop))
                    {
                        current = Expression.Property(current, prop);
                    }
                    else
                    {
                        return null; // Property not found
                    }
                }
                
                return current;
            }

            return null; // Property not found
        }

        /// <summary>
        /// Builds equals expression with proper type handling and case-insensitive string comparison.
        /// </summary>
        private Expression BuildEqualsExpression(Expression property, Expression value, object rawValue)
        {
            if (rawValue is string)
            {
                // Case-insensitive string comparison using ToLower() for EF Core compatibility
                var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
                
                var propertyToLower = Expression.Call(property, toLowerMethod);
                var valueToLower = Expression.Call(value, toLowerMethod);
                
                return Expression.Equal(propertyToLower, valueToLower);
            }

            // Handle nullable type conversions
            var propertyType = property.Type;
            var valueType = value.Type;

            if (propertyType != valueType)
            {
                // Try to convert value to property type
                if (Nullable.GetUnderlyingType(propertyType) == valueType)
                {
                    // Convert value to nullable
                    value = Expression.Convert(value, propertyType);
                }
                else if (Nullable.GetUnderlyingType(valueType) == propertyType)
                {
                    // Convert property to nullable
                    property = Expression.Convert(property, valueType);
                }
            }

            return Expression.Equal(property, value);
        }

        /// <summary>
        /// Builds string contains expression (case-insensitive).
        /// </summary>
        private Expression BuildContainsExpression(Expression property, Expression value)
        {
            if (property.Type != typeof(string))
            {
                throw new InvalidOperationException($"Contains operator only supported on string properties");
            }

            // Use ToLower() for case-insensitive comparison compatible with EF Core
            var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
            
            var propertyToLower = Expression.Call(property, toLowerMethod);
            var valueToLower = Expression.Call(value, toLowerMethod);
            
            return Expression.Call(propertyToLower, containsMethod, valueToLower);
        }

        /// <summary>
        /// Builds string starts-with expression (case-insensitive).
        /// </summary>
        private Expression BuildStartsWithExpression(Expression property, Expression value)
        {
            if (property.Type != typeof(string))
            {
                throw new InvalidOperationException($"StartsWith operator only supported on string properties");
            }

            // Use ToLower() for case-insensitive comparison compatible with EF Core
            var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
            var startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!;
            
            var propertyToLower = Expression.Call(property, toLowerMethod);
            var valueToLower = Expression.Call(value, toLowerMethod);
            
            return Expression.Call(propertyToLower, startsWithMethod, valueToLower);
        }

        /// <summary>
        /// Builds string ends-with expression (case-insensitive).
        /// </summary>
        private Expression BuildEndsWithExpression(Expression property, Expression value)
        {
            if (property.Type != typeof(string))
            {
                throw new InvalidOperationException($"EndsWith operator only supported on string properties");
            }

            // Use ToLower() for case-insensitive comparison compatible with EF Core
            var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
            var endsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!;
            
            var propertyToLower = Expression.Call(property, toLowerMethod);
            var valueToLower = Expression.Call(value, toLowerMethod);
            
            return Expression.Call(propertyToLower, endsWithMethod, valueToLower);
        }

        /// <summary>
        /// Builds numeric/date comparison expression.
        /// </summary>
        private Expression BuildComparisonExpression(Expression property, Expression value, ExpressionType comparisonType)
        {
            // Ensure types match
            var propertyType = property.Type;
            var valueType = value.Type;

            if (propertyType != valueType)
            {
                // Try to convert value to property type
                value = Expression.Convert(value, propertyType);
            }

            return Expression.MakeBinary(comparisonType, property, value);
        }
    }
}

