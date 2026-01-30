using Shouldly;
using Xunit;
using ScimAPI.Filtering;
using ScimAPI.Filtering.AST;
using ScimAPI.Filtering.Visitors;

namespace ScimAPI.Tests.Filtering;

public class FilterParserTests
{
    private readonly FilterParser _parser = new();

    // ==================== SIMPLE COMPARISON TESTS ====================

    [Fact]
    public void Parse_SimpleEquals_WithString()
    {
        var result = _parser.Parse("userName eq \"john.doe\"");
        result.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)result;
        comp.AttributeName.ShouldBe("userName");
        comp.Operator.ShouldBe(FilterOperator.Equals);
        comp.Value.ShouldBeOfType<StringValue>();
        ((StringValue)comp.Value).Value.ShouldBe("john.doe");
    }

    [Fact]
    public void Parse_SimpleEquals_WithBoolean()
    {
        var result = _parser.Parse("active eq true");
        result.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)result;
        comp.AttributeName.ShouldBe("active");
        comp.Operator.ShouldBe(FilterOperator.Equals);
        comp.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)comp.Value).Value.ShouldBe(true);
    }

    [Fact]
    public void Parse_SimpleEquals_WithNumeric()
    {
        var result = _parser.Parse("id eq 12345");
        result.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)result;
        comp.Operator.ShouldBe(FilterOperator.Equals);
        comp.Value.ShouldBeOfType<NumericValue>();
        ((NumericValue)comp.Value).Value.ShouldBe(12345);
    }

    [Fact]
    public void Parse_Contains_Filter()
    {
        var result = _parser.Parse("displayName co \"John\"");
        result.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)result;
        comp.Operator.ShouldBe(FilterOperator.Contains);
    }

    [Fact]
    public void Parse_StartsWith_Filter()
    {
        var result = _parser.Parse("userName sw \"admin\"");
        result.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)result;
        comp.Operator.ShouldBe(FilterOperator.StartsWith);
    }

    [Fact]
    public void Parse_EndsWith_Filter()
    {
        var result = _parser.Parse("emails.value ew \"@company.com\"");
        result.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)result;
        comp.Operator.ShouldBe(FilterOperator.EndsWith);
    }

    [Fact]
    public void Parse_GreaterThan_Filter()
    {
        var result = _parser.Parse("id gt 1000");
        result.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)result;
        comp.Operator.ShouldBe(FilterOperator.GreaterThan);
    }

    [Fact]
    public void Parse_LessOrEqual_Filter()
    {
        var result = _parser.Parse("salary le 50000");
        result.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)result;
        comp.Operator.ShouldBe(FilterOperator.LessOrEqual);
    }

    // ==================== PRESENCE FILTER TESTS ====================

    [Fact]
    public void Parse_Presence_Filter()
    {
        var result = _parser.Parse("phoneNumbers pr");
        result.ShouldBeOfType<PresenceFilter>();
        var pres = (PresenceFilter)result;
        pres.AttributeName.ShouldBe("phoneNumbers");
    }

    [Fact]
    public void Parse_Presence_WithDottedAttribute()
    {
        var result = _parser.Parse("emails.value pr");
        result.ShouldBeOfType<PresenceFilter>();
        var pres = (PresenceFilter)result;
        pres.AttributeName.ShouldBe("emails.value");
    }

    // ==================== LOGICAL OPERATORS TESTS ====================

    [Fact]
    public void Parse_And_TwoFilters()
    {
        var result = _parser.Parse("active eq true and userName eq \"john\"");
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
        right.AttributeName.ShouldBe("userName");
        right.Operator.ShouldBe(FilterOperator.Equals);
        right.Value.ShouldBeOfType<StringValue>();
        ((StringValue)right.Value).Value.ShouldBe("john");
    }

    [Fact]
    public void Parse_Or_TwoFilters()
    {
        var result = _parser.Parse("title eq \"Admin\" or title eq \"Manager\"");
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
    public void Parse_Not_Filter()
    {
        var result = _parser.Parse("not (active eq false)");
        result.ShouldBeOfType<NotFilter>();
        var not = (NotFilter)result;
        
        // Verify Expression field
        not.Expression.ShouldBeOfType<ComparisonFilter>();
        var expr = (ComparisonFilter)not.Expression;
        expr.AttributeName.ShouldBe("active");
        expr.Operator.ShouldBe(FilterOperator.Equals);
        expr.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)expr.Value).Value.ShouldBe(false);
    }

    // ==================== NESTED EXPRESSIONS TESTS ====================

    [Fact]
    public void Parse_And_Or_Nested()
    {
        var result = _parser.Parse("active eq true and (title eq \"Admin\" or title eq \"Manager\")");
        result.ShouldBeOfType<AndFilter>();
        var and = (AndFilter)result;
        
        // Verify Left side is a simple comparison
        and.Left.ShouldBeOfType<ComparisonFilter>();
        var left = (ComparisonFilter)and.Left;
        left.AttributeName.ShouldBe("active");
        left.Operator.ShouldBe(FilterOperator.Equals);
        left.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)left.Value).Value.ShouldBe(true);
        
        // Verify Right side is an OR filter
        and.Right.ShouldBeOfType<OrFilter>();
        var orFilter = (OrFilter)and.Right;
        
        // Verify OR left side
        orFilter.Left.ShouldBeOfType<ComparisonFilter>();
        var orLeft = (ComparisonFilter)orFilter.Left;
        orLeft.AttributeName.ShouldBe("title");
        orLeft.Operator.ShouldBe(FilterOperator.Equals);
        orLeft.Value.ShouldBeOfType<StringValue>();
        ((StringValue)orLeft.Value).Value.ShouldBe("Admin");
        
        // Verify OR right side
        orFilter.Right.ShouldBeOfType<ComparisonFilter>();
        var orRight = (ComparisonFilter)orFilter.Right;
        orRight.AttributeName.ShouldBe("title");
        orRight.Operator.ShouldBe(FilterOperator.Equals);
        orRight.Value.ShouldBeOfType<StringValue>();
        ((StringValue)orRight.Value).Value.ShouldBe("Manager");
    }

    [Fact]
    public void Parse_Complex_Nested()
    {
        var result = _parser.Parse("((active eq true and (title eq \"Admin\" or title eq \"Manager\")) or (active eq false and title eq \"Director\")) and department eq \"Engineering\"");
        result.ShouldBeOfType<AndFilter>();
        var rootAnd = (AndFilter)result;
        
        // Verify Left side is an OR filter
        rootAnd.Left.ShouldBeOfType<OrFilter>();
        var leftOr = (OrFilter)rootAnd.Left;
        
        // Verify Left OR's left side - (active eq true and (title eq "Admin" or title eq "Manager"))
        leftOr.Left.ShouldBeOfType<AndFilter>();
        var leftOrLeftAnd = (AndFilter)leftOr.Left;
        leftOrLeftAnd.Left.ShouldBeOfType<ComparisonFilter>();
        var activeTrue = (ComparisonFilter)leftOrLeftAnd.Left;
        activeTrue.AttributeName.ShouldBe("active");
        ((BooleanValue)activeTrue.Value).Value.ShouldBe(true);
        
        leftOrLeftAnd.Right.ShouldBeOfType<OrFilter>();
        
        // Verify Left OR's right side - (active eq false and title eq "Director")
        leftOr.Right.ShouldBeOfType<AndFilter>();
        var leftOrRightAnd = (AndFilter)leftOr.Right;
        leftOrRightAnd.Left.ShouldBeOfType<ComparisonFilter>();
        var activeFalse = (ComparisonFilter)leftOrRightAnd.Left;
        activeFalse.AttributeName.ShouldBe("active");
        ((BooleanValue)activeFalse.Value).Value.ShouldBe(false);
        
        leftOrRightAnd.Right.ShouldBeOfType<ComparisonFilter>();
        var director = (ComparisonFilter)leftOrRightAnd.Right;
        director.AttributeName.ShouldBe("title");
        ((StringValue)director.Value).Value.ShouldBe("Director");
        
        // Verify Right side is a simple comparison
        rootAnd.Right.ShouldBeOfType<ComparisonFilter>();
        var department = (ComparisonFilter)rootAnd.Right;
        department.AttributeName.ShouldBe("department");
        department.Operator.ShouldBe(FilterOperator.Equals);
        department.Value.ShouldBeOfType<StringValue>();
        ((StringValue)department.Value).Value.ShouldBe("Engineering");
    }

    [Fact]
    public void Parse_Not_With_And()
    {
        var result = _parser.Parse("active eq true and not (userName sw \"admin\")");
        result.ShouldBeOfType<AndFilter>();
        var and = (AndFilter)result;
        
        // Verify Left side
        and.Left.ShouldBeOfType<ComparisonFilter>();
        var left = (ComparisonFilter)and.Left;
        left.AttributeName.ShouldBe("active");
        left.Operator.ShouldBe(FilterOperator.Equals);
        left.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)left.Value).Value.ShouldBe(true);
        
        // Verify Right side is NOT
        and.Right.ShouldBeOfType<NotFilter>();
        var notFilter = (NotFilter)and.Right;
        
        // Verify NOT's expression
        notFilter.Expression.ShouldBeOfType<ComparisonFilter>();
        var notExpr = (ComparisonFilter)notFilter.Expression;
        notExpr.AttributeName.ShouldBe("userName");
        notExpr.Operator.ShouldBe(FilterOperator.StartsWith);
        notExpr.Value.ShouldBeOfType<StringValue>();
        ((StringValue)notExpr.Value).Value.ShouldBe("admin");
    }

    // ==================== OPERATOR PRECEDENCE TESTS ====================

    [Fact]
    public void Parse_And_HigherPrecedenceThan_Or()
    {
        // active eq true and title eq "Admin" or title eq "Manager"
        // Should parse as: (active eq true and title eq "Admin") or (title eq "Manager")
        var result = _parser.Parse("active eq true and title eq \"Admin\" or title eq \"Manager\"");
        result.ShouldBeOfType<OrFilter>();
        var or = (OrFilter)result;
        
        // Verify Left side is an AND
        or.Left.ShouldBeOfType<AndFilter>();
        var leftAnd = (AndFilter)or.Left;
        
        // Verify AND's left side
        leftAnd.Left.ShouldBeOfType<ComparisonFilter>();
        var andLeft = (ComparisonFilter)leftAnd.Left;
        andLeft.AttributeName.ShouldBe("active");
        andLeft.Operator.ShouldBe(FilterOperator.Equals);
        andLeft.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)andLeft.Value).Value.ShouldBe(true);
        
        // Verify AND's right side
        leftAnd.Right.ShouldBeOfType<ComparisonFilter>();
        var andRight = (ComparisonFilter)leftAnd.Right;
        andRight.AttributeName.ShouldBe("title");
        andRight.Operator.ShouldBe(FilterOperator.Equals);
        andRight.Value.ShouldBeOfType<StringValue>();
        ((StringValue)andRight.Value).Value.ShouldBe("Admin");
        
        // Verify Right side is a simple comparison
        or.Right.ShouldBeOfType<ComparisonFilter>();
        var orRight = (ComparisonFilter)or.Right;
        orRight.AttributeName.ShouldBe("title");
        orRight.Operator.ShouldBe(FilterOperator.Equals);
        orRight.Value.ShouldBeOfType<StringValue>();
        ((StringValue)orRight.Value).Value.ShouldBe("Manager");
    }

    [Fact]
    public void Parse_Not_HigherPrecedenceThan_And()
    {
        // not active eq false and title eq "Admin"
        // Should parse as: (not (active eq false)) and (title eq "Admin")
        var result = _parser.Parse("not active eq false and title eq \"Admin\"");
        result.ShouldBeOfType<AndFilter>();
        var and = (AndFilter)result;
        
        // Verify Left side is NOT
        and.Left.ShouldBeOfType<NotFilter>();
        var notFilter = (NotFilter)and.Left;
        
        // Verify NOT's expression
        notFilter.Expression.ShouldBeOfType<ComparisonFilter>();
        var notExpr = (ComparisonFilter)notFilter.Expression;
        notExpr.AttributeName.ShouldBe("active");
        notExpr.Operator.ShouldBe(FilterOperator.Equals);
        notExpr.Value.ShouldBeOfType<BooleanValue>();
        ((BooleanValue)notExpr.Value).Value.ShouldBe(false);
        
        // Verify Right side is simple comparison
        and.Right.ShouldBeOfType<ComparisonFilter>();
        var right = (ComparisonFilter)and.Right;
        right.AttributeName.ShouldBe("title");
        right.Operator.ShouldBe(FilterOperator.Equals);
        right.Value.ShouldBeOfType<StringValue>();
        ((StringValue)right.Value).Value.ShouldBe("Admin");
    }

    // ==================== ERROR HANDLING TESTS ====================

    [Fact]
    public void Parse_EmptyString_Throws()
    {
        Should.Throw<FilterParseException>(() => _parser.Parse(""));
    }

    [Fact]
    public void Parse_MissingClosingParen_Throws()
    {
        Should.Throw<FilterParseException>(() => _parser.Parse("(active eq true"));
    }

    [Fact]
    public void Parse_InvalidOperator_Throws()
    {
        Should.Throw<FilterParseException>(() => _parser.Parse("userName xx \"john\""));
    }

    [Fact]
    public void Parse_MissingValue_Throws()
    {
        Should.Throw<FilterParseException>(() => _parser.Parse("userName eq"));
    }

    // ==================== FILTER BUILDER (F class) TESTS ====================

    [Fact]
    public void FilterBuilder_Equals_String()
    {
        var filter = F.Equals("userName", "john");
        filter.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)filter;
        comp.Operator.ShouldBe(FilterOperator.Equals);
    }

    [Fact]
    public void FilterBuilder_Equals_Boolean()
    {
        var filter = F.Equals("active", true);
        filter.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)filter;
        comp.Value.ShouldBeOfType<BooleanValue>();
    }

    [Fact]
    public void FilterBuilder_Contains()
    {
        var filter = F.Contains("displayName", "John");
        filter.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)filter;
        comp.Operator.ShouldBe(FilterOperator.Contains);
    }

    [Fact]
    public void FilterBuilder_Present()
    {
        var filter = F.Present("phoneNumbers");
        filter.ShouldBeOfType<PresenceFilter>();
    }

    [Fact]
    public void FilterBuilder_And()
    {
        var f1 = F.Equals("active", true);
        var f2 = F.Equals("title", "Admin");
        var result = f1.And(f2); // Use fluent syntax
        result.ShouldBeOfType<AndFilter>();
    }

    [Fact]
    public void FilterBuilder_Fluent_And()
    {
        var result = F.Equals("active", true)
            .And(F.Equals("title", "Admin"));
        result.ShouldBeOfType<AndFilter>();
    }

    [Fact]
    public void FilterBuilder_Fluent_Or()
    {
        var result = F.Equals("title", "Admin")
            .Or(F.Equals("title", "Manager"));
        result.ShouldBeOfType<OrFilter>();
    }

    [Fact]
    public void FilterBuilder_Fluent_Negate()
    {
        var result = F.Equals("active", false).Negate();
        result.ShouldBeOfType<NotFilter>();
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
        var ast = _parser.Parse("active eq true");
        var visitor = new PrintVisitor();
        var output = ast.Accept(visitor);
        output.ShouldContain("Comparison");
        output.ShouldContain("active");
        output.ShouldContain("eq");
    }

    [Fact]
    public void Visitor_PrintsPresence()
    {
        var ast = _parser.Parse("phoneNumbers pr");
        var visitor = new PrintVisitor();
        var output = ast.Accept(visitor);
        output.ShouldContain("Presence");
        output.ShouldContain("phoneNumbers");
    }

    [Fact]
    public void Visitor_PrintsAndFilter()
    {
        var ast = _parser.Parse("active eq true and title eq \"Admin\"");
        var visitor = new PrintVisitor();
        var output = ast.Accept(visitor);
        output.ShouldContain("AND");
        output.ShouldContain("Comparison");
    }

    [Fact]
    public void Visitor_PrintsComplexNested()
    {
        var ast = _parser.Parse("active eq true and (title eq \"Admin\" or title eq \"Manager\")");
        var visitor = new PrintVisitor();
        var output = ast.Accept(visitor);
        output.ShouldContain("AND");
        output.ShouldContain("OR");
    }

    // ==================== DATETIME TESTS ====================

    [Fact]
    public void Parse_DateTime_Filter()
    {
        var result = _parser.Parse("meta.created gt \"2024-01-15T10:00:00Z\"");
        result.ShouldBeOfType<ComparisonFilter>();
        var comp = (ComparisonFilter)result;
        comp.Value.ShouldBeOfType<DateTimeValue>();
    }

    [Fact]
    public void FilterBuilder_GreaterThan_DateTime()
    {
        var dt = new DateTime(2024, 1, 15, 10, 0, 0);
        var filter = F.GreaterThan("meta.created", dt);
        filter.ShouldBeOfType<ComparisonFilter>();
    }

    // ==================== REAL-WORLD EXAMPLES ====================

    [Fact]
    public void RealWorld_AzureAD_UserProvisioning()
    {
        var filter = "active eq true and emails.value ew \"@company.com\" and not (userName sw \"admin\")";
        var ast = _parser.Parse(filter);
        ast.ShouldNotBeNull();
        ast.ShouldBeOfType<AndFilter>();
    }

    [Fact]
    public void RealWorld_GroupManagement()
    {
        var filter = "(displayName sw \"Team\" or displayName sw \"Department\") and (displayName co \"Engineering\" or displayName co \"Architecture\")";
        var ast = _parser.Parse(filter);
        ast.ShouldNotBeNull();
        ast.ShouldBeOfType<AndFilter>();
    }

    [Fact]
    public void RealWorld_Complex_UserFilter()
    {
        var ast = F.Equals("active", true)
            .And(F.EndsWith("emails.value", "@company.com"))
            .And(F.GreaterOrEqual("meta.created", DateTime.Parse("2024-01-01T00:00:00Z")))
            .And(F.Present("phoneNumbers"));
        
        ast.ShouldNotBeNull();
    }
}
