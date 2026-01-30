using ScimAPI.Filtering.AST;

namespace ScimAPI.Filtering.Visitors
{
    /// <summary>
    /// Visitor that prints the AST structure in a readable hierarchical format
    /// </summary>
    public class PrintVisitor : IFilterExpressionVisitor<string>
    {
        private int _depth;

        public string Visit(ComparisonFilter filter)
        {
            var indentation = new string(' ', _depth * 2);
            return $"{indentation}Comparison: {filter.AttributeName} {GetOperatorName(filter.Operator)} {filter.Value}\n";
        }

        public string Visit(PresenceFilter filter)
        {
            var indentation = new string(' ', _depth * 2);
            return $"{indentation}Presence: {filter.AttributeName} pr\n";
        }

        public string Visit(AndFilter filter)
        {
            var indentation = new string(' ', _depth * 2);
            _depth++;
            var left = filter.Left.Accept(this);
            var right = filter.Right.Accept(this);
            _depth--;
            return $"{indentation}AND\n{left}{right}";
        }

        public string Visit(OrFilter filter)
        {
            var indentation = new string(' ', _depth * 2);
            _depth++;
            var left = filter.Left.Accept(this);
            var right = filter.Right.Accept(this);
            _depth--;
            return $"{indentation}OR\n{left}{right}";
        }

        public string Visit(NotFilter filter)
        {
            var indentation = new string(' ', _depth * 2);
            _depth++;
            var expr = filter.Expression.Accept(this);
            _depth--;
            return $"{indentation}NOT\n{expr}";
        }

        private static string GetOperatorName(FilterOperator op) => op switch
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
}
