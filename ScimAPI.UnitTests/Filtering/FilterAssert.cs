using ScimAPI.Filtering.AST;
using Shouldly;

namespace ScimAPI.UnitTests.Filtering;

/// <summary>
/// Helper class for comparing filter expressions in tests
/// </summary>
internal static class FilterAssert
{
    public static void AreEqual(FilterExpression expected, FilterExpression actual)
    {
        if (expected == null && actual == null) return;
        
        expected.ShouldNotBeNull();
        actual.ShouldNotBeNull();
        expected.GetType().ShouldBe(actual.GetType());
        
        switch (expected)
        {
            case ComparisonFilter expComp:
                var actComp = (ComparisonFilter)actual;
                expComp.AttributeName.ShouldBe(actComp.AttributeName);
                expComp.Operator.ShouldBe(actComp.Operator);
                AreEqual(expComp.Value, actComp.Value);
                break;
                
            case PresenceFilter expPres:
                var actPres = (PresenceFilter)actual;
                expPres.AttributeName.ShouldBe(actPres.AttributeName);
                break;
                
            case AndFilter expAnd:
                var actAnd = (AndFilter)actual;
                AreEqual(expAnd.Left, actAnd.Left);
                AreEqual(expAnd.Right, actAnd.Right);
                break;
                
            case OrFilter expOr:
                var actOr = (OrFilter)actual;
                AreEqual(expOr.Left, actOr.Left);
                AreEqual(expOr.Right, actOr.Right);
                break;
                
            case NotFilter expNot:
                var actNot = (NotFilter)actual;
                AreEqual(expNot.Expression, actNot.Expression);
                break;
                
            default:
                throw new ArgumentException($"Unknown filter type: {expected.GetType().Name}");
        }
    }
    
    private static void AreEqual(FilterValue expected, FilterValue actual)
    {
        expected.ShouldNotBeNull();
        actual.ShouldNotBeNull();
        expected.GetType().ShouldBe(actual.GetType());
        
        switch (expected)
        {
            case StringValue expStr:
                var actStr = (StringValue)actual;
                expStr.Value.ShouldBe(actStr.Value);
                break;
                
            case BooleanValue expBool:
                var actBool = (BooleanValue)actual;
                expBool.Value.ShouldBe(actBool.Value);
                break;
                
            case NumericValue expNum:
                var actNum = (NumericValue)actual;
                expNum.Value.ShouldBe(actNum.Value);
                break;
                
            case DateTimeValue expDt:
                var actDt = (DateTimeValue)actual;
                expDt.Value.ShouldBe(actDt.Value);
                break;
                
            default:
                throw new ArgumentException($"Unknown value type: {expected.GetType().Name}");
        }
    }
}