using System;
using System.Collections.Generic;
using ErrorOr;
using ScimAPI.Filtering.AST;

namespace ScimAPI.Filtering
{
    /// <summary>
    /// Exception thrown when filter parsing fails (DEPRECATED - use ErrorOr instead)
    /// </summary>
    [Obsolete("Use ErrorOr<FilterExpression> instead of throwing exceptions. This will be removed in a future version.")]
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
        /// <returns>An ErrorOr containing either the FilterExpression tree or parsing errors</returns>
        public ErrorOr<FilterExpression> Parse(string filterString)
        {
            if (string.IsNullOrWhiteSpace(filterString))
                return FilterErrors.EmptyFilter;

            try
            {
                var tokenizer = new FilterTokenizer(filterString);
                _tokens = tokenizer.Tokenize();
            }
            catch (Exception ex)
            {
                return FilterErrors.TokenizationFailed(ex.Message);
            }

            _current = 0;

            var expressionResult = ParseOrExpression();
            if (expressionResult.IsError)
                return expressionResult.Errors;

            if (!IsAtEnd())
                return FilterErrors.UnexpectedTokensAfterExpression(Peek().Position);

            return expressionResult.Value;
        }

        /// <summary>
        /// Parses OR expressions (lowest precedence)
        /// </summary>
        private ErrorOr<FilterExpression> ParseOrExpression()
        {
            var leftResult = ParseAndExpression();
            if (leftResult.IsError)
                return leftResult.Errors;

            var left = leftResult.Value;

            while (Match(TokenType.Or))
            {
                var rightResult = ParseAndExpression();
                if (rightResult.IsError)
                    return rightResult.Errors;

                left = new OrFilter(left, rightResult.Value);
            }

            return left;
        }

        /// <summary>
        /// Parses AND expressions (medium precedence)
        /// </summary>
        private ErrorOr<FilterExpression> ParseAndExpression()
        {
            var leftResult = ParseNotExpression();
            if (leftResult.IsError)
                return leftResult.Errors;

            var left = leftResult.Value;

            while (Match(TokenType.And))
            {
                var rightResult = ParseNotExpression();
                if (rightResult.IsError)
                    return rightResult.Errors;

                left = new AndFilter(left, rightResult.Value);
            }

            return left;
        }

        /// <summary>
        /// Parses NOT expressions (highest precedence)
        /// </summary>
        private ErrorOr<FilterExpression> ParseNotExpression()
        {
            if (Match(TokenType.Not))
            {
                var expressionResult = ParseNotExpression();
                if (expressionResult.IsError)
                    return expressionResult.Errors;

                return new NotFilter(expressionResult.Value);
            }

            return ParseComparisonOrPresenceExpression();
        }

        /// <summary>
        /// Parses comparison (eq, ne, co, sw, ew, gt, ge, lt, le) or presence (pr) filters
        /// Also handles parenthesized expressions
        /// </summary>
        private ErrorOr<FilterExpression> ParseComparisonOrPresenceExpression()
        {
            // Handle parenthesized expressions
            if (Match(TokenType.OpenParen))
            {
                var expressionResult = ParseOrExpression();
                if (expressionResult.IsError)
                    return expressionResult.Errors;

                if (!Match(TokenType.CloseParen))
                    return FilterErrors.MissingClosingParenthesis(Peek().Position);

                return expressionResult.Value;
            }

            // Expect attribute name
            var currentToken = Peek();
            if (currentToken.Type != TokenType.AttributeName)
                return FilterErrors.ExpectedAttributeName(currentToken.Type.ToString(), currentToken.Position);

            var attributeName = Advance().Value;

            // Check for 'pr' (presence) operator
            if (Match(TokenType.Pr))
            {
                return new PresenceFilter(attributeName);
            }

            // Expect comparison operator
            currentToken = Peek();
            if (currentToken.Type != TokenType.Operator)
                return FilterErrors.ExpectedOperator(currentToken.Type.ToString(), currentToken.Position);

            var operatorToken = Advance();
            var opResult = ParseOperator(operatorToken.Value, operatorToken.Position);
            if (opResult.IsError)
                return opResult.Errors;

            // Expect value
            currentToken = Peek();
            if (currentToken.Type != TokenType.Value)
                return FilterErrors.ExpectedValue(currentToken.Type.ToString(), currentToken.Position);

            var valueToken = Advance();
            var value = ParseValue(valueToken.Value);

            return new ComparisonFilter(attributeName, opResult.Value, value);
        }

        /// <summary>
        /// Parses an operator string to FilterOperator enum
        /// </summary>
        private ErrorOr<FilterOperator> ParseOperator(string op, int position)
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
                _ => FilterErrors.UnknownOperator(op, position)
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
