# FilterErrors Documentation

## Overview

`FilterErrors` is a static class that defines all possible parsing errors that can occur when parsing SCIM filter strings. It uses the **ErrorOr** library to provide type-safe, functional error handling without throwing exceptions.

## Why ErrorOr?

### Traditional Approach (Exceptions)
```csharp
try
{
    var filter = parser.Parse("invalid filter");
    // Use filter...
}
catch (FilterParseException ex)
{
    // Handle error...
}
```

**Problems:**
- ❌ Forces try-catch boilerplate everywhere
- ❌ Exceptions are expensive (stack unwinding)
- ❌ Easy to forget error handling
- ❌ No compile-time guarantee of error handling

### ErrorOr Approach (Functional)
```csharp
var result = parser.Parse("invalid filter");

if (result.IsError)
{
    // Handle errors: result.Errors
    return BadRequest(result.Errors);
}

var filter = result.Value;
// Use filter...
```

**Benefits:**
- ✅ Explicit error handling (can't forget)
- ✅ No expensive exceptions
- ✅ Compile-time safety (must check .IsError)
- ✅ Clean, functional style
- ✅ Multiple errors can be returned

---

## FilterErrors Class Structure

### Location
`ScimAPI/Filtering/FilterErrors.cs`

### Error Types

All errors are **Validation** errors (400 Bad Request level) with unique codes and descriptions that include position information.

#### 1. EmptyFilter
**Code:** `Filter.Empty`  
**Description:** "Filter string cannot be empty"  
**Triggered when:** Input is null, empty, or whitespace

```csharp
var result = parser.Parse("");
// result.FirstError.Code == "Filter.Empty"
```

---

#### 2. TokenizationFailed
**Code:** `Filter.TokenizationFailed`  
**Description:** "Tokenization failed: {message}"  
**Triggered when:** The tokenizer throws an exception (rare)

```csharp
// If tokenizer encounters an unexpected error
// result.FirstError.Code == "Filter.TokenizationFailed"
```

---

#### 3. UnexpectedTokensAfterExpression
**Code:** `Filter.UnexpectedTokensAfterExpression`  
**Description:** "Unexpected tokens after filter expression at position {position}"  
**Triggered when:** Extra tokens found after complete expression

```csharp
var result = parser.Parse("userName eq \"john\" extraStuff");
// result.FirstError.Code == "Filter.UnexpectedTokensAfterExpression"
// result.FirstError.Description contains position
```

---

#### 4. MissingClosingParenthesis
**Code:** `Filter.MissingClosingParenthesis`  
**Description:** "Expected closing parenthesis at position {position}"  
**Triggered when:** Opening `(` without matching `)`

```csharp
var result = parser.Parse("(userName eq \"john\"");
// result.FirstError.Code == "Filter.MissingClosingParenthesis"
```

---

#### 5. ExpectedAttributeName
**Code:** `Filter.ExpectedAttributeName`  
**Description:** "Expected attribute name, but got '{actual}' at position {position}"  
**Triggered when:** Parser expects attribute but finds something else

```csharp
var result = parser.Parse("eq \"value\"");
// result.FirstError.Code == "Filter.ExpectedAttributeName"
```

---

#### 6. ExpectedOperator
**Code:** `Filter.ExpectedOperator`  
**Description:** "Expected operator, but got '{actual}' at position {position}"  
**Triggered when:** Attribute name is followed by non-operator token

```csharp
var result = parser.Parse("userName \"value\"");
// result.FirstError.Code == "Filter.ExpectedOperator"
```

---

#### 7. ExpectedValue
**Code:** `Filter.ExpectedValue`  
**Description:** "Expected value, but got '{actual}' at position {position}"  
**Triggered when:** Operator is not followed by a value

```csharp
var result = parser.Parse("userName eq");
// result.FirstError.Code == "Filter.ExpectedValue"
```

---

#### 8. UnknownOperator
**Code:** `Filter.UnknownOperator`  
**Description:** "Unknown operator '{op}' at position {position}"  
**Triggered when:** Operator token has invalid value (this is rare due to tokenizer classification)

```csharp
// This would need the tokenizer to classify something as Operator token
// that isn't a valid SCIM operator
```

---

#### 9. InvalidSyntax (Future)
**Code:** `Filter.InvalidSyntax`  
**Description:** "Invalid syntax: {message} at position {position}"  
**Reserved for future syntax validation errors**

---

## Position Information

All errors include **position** information (character index in the filter string) to help users identify where the error occurred.

### Example Error with Position

```csharp
var filter = "userName eq \"value\" and title";
var result = parser.Parse(filter);

if (result.IsError)
{
    var error = result.FirstError;
    Console.WriteLine($"Error: {error.Code}");
    Console.WriteLine($"Description: {error.Description}");
    // Output:
    // Error: Filter.ExpectedOperator
    // Description: Expected operator, but got 'Eof' at position 31
}
```

---

## Using ErrorOr in Controllers

### ASP.NET Core Integration

```csharp
[HttpGet]
public IActionResult GetUsers([FromQuery] string? filter)
{
    if (!string.IsNullOrWhiteSpace(filter))
    {
        var parser = new FilterParser();
        var parseResult = parser.Parse(filter);
        
        if (parseResult.IsError)
        {
            // Return 400 Bad Request with error details
            return BadRequest(new 
            { 
                errors = parseResult.Errors.Select(e => new 
                { 
                    code = e.Code, 
                    description = e.Description,
                    type = e.Type.ToString()
                }) 
            });
        }

        var filterExpression = parseResult.Value;
        // Use the filter...
    }
    
    // Continue processing...
}
```

### Response Example (Error)

```json
{
  "errors": [
    {
      "code": "Filter.ExpectedValue",
      "description": "Expected value, but got 'Eof' at position 12",
      "type": "Validation"
    }
  ]
}
```

---

## Pattern Matching

ErrorOr supports elegant pattern matching:

```csharp
var result = parser.Parse(filter);

return result.Match(
    value => ProcessFilter(value),
    errors => BadRequest(new { errors })
);
```

Or with switch expression:

```csharp
var result = parser.Parse(filter);

return result switch
{
    { IsError: false } => ProcessFilter(result.Value),
    { IsError: true } => BadRequest(result.Errors)
};
```

---

## Testing ErrorOr

### Successful Parse Test
```csharp
[Fact]
public void Parse_ValidFilter_ReturnsSuccess()
{
    // Act
    var result = parser.Parse("userName eq \"john\"");
    
    // Assert
    result.IsError.ShouldBeFalse();
    var filter = result.Value;
    // Assert on filter...
}
```

### Error Test
```csharp
[Fact]
public void Parse_InvalidFilter_ReturnsError()
{
    // Act
    var result = parser.Parse("userName eq");
    
    // Assert
    result.IsError.ShouldBeTrue();
    result.FirstError.Code.ShouldBe("Filter.ExpectedValue");
    result.FirstError.Description.ShouldContain("Expected value");
}
```

### Theory Test (Multiple Scenarios)
```csharp
[Theory]
[InlineData("", "Filter.Empty")]
[InlineData("(userName eq \"john\"", "Filter.MissingClosingParenthesis")]
[InlineData("userName eq", "Filter.ExpectedValue")]
public void Parse_InvalidFilters_ReturnCorrectErrorCodes(
    string filter, 
    string expectedCode)
{
    // Act
    var result = parser.Parse(filter);
    
    // Assert
    result.IsError.ShouldBeTrue();
    result.FirstError.Code.ShouldBe(expectedCode);
}
```

---

## Best Practices

### ✅ DO

1. **Always check IsError** before accessing Value
   ```csharp
   if (result.IsError) { /* handle errors */ }
   var value = result.Value;
   ```

2. **Return detailed errors** to API clients
   ```csharp
   if (result.IsError)
       return BadRequest(result.Errors);
   ```

3. **Use FirstError** for single error scenarios
   ```csharp
   var errorCode = result.FirstError.Code;
   ```

4. **Include position** in error messages for better UX
   ```csharp
   description: $"Error at position {position}"
   ```

### ❌ DON'T

1. **Don't access Value without checking IsError**
   ```csharp
   // BAD - may throw if IsError is true
   var filter = result.Value;
   ```

2. **Don't throw exceptions for validation errors**
   ```csharp
   // BAD - use ErrorOr instead
   if (invalid) throw new Exception();
   ```

3. **Don't ignore errors**
   ```csharp
   // BAD - always handle errors
   var result = parser.Parse(filter);
   DoSomething(result.Value); // May throw!
   ```

---

## Error Code Reference Table

| Code | Description | HTTP Status | Common Scenario |
|------|-------------|-------------|-----------------|
| `Filter.Empty` | Filter string is empty | 400 | User didn't provide filter |
| `Filter.TokenizationFailed` | Tokenizer error | 400 | Malformed string |
| `Filter.UnexpectedTokensAfterExpression` | Extra tokens after complete filter | 400 | `"name eq \"x\" garbage"` |
| `Filter.MissingClosingParenthesis` | Unclosed parenthesis | 400 | `"(name eq \"x\""` |
| `Filter.ExpectedAttributeName` | Missing attribute name | 400 | `"eq \"value\""` |
| `Filter.ExpectedOperator` | Missing operator | 400 | `"name \"value\""` |
| `Filter.ExpectedValue` | Missing value | 400 | `"name eq"` |
| `Filter.UnknownOperator` | Invalid operator | 400 | Rare - tokenizer catches most |

---

## Summary

**FilterErrors** provides:
- ✅ Type-safe error codes
- ✅ Descriptive messages with positions
- ✅ Functional error handling (no exceptions)
- ✅ Easy integration with ASP.NET Core
- ✅ Compile-time safety
- ✅ Better testability

**Use ErrorOr when:**
- You want explicit error handling
- Performance matters (no exceptions)
- You need multiple error types
- You want functional programming style
- You want compile-time guarantees

All code, documentation, and comments are in English! 🇬🇧
