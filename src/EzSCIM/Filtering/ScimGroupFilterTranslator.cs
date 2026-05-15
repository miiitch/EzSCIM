using System.Linq.Expressions;
using EzSCIM.Constants;
using EzSCIM.Filtering.AST;
using EzSCIM.Models;

namespace EzSCIM.Filtering
{
    /// <summary>
    /// Translates SCIM FilterExpression AST to LINQ Expression trees for ScimGroup.
    /// This allows filters to be executed server-side (e.g., in EF Core queries).
    /// </summary>
    public class ScimGroupFilterTranslator : IScimFilterTranslator<ScimGroup>
    {
        /// <summary>
        /// Converts a SCIM FilterExpression to a LINQ Expression predicate.
        /// </summary>
        public Expression<Func<ScimGroup, bool>>? BuildPredicate(FilterExpression? filter)
        {
            if (filter == null)
                return null;

            var parameter = Expression.Parameter(typeof(ScimGroup), "group");
            var body = BuildExpression(filter, parameter);
            return Expression.Lambda<Func<ScimGroup, bool>>(body, parameter);
        }

        /// <summary>
        /// Applies a SCIM filter to an IQueryable source.
        /// </summary>
        public IQueryable<ScimGroup> Apply(IQueryable<ScimGroup> source, FilterExpression? filter)
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
            var valueExpression = Expression.Constant(value, value.GetType());

            return comp.Operator switch
            {
                FilterOperator.Equals => BuildEqualsExpression(propertyExpression, valueExpression, value),
                FilterOperator.NotEquals => Expression.Not(BuildEqualsExpression(propertyExpression, valueExpression, value)),
                FilterOperator.Contains => BuildContainsExpression(propertyExpression, valueExpression),
                FilterOperator.StartsWith => BuildStartsWithExpression(propertyExpression, valueExpression),
                FilterOperator.EndsWith => BuildEndsWithExpression(propertyExpression, valueExpression),
                FilterOperator.GreaterThan => Expression.GreaterThan(propertyExpression, valueExpression),
                FilterOperator.GreaterOrEqual => Expression.GreaterThanOrEqual(propertyExpression, valueExpression),
                FilterOperator.LessThan => Expression.LessThan(propertyExpression, valueExpression),
                FilterOperator.LessOrEqual => Expression.LessThanOrEqual(propertyExpression, valueExpression),
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
        /// Gets property access expression from attribute path (supports nested properties).
        /// </summary>
        private Expression GetPropertyExpression(ParameterExpression parameter, string attributePath)
        {
            Expression current = parameter;
            var parts = attributePath.Split('.');

            foreach (var part in parts)
            {
                var propertyName = NormalizePropertyName(part);
                var propertyInfo = current.Type.GetProperty(propertyName);

                if (propertyInfo == null)
                    throw new InvalidOperationException($"Property '{propertyName}' not found on type {current.Type.Name}");

                current = Expression.Property(current, propertyInfo);
            }

            return current;
        }

        /// <summary>
        /// Normalizes SCIM attribute names to C# property names (camelCase ? PascalCase).
        /// Uses ScimAttributeNames constants for mapping.
        /// </summary>
        private string NormalizePropertyName(string attributeName)
        {
            var lowerName = attributeName.ToLower();
            return lowerName switch
            {
                "displayname" => "DisplayName",
                "members" => "Members",
                "externalid" => "ExternalId",
                _ => char.ToUpper(lowerName[0]) + lowerName.Substring(1)
            };
        }

        /// <summary>
        /// Builds equals expression with proper type handling and case-insensitive string comparison.
        /// </summary>
        private Expression BuildEqualsExpression(Expression property, Expression value, object rawValue)
        {
            if (rawValue is string)
            {
                // Case-insensitive string comparison
                var equalsMethod = typeof(string).GetMethod(nameof(string.Equals), 
                    new[] { typeof(string), typeof(string), typeof(StringComparison) })!;
                
                return Expression.Call(
                    equalsMethod,
                    property,
                    value,
                    Expression.Constant(StringComparison.OrdinalIgnoreCase));
            }

            // For nullable properties, handle null comparison
            if (Nullable.GetUnderlyingType(property.Type) != null && rawValue is bool boolValue)
            {
                var nullableBool = Expression.Constant((bool?)boolValue, typeof(bool?));
                return Expression.Equal(property, nullableBool);
            }

            return Expression.Equal(property, value);
        }

        /// <summary>
        /// Builds string contains expression (case-insensitive).
        /// </summary>
        private Expression BuildContainsExpression(Expression property, Expression value)
        {
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) })!;
            
            return Expression.Call(
                property,
                containsMethod,
                value,
                Expression.Constant(StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Builds string starts-with expression (case-insensitive).
        /// </summary>
        private Expression BuildStartsWithExpression(Expression property, Expression value)
        {
            var startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string), typeof(StringComparison) })!;
            
            return Expression.Call(
                property,
                startsWithMethod,
                value,
                Expression.Constant(StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Builds string ends-with expression (case-insensitive).
        /// </summary>
        private Expression BuildEndsWithExpression(Expression property, Expression value)
        {
            var endsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string), typeof(StringComparison) })!;
            
            return Expression.Call(
                property,
                endsWithMethod,
                value,
                Expression.Constant(StringComparison.OrdinalIgnoreCase));
        }
    }
}

