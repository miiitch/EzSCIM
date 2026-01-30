using ScimAPI.Filtering.AST;

namespace ScimAPI.Filtering
{
    /// <summary>
    /// Fluent builder class for creating SCIM filter expressions using static methods.
    /// Usage: F.Equals("userName", "john") or F.And(filter1, filter2)
    /// </summary>
    public static class F
    {
        // ==================== COMPARISON FILTERS ====================

        /// <summary>
        /// Creates an equality comparison filter for string values
        /// </summary>
        public static ComparisonFilter Equals(string attributeName, string value)
            => new(attributeName, FilterOperator.Equals, new StringValue(value));

        /// <summary>
        /// Creates an equality comparison filter for boolean values
        /// </summary>
        public static ComparisonFilter Equals(string attributeName, bool value)
            => new(attributeName, FilterOperator.Equals, new BooleanValue(value));

        /// <summary>
        /// Creates an equality comparison filter for decimal values
        /// </summary>
        public static ComparisonFilter Equals(string attributeName, decimal value)
            => new(attributeName, FilterOperator.Equals, new NumericValue(value));

        /// <summary>
        /// Creates an equality comparison filter for integer values
        /// </summary>
        public static ComparisonFilter Equals(string attributeName, int value)
            => new(attributeName, FilterOperator.Equals, new NumericValue(value));

        /// <summary>
        /// Creates an equality comparison filter for DateTime values
        /// </summary>
        public static ComparisonFilter Equals(string attributeName, DateTime value)
            => new(attributeName, FilterOperator.Equals, new DateTimeValue(value));

        /// <summary>
        /// Creates a not-equal comparison filter for string values
        /// </summary>
        public static ComparisonFilter NotEquals(string attributeName, string value)
            => new(attributeName, FilterOperator.NotEquals, new StringValue(value));

        /// <summary>
        /// Creates a not-equal comparison filter for boolean values
        /// </summary>
        public static ComparisonFilter NotEquals(string attributeName, bool value)
            => new(attributeName, FilterOperator.NotEquals, new BooleanValue(value));

        /// <summary>
        /// Creates a not-equal comparison filter for numeric values
        /// </summary>
        public static ComparisonFilter NotEquals(string attributeName, decimal value)
            => new(attributeName, FilterOperator.NotEquals, new NumericValue(value));

        /// <summary>
        /// Creates a contains comparison filter
        /// </summary>
        public static ComparisonFilter Contains(string attributeName, string value)
            => new(attributeName, FilterOperator.Contains, new StringValue(value));

        /// <summary>
        /// Creates a starts-with comparison filter
        /// </summary>
        public static ComparisonFilter StartsWith(string attributeName, string value)
            => new(attributeName, FilterOperator.StartsWith, new StringValue(value));

        /// <summary>
        /// Creates an ends-with comparison filter
        /// </summary>
        public static ComparisonFilter EndsWith(string attributeName, string value)
            => new(attributeName, FilterOperator.EndsWith, new StringValue(value));

        /// <summary>
        /// Creates a greater-than comparison filter for decimal values
        /// </summary>
        public static ComparisonFilter GreaterThan(string attributeName, decimal value)
            => new(attributeName, FilterOperator.GreaterThan, new NumericValue(value));

        /// <summary>
        /// Creates a greater-than comparison filter for integer values
        /// </summary>
        public static ComparisonFilter GreaterThan(string attributeName, int value)
            => new(attributeName, FilterOperator.GreaterThan, new NumericValue(value));

        /// <summary>
        /// Creates a greater-than comparison filter for DateTime values
        /// </summary>
        public static ComparisonFilter GreaterThan(string attributeName, DateTime value)
            => new(attributeName, FilterOperator.GreaterThan, new DateTimeValue(value));

        /// <summary>
        /// Creates a greater-than-or-equal comparison filter for decimal values
        /// </summary>
        public static ComparisonFilter GreaterOrEqual(string attributeName, decimal value)
            => new(attributeName, FilterOperator.GreaterOrEqual, new NumericValue(value));

        /// <summary>
        /// Creates a greater-than-or-equal comparison filter for integer values
        /// </summary>
        public static ComparisonFilter GreaterOrEqual(string attributeName, int value)
            => new(attributeName, FilterOperator.GreaterOrEqual, new NumericValue(value));

        /// <summary>
        /// Creates a greater-than-or-equal comparison filter for DateTime values
        /// </summary>
        public static ComparisonFilter GreaterOrEqual(string attributeName, DateTime value)
            => new(attributeName, FilterOperator.GreaterOrEqual, new DateTimeValue(value));

        /// <summary>
        /// Creates a less-than comparison filter for decimal values
        /// </summary>
        public static ComparisonFilter LessThan(string attributeName, decimal value)
            => new(attributeName, FilterOperator.LessThan, new NumericValue(value));

        /// <summary>
        /// Creates a less-than comparison filter for integer values
        /// </summary>
        public static ComparisonFilter LessThan(string attributeName, int value)
            => new(attributeName, FilterOperator.LessThan, new NumericValue(value));

        /// <summary>
        /// Creates a less-than comparison filter for DateTime values
        /// </summary>
        public static ComparisonFilter LessThan(string attributeName, DateTime value)
            => new(attributeName, FilterOperator.LessThan, new DateTimeValue(value));

        /// <summary>
        /// Creates a less-than-or-equal comparison filter for decimal values
        /// </summary>
        public static ComparisonFilter LessOrEqual(string attributeName, decimal value)
            => new(attributeName, FilterOperator.LessOrEqual, new NumericValue(value));

        /// <summary>
        /// Creates a less-than-or-equal comparison filter for integer values
        /// </summary>
        public static ComparisonFilter LessOrEqual(string attributeName, int value)
            => new(attributeName, FilterOperator.LessOrEqual, new NumericValue(value));

        /// <summary>
        /// Creates a less-than-or-equal comparison filter for DateTime values
        /// </summary>
        public static ComparisonFilter LessOrEqual(string attributeName, DateTime value)
            => new(attributeName, FilterOperator.LessOrEqual, new DateTimeValue(value));

        // ==================== PRESENCE FILTER ====================

        /// <summary>
        /// Creates a presence filter (e.g., phoneNumbers pr)
        /// </summary>
        public static PresenceFilter Present(string attributeName)
            => new(attributeName);

        // ==================== LOGICAL OPERATORS ====================

        /// <summary>
        /// Creates a NOT filter negating an expression
        /// </summary>
        public static NotFilter Not(FilterExpression expression)
            => new(expression);

        // ==================== FLUENT EXTENSION METHODS ====================

        /// <summary>
        /// Fluent method to AND two filters together
        /// Usage: filter1.And(filter2) or F.And(filter1, filter2)
        /// </summary>
        public static AndFilter And(this FilterExpression left, FilterExpression right)
            => new(left, right);

        /// <summary>
        /// Fluent method to OR two filters together
        /// Usage: filter1.Or(filter2) or F.Or(filter1, filter2)
        /// </summary>
        public static OrFilter Or(this FilterExpression left, FilterExpression right)
            => new(left, right);

        /// <summary>
        /// Fluent method to negate a filter
        /// Usage: filter.Negate()
        /// </summary>
        public static NotFilter Negate(this FilterExpression expression)
            => new(expression);
    }
}
