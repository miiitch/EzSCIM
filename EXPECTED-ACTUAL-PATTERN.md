# Expected/Actual Test Pattern - Best Practices

## Pattern Overview

The **expected/actual** test pattern is a clean and maintainable approach to writing unit tests where:

1. **Arrange**: Define what you expect the result to be
2. **Act**: Execute the operation to get the actual result
3. **Assert**: Compare expected and actual using a helper

## Implementation

### Step 1: Create a Helper Class

Create a static helper class that knows how to compare your domain objects:

```csharp
internal static class YourAssert
{
    public static void AreEqual(YourType expected, YourType actual)
    {
        // Validate nulls
        if (expected == null && actual == null) return;
        expected.ShouldNotBeNull();
        actual.ShouldNotBeNull();
        
        // Compare type
        expected.GetType().ShouldBe(actual.GetType());
        
        // Compare properties
        expected.Property1.ShouldBe(actual.Property1);
        expected.Property2.ShouldBe(actual.Property2);
        
        // Recursively compare nested objects
        AreEqual(expected.NestedObject, actual.NestedObject);
    }
}
```

### Step 2: Write Tests Using the Pattern

```csharp
[Fact]
public void YourTest()
{
    // Arrange - Define what you expect
    var expected = new YourType 
    { 
        Property1 = "value1",
        Property2 = 42 
    };
    
    // Act - Execute the operation
    var actual = YourService.DoSomething();
    
    // Assert - Compare
    YourAssert.AreEqual(expected, actual);
}
```

## Example: FilterAssert Implementation

The `FilterAssert` class in `FilterParserTests.cs` demonstrates this pattern:

```csharp
internal static class FilterAssert
{
    public static void AreEqual(FilterExpression expected, FilterExpression actual)
    {
        // Handle nulls
        if (expected == null && actual == null) return;
        expected.ShouldNotBeNull();
        actual.ShouldNotBeNull();
        
        // Ensure same type
        expected.GetType().ShouldBe(actual.GetType());
        
        // Type-specific comparison using pattern matching
        switch (expected)
        {
            case ComparisonFilter expComp:
                var actComp = (ComparisonFilter)actual;
                expComp.AttributeName.ShouldBe(actComp.AttributeName);
                expComp.Operator.ShouldBe(actComp.Operator);
                AreEqual(expComp.Value, actComp.Value);  // Recursive
                break;
                
            case AndFilter expAnd:
                var actAnd = (AndFilter)actual;
                AreEqual(expAnd.Left, actAnd.Left);      // Recursive
                AreEqual(expAnd.Right, actAnd.Right);    // Recursive
                break;
                
            // ... other cases
        }
    }
    
    // Helper for comparing values
    private static void AreEqual(FilterValue expected, FilterValue actual)
    {
        // Similar implementation for value types
    }
}
```

## Benefits

### 1. Concise Tests
```csharp
// Before: 15 lines
var result = Parse("complex expression");
result.ShouldBeOfType<AndFilter>();
var and = (AndFilter)result;
and.Left.ShouldBeOfType<ComparisonFilter>();
var left = (ComparisonFilter)and.Left;
left.AttributeName.ShouldBe("name");
// ... 10 more lines

// After: 3 lines
var expected = F.Equals("name", "value").And(F.Present("field"));
var actual = Parse("complex expression");
YourAssert.AreEqual(expected, actual);
```

### 2. Better Error Messages

When using a custom assert helper with Shouldly:
- You get clear error messages showing what doesn't match
- The helper can provide context about where in the tree the mismatch occurs

### 3. Easier Maintenance

When your domain objects change:
- Update only the helper class
- All tests automatically work with the new structure
- No need to touch individual test assertions

### 4. Reusability

The helper can be used across multiple test files:
```csharp
public class ParserTests
{
    [Fact] 
    public void Test1() => YourAssert.AreEqual(expected, actual);
}

public class BuilderTests  
{
    [Fact]
    public void Test2() => YourAssert.AreEqual(expected, actual);
}
```

## When to Use This Pattern

✅ **Good Use Cases:**
- Testing parsers (string → object tree)
- Testing builders (fluent API → object)
- Testing serialization/deserialization
- Testing complex object transformations
- Comparing hierarchical structures

❌ **Not Ideal For:**
- Simple value comparisons (just use `.ShouldBe()`)
- Testing exceptions (use `Should.Throw<>()`)
- Testing side effects (use specific assertions)

## Anti-Patterns to Avoid

### ❌ Don't Make Expected Equal Actual
```csharp
// BAD: This tests nothing!
var expected = _service.DoWork();
var actual = _service.DoWork();
Assert.AreEqual(expected, actual);
```

### ❌ Don't Use for Simple Assertions
```csharp
// BAD: Overkill for simple value
var expected = 42;
var actual = Calculate();
YourAssert.AreEqual(expected, actual);  // Just use: actual.ShouldBe(42)
```

### ✅ Do Build Expected Independently
```csharp
// GOOD: Expected is built differently than actual
var expected = new FilterBuilder()
    .WithName("test")
    .WithValue(42)
    .Build();

var actual = _parser.Parse("name eq 'test' and value eq 42");

FilterAssert.AreEqual(expected, actual);
```

## Advanced Techniques

### Handling Collections

```csharp
public static void AreEqual(List<YourType> expected, List<YourType> actual)
{
    expected.Count.ShouldBe(actual.Count);
    
    for (int i = 0; i < expected.Count; i++)
    {
        AreEqual(expected[i], actual[i]);
    }
}
```

### Custom Error Messages

```csharp
public static void AreEqual(YourType expected, YourType actual, string context = "")
{
    expected.Property1.ShouldBe(actual.Property1, 
        $"Property1 mismatch in {context}");
    
    AreEqual(expected.Nested, actual.Nested, 
        $"{context}.Nested");
}
```

### Optional Deep Comparison

```csharp
public static void AreEqual(YourType expected, YourType actual, 
    bool deepCompare = true)
{
    expected.Id.ShouldBe(actual.Id);
    
    if (deepCompare)
    {
        AreEqual(expected.Details, actual.Details);
    }
}
```

## Summary

The expected/actual pattern with a custom assert helper:
- Makes tests **shorter and clearer**
- **Centralizes comparison logic**
- Makes tests **easier to maintain**
- Provides **better error messages**

Use it when comparing complex objects or hierarchical structures!
