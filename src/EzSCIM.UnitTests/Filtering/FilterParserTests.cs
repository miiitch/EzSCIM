using Shouldly;
using Xunit;
using EzSCIM.Filtering;
using EzSCIM.Filtering.AST;
using EzSCIM.Filtering.Visitors;

namespace EzSCIM.UnitTests.Filtering;

public class FilterParserTests
{
    private readonly FilterParser _parser = new();

    // ==================== SIMPLE COMPARISON TESTS ====================

    [Fact]
    public void Parse_SimpleEquals_WithString()
    {
        // Arrange
        var expected = F.Equals("userName", "john.doe");
        
        // Act
        var result = _parser.Parse("userName eq \"john.doe\"");
        
        // Assert
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_SimpleEquals_WithBoolean()
    {
        // Arrange
        var expected = F.Equals("active", true);
        
        // Act
        var result = _parser.Parse("active eq true");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_SimpleEquals_WithNumeric()
    {
        // Arrange
        var expected = F.Equals("id", 12345);
        
        // Act
        var result = _parser.Parse("id eq 12345");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_Contains_Filter()
    {
        // Arrange
        var expected = F.Contains("displayName", "John");
        
        // Act
        var result = _parser.Parse("displayName co \"John\"");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_StartsWith_Filter()
    {
        // Arrange
        var expected = F.StartsWith("userName", "admin");
        
        // Act
        var result = _parser.Parse("userName sw \"admin\"");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_EndsWith_Filter()
    {
        // Arrange
        var expected = F.EndsWith("emails.value", "@company.com");
        
        // Act
        var result = _parser.Parse("emails.value ew \"@company.com\"");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_GreaterThan_Filter()
    {
        // Arrange
        var expected = F.GreaterThan("id", 1000);
        
        // Act
        var result = _parser.Parse("id gt 1000");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_LessOrEqual_Filter()
    {
        // Arrange
        var expected = F.LessOrEqual("salary", 50000);
        
        // Act
        var result = _parser.Parse("salary le 50000");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    // ==================== PRESENCE FILTER TESTS ====================

    [Fact]
    public void Parse_Presence_Filter()
    {
        // Arrange
        var expected = F.Present("phoneNumbers");
        
        // Act
        var result = _parser.Parse("phoneNumbers pr");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_Presence_WithDottedAttribute()
    {
        // Arrange
        var expected = F.Present("emails.value");
        
        // Act
        var result = _parser.Parse("emails.value pr");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    // ==================== LOGICAL OPERATORS TESTS ====================

    [Fact]
    public void Parse_And_TwoFilters()
    {
        // Arrange
        var expected = F.Equals("active", true).And(F.Equals("userName", "john"));
        
        // Act
        var result = _parser.Parse("active eq true and userName eq \"john\"");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_Or_TwoFilters()
    {
        // Arrange
        var expected = F.Equals("title", "Admin").Or(F.Equals("title", "Manager"));
        
        // Act
        var result = _parser.Parse("title eq \"Admin\" or title eq \"Manager\"");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_Not_Filter()
    {
        // Arrange
        var expected = F.Equals("active", false).Negate();
        
        // Act
        var result = _parser.Parse("not (active eq false)");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    // ==================== NESTED EXPRESSIONS TESTS ====================

    [Fact]
    public void Parse_And_Or_Nested()
    {
        // Arrange
        var expected = F.Equals("active", true)
            .And(F.Equals("title", "Admin").Or(F.Equals("title", "Manager")));
        
        // Act
        var result = _parser.Parse("active eq true and (title eq \"Admin\" or title eq \"Manager\")");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_Complex_Nested()
    {
        // Arrange
        var expected = F.Equals("active", true)
            .And(F.Equals("title", "Admin").Or(F.Equals("title", "Manager")))
            .Or(F.Equals("active", false).And(F.Equals("title", "Director")))
            .And(F.Equals("department", "Engineering"));
        
        // Act
        var result = _parser.Parse("((active eq true and (title eq \"Admin\" or title eq \"Manager\")) or (active eq false and title eq \"Director\")) and department eq \"Engineering\"");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_Not_With_And()
    {
        // Arrange
        var expected = F.Equals("active", true).And(F.StartsWith("userName", "admin").Negate());
        
        // Act
        var result = _parser.Parse("active eq true and not (userName sw \"admin\")");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    // ==================== OPERATOR PRECEDENCE TESTS ====================

    [Fact]
    public void Parse_And_HigherPrecedenceThan_Or()
    {
        // active eq true and title eq "Admin" or title eq "Manager"
        // Should parse as: (active eq true and title eq "Admin") or (title eq "Manager")
        
        // Arrange
        var expected = F.Equals("active", true)
            .And(F.Equals("title", "Admin"))
            .Or(F.Equals("title", "Manager"));
        
        // Act
        var result = _parser.Parse("active eq true and title eq \"Admin\" or title eq \"Manager\"");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void Parse_Not_HigherPrecedenceThan_And()
    {
        // not active eq false and title eq "Admin"
        // Should parse as: (not (active eq false)) and (title eq "Admin")
        
        // Arrange
        var expected = F.Equals("active", false).Negate()
            .And(F.Equals("title", "Admin"));
        
        // Act
        var result = _parser.Parse("not active eq false and title eq \"Admin\"");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    // ==================== ERROR HANDLING TESTS ====================

    [Fact]
    public void Parse_EmptyString_ReturnsError()
    {
        // Act
        var result = _parser.Parse("");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.Empty");
        result.FirstError.Description.ShouldContain("cannot be empty");
    }

    [Fact]
    public void Parse_MissingClosingParen_ReturnsError()
    {
        // Act
        var result = _parser.Parse("(active eq true");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.MissingClosingParenthesis");
        result.FirstError.Description.ShouldContain("closing parenthesis");
    }

    [Fact]
    public void Parse_InvalidOperator_ReturnsError()
    {
        // Act - "xx" is tokenized as AttributeName, not Operator, so parser expects an operator after it
        var result = _parser.Parse("userName xx \"john\"");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedOperator");
        result.FirstError.Description.ShouldContain("Expected operator");
    }

    [Fact]
    public void Parse_MissingValue_ReturnsError()
    {
        // Act
        var result = _parser.Parse("userName eq");
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
        result.FirstError.Description.ShouldContain("Expected value");
    }

    [Theory]
    [InlineData("", "Filter.Empty")]
    [InlineData("   ", "Filter.Empty")]
    [InlineData("(active eq true", "Filter.MissingClosingParenthesis")]
    [InlineData("userName eq", "Filter.ExpectedValue")]
    [InlineData("eq \"value\"", "Filter.ExpectedAttributeName")]
    [InlineData("userName \"value\"", "Filter.ExpectedOperator")]
    public void Parse_InvalidFilter_ReturnsCorrectErrorCode(string filter, string expectedErrorCode)
    {
        // Act
        var result = _parser.Parse(filter);
        
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(expectedErrorCode);
    }

    // ==================== FILTER BUILDER (F class) TESTS ====================

    [Fact]
    public void FilterBuilder_Equals_String()
    {
        var filter = F.Equals("userName", "john");
        filter.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)filter;
        comp.AttributeName.ShouldBe("userName");
        comp.Operator.ShouldBe(FilterOperator.Equals);
        comp.Value.ShouldBeOfType<StringValue>();
        ((StringValue)comp.Value).Value.ShouldBe("john");
    }

    [Fact]
    public void FilterBuilder_Equals_Boolean()
    {
        var filter = F.Equals("active", true);
        filter.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)filter;
        comp.AttributeName.ShouldBe("active");
        comp.Operator.ShouldBe(FilterOperator.Equals);
        comp.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)comp.Value).Value.ShouldBe(true);
    }

    [Fact]
    public void FilterBuilder_Contains()
    {
        var filter = F.Contains("displayName", "John");
        filter.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)filter;
        comp.AttributeName.ShouldBe("displayName");
        comp.Operator.ShouldBe(FilterOperator.Contains);
        comp.Value.ShouldBeOfType<StringValue>();
        ((StringValue)comp.Value).Value.ShouldBe("John");
    }

    [Fact]
    public void FilterBuilder_Present()
    {
        var filter = F.Present("phoneNumbers");
        filter.ShouldBeOfType<PresenceFilter>();
        var pres = (PresenceFilter)filter;
        pres.AttributeName.ShouldBe("phoneNumbers");
    }

    [Fact]
    public void FilterBuilder_And()
    {
        var f1 = F.Equals("active", true);
        var f2 = F.Equals("title", "Admin");
        var result = f1.And(f2); // Use fluent syntax
        result.ShouldBeOfType<AndFilter>();
        var and = (AndFilter)result;
        
        // Verify Left side
        and.Left.ShouldBeOfType<ComparisonFilter>();
        var left = (ComparisonFilter)and.Left;
        left.AttributeName.ShouldBe("active");
        left.Operator.ShouldBe(FilterOperator.Equals);
        left.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)left.Value).Value.ShouldBe(true);
        
        // Verify Right side
        and.Right.ShouldBeOfType<ComparisonFilter>();
        var right = (ComparisonFilter)and.Right;
        right.AttributeName.ShouldBe("title");
        right.Operator.ShouldBe(FilterOperator.Equals);
        right.Value.ShouldBeOfType<StringValue>();
        ((StringValue)right.Value).Value.ShouldBe("Admin");
    }

    [Fact]
    public void FilterBuilder_Fluent_And()
    {
        var result = F.Equals("active", true)
            .And(F.Equals("title", "Admin"));
        result.ShouldBeOfType<AndFilter>();
        var and = (AndFilter)result;
        
        // Verify Left side
        and.Left.ShouldBeOfType<ComparisonFilter>();
        var left = (ComparisonFilter)and.Left;
        left.AttributeName.ShouldBe("active");
        left.Operator.ShouldBe(FilterOperator.Equals);
        left.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)left.Value).Value.ShouldBe(true);
        
        // Verify Right side
        and.Right.ShouldBeOfType<ComparisonFilter>();
        var right = (ComparisonFilter)and.Right;
        right.AttributeName.ShouldBe("title");
        right.Operator.ShouldBe(FilterOperator.Equals);
        right.Value.ShouldBeOfType<StringValue>();
        ((StringValue)right.Value).Value.ShouldBe("Admin");
    }

    [Fact]
    public void FilterBuilder_Fluent_Or()
    {
        var result = F.Equals("title", "Admin")
            .Or(F.Equals("title", "Manager"));
        result.ShouldBeOfType<OrFilter>();
        var or = (OrFilter)result;
        
        // Verify Left side
        or.Left.ShouldBeOfType<ComparisonFilter>();
        var left = (ComparisonFilter)or.Left;
        left.AttributeName.ShouldBe("title");
        left.Operator.ShouldBe(FilterOperator.Equals);
        left.Value.ShouldBeOfType<StringValue>();
        ((StringValue)left.Value).Value.ShouldBe("Admin");
        
        // Verify Right side
        or.Right.ShouldBeOfType<ComparisonFilter>();
        var right = (ComparisonFilter)or.Right;
        right.AttributeName.ShouldBe("title");
        right.Operator.ShouldBe(FilterOperator.Equals);
        right.Value.ShouldBeOfType<StringValue>();
        ((StringValue)right.Value).Value.ShouldBe("Manager");
    }

    [Fact]
    public void FilterBuilder_Fluent_Negate()
    {
        var result = F.Equals("active", false).Negate();
        result.ShouldBeOfType<NotFilter>();
        var not = (NotFilter)result;
        
        // Verify Expression
        not.Expression.ShouldBeOfType<ComparisonFilter>();
        var expr = (ComparisonFilter)not.Expression;
        expr.AttributeName.ShouldBe("active");
        expr.Operator.ShouldBe(FilterOperator.Equals);
        expr.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)expr.Value).Value.ShouldBe(false);
    }

    [Fact]
    public void FilterBuilder_Complex_Fluent()
    {
        var result = F.Equals("active", true)
            .And(F.Equals("title", "Admin").Or(F.Equals("title", "Manager")));
        result.ShouldBeOfType<AndFilter>();
        var and = (AndFilter)result;
        
        // Verify Left side
        and.Left.ShouldBeOfType<ComparisonFilter>();
        var left = (ComparisonFilter)and.Left;
        left.AttributeName.ShouldBe("active");
        left.Operator.ShouldBe(FilterOperator.Equals);
        left.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)left.Value).Value.ShouldBe(true);
        
        // Verify Right side is OR
        and.Right.ShouldBeOfType<OrFilter>();
        var orFilter = (OrFilter)and.Right;
        
        // Verify OR's left side
        orFilter.Left.ShouldBeOfType<ComparisonFilter>();
        var orLeft = (ComparisonFilter)orFilter.Left;
        orLeft.AttributeName.ShouldBe("title");
        orLeft.Operator.ShouldBe(FilterOperator.Equals);
        orLeft.Value.ShouldBeOfType<StringValue>();
        ((StringValue)orLeft.Value).Value.ShouldBe("Admin");
        
        // Verify OR's right side
        orFilter.Right.ShouldBeOfType<ComparisonFilter>();
        var orRight = (ComparisonFilter)orFilter.Right;
        orRight.AttributeName.ShouldBe("title");
        orRight.Operator.ShouldBe(FilterOperator.Equals);
        orRight.Value.ShouldBeOfType<StringValue>();
        ((StringValue)orRight.Value).Value.ShouldBe("Manager");
    }

    // ==================== VISITOR TESTS ====================

    [Fact]
    public void Visitor_PrintsSimpleComparison()
    {
        var result = _parser.Parse("active eq true");
        result.IsError.ShouldBeFalse();
        var ast = result.Value;
        var visitor = new PrintVisitor();
        var output = ast.Accept(visitor);
        output.ShouldContain("Comparison");
        output.ShouldContain("active");
        output.ShouldContain("eq");
    }

    [Fact]
    public void Visitor_PrintsPresence()
    {
        var result = _parser.Parse("phoneNumbers pr");
        result.IsError.ShouldBeFalse();
        var ast = result.Value;
        var visitor = new PrintVisitor();
        var output = ast.Accept(visitor);
        output.ShouldContain("Presence");
        output.ShouldContain("phoneNumbers");
    }

    [Fact]
    public void Visitor_PrintsAndFilter()
    {
        var result = _parser.Parse("active eq true and title eq \"Admin\"");
        result.IsError.ShouldBeFalse();
        var ast = result.Value;
        var visitor = new PrintVisitor();
        var output = ast.Accept(visitor);
        output.ShouldContain("AND");
        output.ShouldContain("Comparison");
    }

    [Fact]
    public void Visitor_PrintsComplexNested()
    {
        var result = _parser.Parse("active eq true and (title eq \"Admin\" or title eq \"Manager\")");
        result.IsError.ShouldBeFalse();
        var ast = result.Value;
        var visitor = new PrintVisitor();
        var output = ast.Accept(visitor);
        output.ShouldContain("AND");
        output.ShouldContain("OR");
    }

    // ==================== DATETIME TESTS ====================

    [Fact]
    public void Parse_DateTime_Filter()
    {
        // Arrange
        var expected = F.GreaterThan("meta.created", DateTime.Parse("2024-01-15T10:00:00Z"));
        
        // Act
        var result = _parser.Parse("meta.created gt \"2024-01-15T10:00:00Z\"");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void FilterBuilder_GreaterThan_DateTime()
    {
        // Arrange
        var dt = new DateTime(2024, 1, 15, 10, 0, 0);
        var expected = F.GreaterThan("meta.created", dt);
        
        // Act
        var result = _parser.Parse($"meta.created gt \"{dt:O}\"");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    // ==================== REAL-WORLD EXAMPLES ====================

    [Fact]
    public void RealWorld_AzureAD_UserProvisioning()
    {
        // Arrange
        var expected = F.Equals("active", true)
            .And(F.EndsWith("emails.value", "@company.com"))
            .And(F.StartsWith("userName", "admin").Negate());
        
        // Act
        var result = _parser.Parse("active eq true and emails.value ew \"@company.com\" and not (userName sw \"admin\")");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void RealWorld_GroupManagement()
    {
        // Arrange
        var expected = F.StartsWith("displayName", "Team")
            .Or(F.StartsWith("displayName", "Department"))
            .And(F.Contains("displayName", "Engineering").Or(F.Contains("displayName", "Architecture")));
        
        // Act
        var result = _parser.Parse("(displayName sw \"Team\" or displayName sw \"Department\") and (displayName co \"Engineering\" or displayName co \"Architecture\")");
        result.IsError.ShouldBeFalse();
        var actual = result.Value;
        
        // Assert
        FilterAssert.AreEqual(expected, actual);
    }

    [Fact]
    public void RealWorld_Complex_UserFilter()
    {
        // Arrange - Building the filter using the fluent API
        var expected = F.Equals("active", true)
            .And(F.EndsWith("emails.value", "@company.com"))
            .And(F.GreaterOrEqual("meta.created", DateTime.Parse("2024-01-01T00:00:00Z")))
            .And(F.Present("phoneNumbers"));
        
        // Act - No parsing needed, just verifying the structure
        var actual = expected;
        
        // Assert - Verify the structure
        actual.ShouldNotBeNull();
        actual.ShouldBeOfType<AndFilter>();
        
        // Verify nested AND structure
        var topAnd = (AndFilter)actual;
        topAnd.Left.ShouldBeOfType<AndFilter>();
        var midAnd = (AndFilter)topAnd.Left;
        midAnd.Left.ShouldBeOfType<AndFilter>();
        var bottomAnd = (AndFilter)midAnd.Left;
        
        // Verify first condition: active eq true
        bottomAnd.Left.ShouldBeOfType<ComparisonFilter>();
        var active = (ComparisonFilter)bottomAnd.Left;
        active.AttributeName.ShouldBe("active");
        active.Operator.ShouldBe(FilterOperator.Equals);
        active.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)active.Value).Value.ShouldBe(true);
        
        // Verify second condition: emails.value ew "@company.com"
        bottomAnd.Right.ShouldBeOfType<ComparisonFilter>();
        var email = (ComparisonFilter)bottomAnd.Right;
        email.AttributeName.ShouldBe("emails.value");
        email.Operator.ShouldBe(FilterOperator.EndsWith);
        email.Value.ShouldBeOfType<StringValue>();
        ((StringValue)email.Value).Value.ShouldBe("@company.com");
        
        // Verify third condition: meta.created ge "2024-01-01T00:00:00Z"
        midAnd.Right.ShouldBeOfType<ComparisonFilter>();
        var metaCreated = (ComparisonFilter)midAnd.Right;
        metaCreated.AttributeName.ShouldBe("meta.created");
        metaCreated.Operator.ShouldBe(FilterOperator.GreaterOrEqual);
        metaCreated.Value.ShouldBeOfType<DateTimeValue>();
        
        // Verify fourth condition: phoneNumbers pr
        topAnd.Right.ShouldBeOfType<PresenceFilter>();
        var presence = (PresenceFilter)topAnd.Right;
        presence.AttributeName.ShouldBe("phoneNumbers");
    }
}
