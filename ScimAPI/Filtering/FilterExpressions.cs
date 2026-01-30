
using System;

namespace ScimAPI.Filtering.AST
{
    // ==================== BASE CLASSES ====================

    /// <summary>
    /// Base class for all filter expressions in the AST (Abstract Syntax Tree)
    /// </summary>
    public abstract class FilterExpression
    {
        public abstract T Accept<T>(IFilterExpressionVisitor<T> visitor);
    }

    /// <summary>
    /// Base class for filter values
    /// </summary>
    public abstract class FilterValue
    {
        public abstract object GetValue();
        public abstract FilterValueType GetValueType();
    }

    // ==================== FILTER TYPES ====================

    /// <summary>
    /// Represents a comparison filter (e.g., active eq true)
    /// </summary>
    public class ComparisonFilter : FilterExpression
    {
        public string AttributeName { get; set; }
        public FilterOperator Operator { get; set; }
        public FilterValue Value { get; set; }

        public ComparisonFilter(string attributeName, FilterOperator op, FilterValue value)
        {
            AttributeName = attributeName ?? throw new ArgumentNullException(nameof(attributeName));
            Operator = op;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override T Accept<T>(IFilterExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override string ToString() => $"{AttributeName} {OpStr(Operator)} {Value}";

        private static string OpStr(FilterOperator op) => op switch
        {
            FilterOperator.Equals => "eq",
            FilterOperator.NotEquals => "ne",
            FilterOperator.Contains => "co",
            FilterOperator.StartsWith => "sw",
            FilterOperator.EndsWith => "ew",
            FilterOperator.GreaterThan => "gt",
            FilterOperator.GreaterOrEqual => "ge",
            FilterOperator.LessThan => "lt",
            FilterOperator.LessOrEqual => "le",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Represents a presence check filter (e.g., phoneNumbers pr)
    /// </summary>
    public class PresenceFilter : FilterExpression
    {
        public string AttributeName { get; set; }

        public PresenceFilter(string attributeName)
        {
            AttributeName = attributeName ?? throw new ArgumentNullException(nameof(attributeName));
        }

        public override T Accept<T>(IFilterExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override string ToString() => $"{AttributeName} pr";
    }

    /// <summary>
    /// Represents a logical AND operation between two filters
    /// </summary>
    public class AndFilter : FilterExpression
    {
        public FilterExpression Left { get; set; }
        public FilterExpression Right { get; set; }

        public AndFilter(FilterExpression left, FilterExpression right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public override T Accept<T>(IFilterExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override string ToString() => $"({Left}) and ({Right})";
    }

    /// <summary>
    /// Represents a logical OR operation between two filters
    /// </summary>
    public class OrFilter : FilterExpression
    {
        public FilterExpression Left { get; set; }
        public FilterExpression Right { get; set; }

        public OrFilter(FilterExpression left, FilterExpression right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public override T Accept<T>(IFilterExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override string ToString() => $"({Left}) or ({Right})";
    }

    /// <summary>
    /// Represents a logical NOT operation
    /// </summary>
    public class NotFilter : FilterExpression
    {
        public FilterExpression Expression { get; set; }

        public NotFilter(FilterExpression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public override T Accept<T>(IFilterExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override string ToString() => $"not ({Expression})";
    }

    // ==================== VALUE TYPES ====================

    /// <summary>
    /// String value in a filter (e.g., "john.doe")
    /// </summary>
    public class StringValue : FilterValue
    {
        public string Value { get; set; }

        public StringValue(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override object GetValue() => Value;
        public override FilterValueType GetValueType() => FilterValueType.String;
        public override string ToString() => $"\"{Value}\"";
    }

    /// <summary>
    /// Boolean value in a filter (e.g., true, false)
    /// </summary>
    public class BooleanValue : FilterValue
    {
        public bool Value { get; set; }

        public BooleanValue(bool value) => Value = value;

        public override object GetValue() => Value;
        public override FilterValueType GetValueType() => FilterValueType.Boolean;
        public override string ToString() => Value ? "true" : "false";
    }

    /// <summary>
    /// Numeric value in a filter (e.g., 12345, 50.5)
    /// </summary>
    public class NumericValue : FilterValue
    {
        public decimal Value { get; set; }

        public NumericValue(decimal value) => Value = value;

        public override object GetValue() => Value;
        public override FilterValueType GetValueType() => FilterValueType.Numeric;
        public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// DateTime value in a filter (e.g., "2024-01-15T10:00:00Z")
    /// </summary>
    public class DateTimeValue : FilterValue
    {
        public DateTime Value { get; set; }

        public DateTimeValue(DateTime value) => Value = value;

        public override object GetValue() => Value;
        public override FilterValueType GetValueType() => FilterValueType.DateTime;
        public override string ToString() => $"\"{Value:O}\"";
    }

    // ==================== ENUMS ====================

    /// <summary>
    /// Comparison operators supported in SCIM filters
    /// </summary>
    public enum FilterOperator
    {
        Equals,
        NotEquals,
        Contains,
        StartsWith,
        EndsWith,
        GreaterThan,
        GreaterOrEqual,
        LessThan,
        LessOrEqual
    }

    /// <summary>
    /// Types of values that can appear in filter expressions
    /// </summary>
    public enum FilterValueType
    {
        String,
        Boolean,
        Numeric,
        DateTime
    }

    // ==================== VISITOR PATTERN ====================

    /// <summary>
    /// Visitor pattern interface for traversing filter expressions
    /// </summary>
    public interface IFilterExpressionVisitor<T>
    {
        T Visit(ComparisonFilter filter);
        T Visit(PresenceFilter filter);
        T Visit(AndFilter filter);
        T Visit(OrFilter filter);
        T Visit(NotFilter filter);
    }
}
