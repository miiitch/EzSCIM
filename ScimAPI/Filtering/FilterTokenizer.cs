using System;
using System.Collections.Generic;

namespace ScimAPI.Filtering
{
    /// <summary>
    /// Token types for SCIM filter parsing
    /// </summary>
    public enum TokenType
    {
        AttributeName,
        Operator,
        Value,
        And,
        Or,
        Not,
        OpenParen,
        CloseParen,
        Pr,
        Eof
    }

    /// <summary>
    /// Represents a single token in a filter string
    /// </summary>
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => $"{Type}:{Value}";
    }

    /// <summary>
    /// Tokenizes SCIM filter strings using Span&lt;char&gt; (NO Substring allocations)
    /// </summary>
    public class FilterTokenizer
    {
        private readonly string _filter;
        private int _position;

        public FilterTokenizer(string filter)
        {
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _position = 0;
        }

        /// <summary>
        /// Tokenizes the filter string into a list of tokens
        /// </summary>
        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            var filterSpan = _filter.AsSpan();

            while (_position < filterSpan.Length)
            {
                SkipWhitespace(filterSpan);
                if (_position >= filterSpan.Length) break;

                switch (filterSpan[_position])
                {
                    case '(':
                        tokens.Add(new Token(TokenType.OpenParen, "("));
                        _position++;
                        break;
                    case ')':
                        tokens.Add(new Token(TokenType.CloseParen, ")"));
                        _position++;
                        break;
                    case '"':
                        tokens.Add(ParseStringToken(filterSpan));
                        break;
                    default:
                        var word = ParseWord(filterSpan);
                        if (word == null) break;
                        tokens.Add(ClassifyToken(word));
                        break;
                }
            }

            tokens.Add(new Token(TokenType.Eof, ""));
            return tokens;
        }

        /// <summary>
        /// Skips whitespace characters using Span navigation
        /// </summary>
        private void SkipWhitespace(ReadOnlySpan<char> filterSpan)
        {
            while (_position < filterSpan.Length && char.IsWhiteSpace(filterSpan[_position]))
                _position++;
        }

        /// <summary>
        /// Parses a string token (quoted value) using Span.Slice instead of Substring
        /// </summary>
        private Token ParseStringToken(ReadOnlySpan<char> filterSpan)
        {
            _position++; // Skip opening quote
            var start = _position;

            while (_position < filterSpan.Length && filterSpan[_position] != '"')
            {
                if (filterSpan[_position] == '\\' && _position + 1 < filterSpan.Length)
                    _position += 2;
                else
                    _position++;
            }

            // Use Span.Slice instead of Substring - NO allocation!
            var value = filterSpan.Slice(start, _position - start).ToString();
            _position++; // Skip closing quote
            return new Token(TokenType.Value, value);
        }

        /// <summary>
        /// Parses a word (attribute name, operator, keyword) using Span.Slice
        /// </summary>
        private string? ParseWord(ReadOnlySpan<char> filterSpan)
        {
            var start = _position;

            while (_position < filterSpan.Length &&
                   (char.IsLetterOrDigit(filterSpan[_position]) ||
                    filterSpan[_position] == '_' ||
                    filterSpan[_position] == '.' ||
                    filterSpan[_position] == '-'))
            {
                _position++;
            }

            if (_position <= start) return null;

            // Use Span.Slice instead of Substring - NO allocation!
            return filterSpan.Slice(start, _position - start).ToString();
        }

        /// <summary>
        /// Classifies a word token into its appropriate type
        /// </summary>
        private Token ClassifyToken(string word)
        {
            return word.ToLower() switch
            {
                "and" => new Token(TokenType.And, "and"),
                "or" => new Token(TokenType.Or, "or"),
                "not" => new Token(TokenType.Not, "not"),
                "pr" => new Token(TokenType.Pr, "pr"),
                "eq" or "ne" or "co" or "sw" or "ew" or "gt" or "ge" or "lt" or "le" =>
                    new Token(TokenType.Operator, word.ToLower()),
                "true" or "false" => new Token(TokenType.Value, word.ToLower()),
                _ when decimal.TryParse(word, out _) => new Token(TokenType.Value, word),
                _ => new Token(TokenType.AttributeName, word)
            };
        }
    }
}
