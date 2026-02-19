using ErrorOr;

namespace EzSCIM.Filtering
{
    /// <summary>
    /// Static class containing all possible filter parsing errors with position information
    /// </summary>
    public static class FilterErrors
    {
        /// <summary>
        /// Error when the filter string is empty or whitespace
        /// </summary>
        public static Error EmptyFilter => Error.Validation(
            code: "Filter.Empty",
            description: "Filter string cannot be empty");

        /// <summary>
        /// Error when an unexpected token is encountered
        /// </summary>
        public static Error UnexpectedToken(string expected, string actual, int position) => Error.Validation(
            code: "Filter.UnexpectedToken",
            description: $"Expected {expected}, but got {actual} at position {position}");

        /// <summary>
        /// Error when a closing parenthesis is missing
        /// </summary>
        public static Error MissingClosingParenthesis(int position) => Error.Validation(
            code: "Filter.MissingClosingParenthesis",
            description: $"Expected closing parenthesis at position {position}");

        /// <summary>
        /// Error when an attribute name is expected but not found
        /// </summary>
        public static Error ExpectedAttributeName(string actual, int position) => Error.Validation(
            code: "Filter.ExpectedAttributeName",
            description: $"Expected attribute name, but got '{actual}' at position {position}");

        /// <summary>
        /// Error when an operator is expected but not found
        /// </summary>
        public static Error ExpectedOperator(string actual, int position) => Error.Validation(
            code: "Filter.ExpectedOperator",
            description: $"Expected operator, but got '{actual}' at position {position}");

        /// <summary>
        /// Error when a value is expected but not found
        /// </summary>
        public static Error ExpectedValue(string actual, int position) => Error.Validation(
            code: "Filter.ExpectedValue",
            description: $"Expected value, but got '{actual}' at position {position}");

        /// <summary>
        /// Error when an unknown operator is encountered
        /// </summary>
        public static Error UnknownOperator(string op, int position) => Error.Validation(
            code: "Filter.UnknownOperator",
            description: $"Unknown operator '{op}' at position {position}");

        /// <summary>
        /// Error when there are unexpected tokens after the filter expression
        /// </summary>
        public static Error UnexpectedTokensAfterExpression(int position) => Error.Validation(
            code: "Filter.UnexpectedTokensAfterExpression",
            description: $"Unexpected tokens after filter expression at position {position}");

        /// <summary>
        /// Error when tokenization fails
        /// </summary>
        public static Error TokenizationFailed(string message) => Error.Validation(
            code: "Filter.TokenizationFailed",
            description: $"Tokenization failed: {message}");

        /// <summary>
        /// Error when parsing fails due to invalid syntax
        /// </summary>
        public static Error InvalidSyntax(string message, int position) => Error.Validation(
            code: "Filter.InvalidSyntax",
            description: $"Invalid syntax: {message} at position {position}");
    }
}
