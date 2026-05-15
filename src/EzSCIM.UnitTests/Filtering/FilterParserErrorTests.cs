using Shouldly;
using Xunit;
using EzSCIM.Filtering;

namespace EzSCIM.UnitTests.Filtering;

/// <summary>
/// Comprehensive tests for FilterParser error handling using ErrorOr
/// Tests all error codes defined in FilterErrors
/// </summary>
public class FilterParserErrorTests
{
    private readonly FilterParser _parser = new();

    // ==================== EMPTY FILTER TESTS ====================

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyFilterError()
    {
        // Act
        var result = _parser.Parse("");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        
        var error = result.FirstError;
        error.Code.ShouldBe("Filter.Empty");
        error.Type.ShouldBe(ErrorOr.ErrorType.Validation);
        error.Description.ShouldContain("cannot be empty");
    }

    [Fact]
    public void Parse_WhitespaceString_ReturnsEmptyFilterError()
    {
        // Act
        var result = _parser.Parse("   ");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.Empty");
        result.FirstError.Description.ShouldContain("cannot be empty");
    }

    [Fact]
    public void Parse_TabsAndSpaces_ReturnsEmptyFilterError()
    {
        // Act
        var result = _parser.Parse("\t  \t  ");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.Empty");
    }

    [Fact]
    public void Parse_NewlinesOnly_ReturnsEmptyFilterError()
    {
        // Act
        var result = _parser.Parse("\n\r\n");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.Empty");
    }

    // ==================== MISSING CLOSING PARENTHESIS TESTS ====================

    [Fact]
    public void Parse_MissingClosingParenthesis_Simple_ReturnsError()
    {
        // Act
        var result = _parser.Parse("(active eq true");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.MissingClosingParenthesis");
        result.FirstError.Description.ShouldContain("closing parenthesis");
        result.FirstError.Description.ShouldContain("position");
    }

    [Fact]
    public void Parse_MissingClosingParenthesis_Nested_ReturnsError()
    {
        // Act
        var result = _parser.Parse("(active eq true and (title eq \"Admin\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.MissingClosingParenthesis");
    }

    [Fact]
    public void Parse_MissingClosingParenthesis_Complex_ReturnsError()
    {
        // Act
        var result = _parser.Parse("((active eq true and title eq \"Admin\") or userName eq \"john\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.MissingClosingParenthesis");
    }

    [Fact]
    public void Parse_MissingClosingParenthesis_AfterNot_ReturnsError()
    {
        // Act
        var result = _parser.Parse("active eq true and not (userName sw \"admin\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.MissingClosingParenthesis");
    }

    // ==================== EXPECTED ATTRIBUTE NAME TESTS ====================

    [Fact]
    public void Parse_ExpectedAttributeName_StartsWithOperator_ReturnsError()
    {
        // Act
        var result = _parser.Parse("eq \"value\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
        result.FirstError.Description.ShouldContain("Expected attribute name");
        result.FirstError.Description.ShouldContain("position");
    }

    [Fact]
    public void Parse_ExpectedAttributeName_StartsWithValue_ReturnsError()
    {
        // Act
        var result = _parser.Parse("\"value\" eq something");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    [Fact]
    public void Parse_ExpectedAttributeName_StartsWithAnd_ReturnsError()
    {
        // Act
        var result = _parser.Parse("and userName eq \"john\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    [Fact]
    public void Parse_ExpectedAttributeName_StartsWithOr_ReturnsError()
    {
        // Act
        var result = _parser.Parse("or title eq \"Admin\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    [Fact]
    public void Parse_ExpectedAttributeName_EmptyParentheses_ReturnsError()
    {
        // Act
        var result = _parser.Parse("()");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    // ==================== EXPECTED OPERATOR TESTS ====================

    [Fact]
    public void Parse_ExpectedOperator_AttributeThenValue_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName \"john\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedOperator");
        result.FirstError.Description.ShouldContain("Expected operator");
        result.FirstError.Description.ShouldContain("position");
    }

    [Fact]
    public void Parse_ExpectedOperator_AttributeThenAttribute_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName title");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedOperator");
    }

    [Fact]
    public void Parse_ExpectedOperator_InvalidOperatorString_ReturnsError()
    {
        // Act - "xx" is not a valid operator, so it's tokenized as AttributeName
        var result = _parser.Parse("userName xx \"value\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedOperator");
    }

    [Fact]
    public void Parse_ExpectedOperator_AttributeThenAnd_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName and title eq \"Admin\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedOperator");
    }

    [Fact]
    public void Parse_ExpectedOperator_AttributeThenOr_ReturnsError()
    {
        // Act
        var result = _parser.Parse("title or userName eq \"john\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedOperator");
    }

    [Fact]
    public void Parse_ExpectedOperator_AttributeThenNot_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName not active eq true");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedOperator");
    }

    // ==================== EXPECTED VALUE TESTS ====================

    [Fact]
    public void Parse_ExpectedValue_OperatorAtEnd_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName eq");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
        result.FirstError.Description.ShouldContain("Expected value");
        result.FirstError.Description.ShouldContain("position");
    }

    [Fact]
    public void Parse_ExpectedValue_OperatorThenOperator_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName eq ne");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
    }

    [Fact]
    public void Parse_ExpectedValue_OperatorThenAttribute_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName eq title");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
    }

    [Fact]
    public void Parse_ExpectedValue_OperatorThenAnd_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName eq and title eq \"Admin\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
    }

    [Fact]
    public void Parse_ExpectedValue_OperatorThenOr_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName eq or title eq \"Admin\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
    }

    [Fact]
    public void Parse_ExpectedValue_OperatorThenNot_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName eq not");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
    }

    [Fact]
    public void Parse_ExpectedValue_OperatorThenOpenParen_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName eq (");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
    }

    [Fact]
    public void Parse_ExpectedValue_OperatorThenCloseParen_ReturnsError()
    {
        // Act
        var result = _parser.Parse("(userName eq )");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
    }

    // ==================== UNEXPECTED TOKENS AFTER EXPRESSION TESTS ====================

    [Fact]
    public void Parse_UnexpectedTokensAfterExpression_ExtraAttribute_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName eq \"john\" extraStuff");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.UnexpectedTokensAfterExpression");
        result.FirstError.Description.ShouldContain("Unexpected tokens");
        result.FirstError.Description.ShouldContain("position");
    }

    [Fact]
    public void Parse_UnexpectedTokensAfterExpression_ExtraValue_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName eq \"john\" \"extra\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.UnexpectedTokensAfterExpression");
    }

    [Fact]
    public void Parse_UnexpectedTokensAfterExpression_ExtraCloseParen_ReturnsError()
    {
        // Act
        var result = _parser.Parse("(userName eq \"john\"))");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.UnexpectedTokensAfterExpression");
    }

    [Fact]
    public void Parse_UnexpectedTokensAfterExpression_CompleteExpressionPlusGarbage_ReturnsError()
    {
        // Act
        var result = _parser.Parse("active eq true and title eq \"Admin\" garbage");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.UnexpectedTokensAfterExpression");
    }

    // ==================== POSITION INFORMATION TESTS ====================

    [Fact]
    public void Parse_ErrorIncludesPosition_MissingClosingParen()
    {
        // Act
        var result = _parser.Parse("(userName eq \"john\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        var errorDescription = result.FirstError.Description;
        errorDescription.ShouldContain("position");
        
        // Position should be at or after position 0
        var positionMatch = System.Text.RegularExpressions.Regex.Match(
            errorDescription, 
            @"position (\d+)"
        );
        positionMatch.Success.ShouldBeTrue();
        var position = int.Parse(positionMatch.Groups[1].Value);
        position.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Parse_ErrorIncludesPosition_ExpectedValue()
    {
        // Act
        var filterString = "userName eq";
        var result = _parser.Parse(filterString);
        
        // Assert
        result.IsError.ShouldBeTrue();
        var errorDescription = result.FirstError.Description;
        errorDescription.ShouldContain("position");
        
        // Position should be at the end of the string
        var positionMatch = System.Text.RegularExpressions.Regex.Match(
            errorDescription, 
            @"position (\d+)"
        );
        positionMatch.Success.ShouldBeTrue();
        var position = int.Parse(positionMatch.Groups[1].Value);
        position.ShouldBe(filterString.Length);
    }

    [Fact]
    public void Parse_ErrorIncludesPosition_ExpectedOperator()
    {
        // Act
        var result = _parser.Parse("userName \"value\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        var errorDescription = result.FirstError.Description;
        errorDescription.ShouldContain("position");
        
        // Position should indicate where the value was found instead of operator
        var positionMatch = System.Text.RegularExpressions.Regex.Match(
            errorDescription, 
            @"position (\d+)"
        );
        positionMatch.Success.ShouldBeTrue();
        var position = int.Parse(positionMatch.Groups[1].Value);
        position.ShouldBeGreaterThan(0); // After "userName "
    }

    // ==================== COMPREHENSIVE ERROR CODE THEORY TEST ====================

    [Theory]
    [InlineData("", "Filter.Empty", "cannot be empty")]
    [InlineData("   ", "Filter.Empty", "cannot be empty")]
    [InlineData("(userName eq \"john\"", "Filter.MissingClosingParenthesis", "closing parenthesis")]
    [InlineData("userName eq", "Filter.ExpectedValue", "Expected value")]
    [InlineData("userName \"value\"", "Filter.ExpectedOperator", "Expected operator")]
    [InlineData("eq \"value\"", "Filter.ExpectedAttributeName", "Expected attribute name")]
    [InlineData("userName eq \"john\" extra", "Filter.UnexpectedTokensAfterExpression", "Unexpected tokens")]
    public void Parse_ErrorScenarios_ReturnsCorrectErrorCodeAndMessage(
        string filter, 
        string expectedCode, 
        string expectedMessageFragment)
    {
        // Act
        var result = _parser.Parse(filter);
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(expectedCode);
        result.FirstError.Description.ShouldContain(expectedMessageFragment);
    }

    // ==================== ERROR TYPE TESTS ====================

    [Fact]
    public void Parse_AllErrors_AreValidationType()
    {
        // Arrange
        var testCases = new[]
        {
            "",
            "(userName eq \"john\"",
            "userName eq",
            "userName \"value\"",
            "eq \"value\"",
            "userName eq \"john\" extra"
        };
        
        // Act & Assert
        foreach (var testCase in testCases)
        {
            var result = _parser.Parse(testCase);
            result.IsError.ShouldBeTrue($"Filter '{testCase}' should return error");
            result.FirstError.Type.ShouldBe(
                ErrorOr.ErrorType.Validation,
                $"Filter '{testCase}' error should be Validation type"
            );
        }
    }

    // ==================== COMPLEX ERROR SCENARIOS ====================

    [Fact]
    public void Parse_ComplexNested_MissingParen_ReturnsError()
    {
        // Act
        var result = _parser.Parse(
            "((active eq true and (title eq \"Admin\" or title eq \"Manager\")) " +
            "or (active eq false and title eq \"Director\")"
        );
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.MissingClosingParenthesis");
    }

    [Fact]
    public void Parse_MultipleLogicalOperators_MissingValue_ReturnsError()
    {
        // Act
        var result = _parser.Parse("active eq true and userName eq and title eq \"Admin\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
    }

    [Fact]
    public void Parse_NotOperator_MissingExpression_ReturnsError()
    {
        // Act
        var result = _parser.Parse("not");
        
        // Assert
        result.IsError.ShouldBeTrue();
        // "not" keyword expects an expression (which starts with attribute or paren)
        // When it hits EOF, it expects an attribute name
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    [Fact]
    public void Parse_PresenceOperator_WithValue_ReturnsError()
    {
        // Act - "pr" should not be followed by a value
        // But actually in SCIM, "pr" is a unary operator, so this is valid: "userName pr"
        // Let's test an invalid case: trying to use pr with comparison structure
        var result = _parser.Parse("userName pr \"value\"");
        
        // Assert - After pr, there should be no more tokens (or logical operator)
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.UnexpectedTokensAfterExpression");
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Parse_OnlyParentheses_ReturnsError()
    {
        // Act
        var result = _parser.Parse("()");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    [Fact]
    public void Parse_OnlyOpenParenthesis_ReturnsError()
    {
        // Act
        var result = _parser.Parse("(");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    [Fact]
    public void Parse_OnlyCloseParenthesis_ReturnsError()
    {
        // Act
        var result = _parser.Parse(")");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    [Fact]
    public void Parse_OnlyLogicalOperator_And_ReturnsError()
    {
        // Act
        var result = _parser.Parse("and");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    [Fact]
    public void Parse_OnlyLogicalOperator_Or_ReturnsError()
    {
        // Act
        var result = _parser.Parse("or");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    [Fact]
    public void Parse_OnlyLogicalOperator_Not_ReturnsError()
    {
        // Act
        var result = _parser.Parse("not");
        
        // Assert
        result.IsError.ShouldBeTrue();
        // "not" expects an expression after it (attribute or paren), but gets EOF
        result.FirstError.Code.ShouldBe("Filter.ExpectedAttributeName");
    }

    // ==================== ERROR OBJECT VERIFICATION ====================

    [Fact]
    public void Parse_ErrorObject_HasCorrectProperties()
    {
        // Act
        var result = _parser.Parse("");
        
        // Assert
        result.IsError.ShouldBeTrue();
        
        var error = result.FirstError;
        error.Code.ShouldNotBeNullOrEmpty();
        error.Description.ShouldNotBeNullOrEmpty();
        error.Type.ShouldBe(ErrorOr.ErrorType.Validation);
        
        // Error codes should follow pattern "Filter.*"
        error.Code.ShouldStartWith("Filter.");
    }

    [Fact]
    public void Parse_ErrorsList_ContainsOnlyOneError()
    {
        // Act
        var result = _parser.Parse("userName eq");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        
        // FirstError should be the same as Errors[0]
        result.FirstError.ShouldBe(result.Errors[0]);
    }

    // ==================== NO FALSE POSITIVES ====================

    [Theory]
    [InlineData("userName eq \"john\"")]
    [InlineData("active eq true")]
    [InlineData("id eq 12345")]
    [InlineData("phoneNumbers pr")]
    [InlineData("(userName eq \"john\")")]
    [InlineData("active eq true and title eq \"Admin\"")]
    [InlineData("userName eq \"john\" or userName eq \"jane\"")]
    [InlineData("not (active eq false)")]
    public void Parse_ValidFilters_DoNotReturnErrors(string filter)
    {
        // Act
        var result = _parser.Parse(filter);
        
        // Assert
        result.IsError.ShouldBeFalse($"Valid filter '{filter}' should not return error");
        result.Value.ShouldNotBeNull();
    }
}
