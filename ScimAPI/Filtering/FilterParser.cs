using System;
using System.Collections.Generic;
using ScimAPI.Filtering.AST;

namespace ScimAPI.Filtering
{
    /// <summary>
    /// Exception thrown when filter parsing fails
    /// </summary>
    public class FilterParseException : Exception
    {
        public FilterParseException(string message) : base(message) { }
    }

    /// <summary>
    /// SCIM filter parser - converts filter strings to AST (Abstract Syntax Tree)
    /// Handles operator precedence: not > and > or
    /// Supports nested expressions with parentheses
    /// </summary>
    public class FilterParser
    {
        private List<Token> _tokens;
        private int _current;

        public FilterParser()
        {
            _tokens = new List<Token>();
            _current = 0;
        }

        /// <summary>
        /// Parses a SCIM filter string into an AST
        /// </summary>
        /// <param name="filterString">The filter string to parse</param>
        /// <returns>A FilterExpression tree representing the filter</returns>
        public FilterExpression Parse(string filterString)
        {
            if (string.IsNullOrWhiteSpace(filterString))
                throw new FilterParseException("Filter string cannot be empty");

            var tokenizer = new FilterTokenizer(filterString);
            _tokens = tokenizer.Tokenize();
            _current = 0;

            var expression = ParseOrExpression();

            if (!IsAtEnd())
                throw new FilterParseException("Unexpected tokens after filter expression");

            return expression;
        }

        /// <summary>
        /// Parses OR expressions (lowest precedence)
        /// </summary>
        private FilterExpression ParseOrExpression()
        {
            var left = ParseAndExpression();

            while (Match(TokenType.Or))
            {
                var right = ParseAndExpression();
                left = new OrFilter(left, right);
            }

            return left;
        }

        /// <summary>
        /// Parses AND expressions (medium precedence)
        /// </summary>
        private FilterExpression ParseAndExpression()
        {
            var left = ParseNotExpression();

            while (Match(TokenType.And))
            {
                var right = ParseNotExpression();
                left = new AndFilter(left, right);
            }

            return left;
        }

        /// <summary>
        /// Parses NOT expressions (highest precedence)
        /// </summary>
        private FilterExpression ParseNotExpression()
        {
            if (Match(TokenType.Not))
            {
                var expression = ParseNotExpression();
                return new NotFilter(expression);
            }

            return ParseComparisonOrPresenceExpression();
        }

        /// <summary>
        /// Parses comparison (eq, ne, co, sw, ew, gt, ge, lt, le) or presence (pr) filters
        /// Also handles parenthesized expressions
        /// </summary>
        private FilterExpression ParseComparisonOrPresenceExpression()
        {
            // Handle parenthesized expressions
            if (Match(TokenType.OpenParen))
            {
                var expression = ParseOrExpression();
                if (!Match(TokenType.CloseParen))
                    throw new FilterParseException("Expected closing parenthesis");
                return expression;
            }

            // Expect attribute name
            if (Peek().Type != TokenType.AttributeName)
                throw new FilterParseException($"Expected attribute name, got {Peek().Type}");

            var attributeName = Advance().Value;

            // Check for 'pr' (presence) operator
            if (Match(TokenType.Pr))
            {
                return new PresenceFilter(attributeName);
            }

            // Expect comparison operator
            if (Peek().Type != TokenType.Operator)
                throw new FilterParseException($"Expected operator, got {Peek().Type}");

            var operatorToken = Advance().Value;
            var op = ParseOperator(operatorToken);

            // Expect value
            if (Peek().Type != TokenType.Value)
                throw new FilterParseException($"Expected value, got {Peek().Type}");

            var valueToken = Advance();
            var value = ParseValue(valueToken.Value);

            return new ComparisonFilter(attributeName, op, value);
        }

        /// <summary>
        /// Parses an operator string to FilterOperator enum
        /// </summary>
        private FilterOperator ParseOperator(string op)
        {
            return op switch
            {
                "eq" => FilterOperator.Equals,
                "ne" => FilterOperator.NotEquals,
                "co" => FilterOperator.Contains,
                "sw" => FilterOperator.StartsWith,
                "ew" => FilterOperator.EndsWith,
                "gt" => FilterOperator.GreaterThan,
                "ge" => FilterOperator.GreaterOrEqual,
                "lt" => FilterOperator.LessThan,
                "le" => FilterOperator.LessOrEqual,
                _ => throw new FilterParseException($"Unknown operator: {op}")
            };
        }

        /// <summary>
        /// Parses a value string to the appropriate FilterValue type
        /// Automatically detects: boolean, DateTime, numeric, or string
        /// </summary>
        private FilterValue ParseValue(string valueStr)
        {
            // Check boolean
            if (valueStr == "true") return new BooleanValue(true);
            if (valueStr == "false") return new BooleanValue(false);

            // Check DateTime (ISO 8601)
            if (DateTime.TryParse(valueStr, out var dateTime))
                return new DateTimeValue(dateTime);

            // Check numeric
            if (decimal.TryParse(valueStr, out var number))
                return new NumericValue(number);

            // Default to string
            return new StringValue(valueStr);
        }

        /// <summary>
        /// Checks if current token matches given type and advances if it does
        /// </summary>
        private bool Match(TokenType type)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if current token matches given type without advancing
        /// </summary>
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        /// <summary>
        /// Advances to next token and returns previous token
        /// </summary>
        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        /// <summary>
        /// Checks if we've reached the end of tokens
        /// </summary>
        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.Eof;
        }

        /// <summary>
        /// Returns current token without advancing
        /// </summary>
        private Token Peek()
        {
            return _tokens[_current];
        }

        /// <summary>
        /// Returns previous token
        /// </summary>
        private Token Previous()
        {
            return _tokens[_current - 1];
        }
    }
}
