# Action Plan to Fix SCIM Validator Errors

**Created**: February 22, 2026  
**Target**: Microsoft SCIM Validator Compliance  
**Status**: In Progress

## Overview

This document outlines the specific code changes required to fix the 3 critical failures identified by the Microsoft SCIM Validator.

---

## Issue #1: Fix excludedAttributes Support

### Error Details
- **Test ID**: 70, 72
- **Endpoint**: GET /scim/Groups with `?excludedAttributes=members`
- **Problem**: The `members` attribute is included in responses despite being in the excludedAttributes parameter

### Files to Modify

#### 1. **EzSCIM/Controllers/GroupsController.cs**
Add support for `excludedAttributes` parameter parsing and filtering

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetGroupById(
    string id,
    [FromQuery] string? attributes = null,
    [FromQuery] string? excludedAttributes = null)
{
    // ...existing code...
    
    // Parse excludedAttributes into a list
    var excludedSet = ParseAttributeList(excludedAttributes);
    
    // Filter the response
    var filteredGroup = FilterAttributes(group, excludedSet, isExclude: true);
    
    return Ok(filteredGroup);
}

[HttpGet]
public async Task<IActionResult> GetGroups(
    [FromQuery] string? filter = null,
    [FromQuery] int? startIndex = null,
    [FromQuery] int? count = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? attributes = null,
    [FromQuery] string? excludedAttributes = null)
{
    // ...existing code...
    
    var excludedSet = ParseAttributeList(excludedAttributes);
    
    // Filter each resource in the list response
    var filteredResources = resources.Select(r => FilterAttributes(r, excludedSet, isExclude: true)).ToList();
    
    return Ok(new { schemas, totalResults, startIndex, itemsPerPage, resources = filteredResources });
}
```

#### 2. **EzSCIM/Helpers/AttributeFilterHelper.cs** (NEW FILE)
Create a utility class to handle attribute filtering

```csharp
public static class AttributeFilterHelper
{
    /// <summary>
    /// Parses comma-separated attribute list from query parameter
    /// </summary>
    public static HashSet<string> ParseAttributeList(string? attributeString)
    {
        if (string.IsNullOrWhiteSpace(attributeString))
            return new HashSet<string>();
        
        return new HashSet<string>(
            attributeString.Split(',')
                .Select(a => a.Trim().ToLowerInvariant())
                .Where(a => !string.IsNullOrEmpty(a))
        );
    }
    
    /// <summary>
    /// Filters a SCIM resource to include/exclude specified attributes
    /// </summary>
    public static JObject FilterAttributes(
        object resource, 
        HashSet<string> attributeSet, 
        bool isExclude = false)
    {
        if (string.IsNullOrEmpty(attributeSet))
            return JObject.FromObject(resource);
        
        var jObject = JObject.FromObject(resource);
        
        if (isExclude)
        {
            // Remove excluded attributes
            foreach (var property in jObject.Properties().ToList())
            {
                if (attributeSet.Contains(property.Name.ToLowerInvariant()))
                {
                    property.Remove();
                }
            }
        }
        else
        {
            // Keep only included attributes (plus required attributes)
            var required = new[] { "schemas", "id", "meta" };
            var toKeep = attributeSet.Union(required.Select(r => r.ToLowerInvariant()));
            
            foreach (var property in jObject.Properties().ToList())
            {
                if (!toKeep.Contains(property.Name.ToLowerInvariant()))
                {
                    property.Remove();
                }
            }
        }
        
        return jObject;
    }
}
```

#### 3. **EzSCIM/Controllers/UsersController.cs**
Apply same changes for Users endpoint for consistency

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUserById(
    string id,
    [FromQuery] string? attributes = null,
    [FromQuery] string? excludedAttributes = null)
{
    // ...existing code...
    
    var excludedSet = AttributeFilterHelper.ParseAttributeList(excludedAttributes);
    var filteredUser = AttributeFilterHelper.FilterAttributes(user, excludedSet, isExclude: true);
    
    return Ok(filteredUser);
}

[HttpGet]
public async Task<IActionResult> GetUsers(
    [FromQuery] string? filter = null,
    [FromQuery] int? startIndex = null,
    [FromQuery] int? count = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = null,
    [FromQuery] string? attributes = null,
    [FromQuery] string? excludedAttributes = null)
{
    // ...existing code...
    
    var excludedSet = AttributeFilterHelper.ParseAttributeList(excludedAttributes);
    var filteredResources = resources.Select(r => AttributeFilterHelper.FilterAttributes(r, excludedSet, isExclude: true)).ToList();
    
    return Ok(new { schemas, totalResults, startIndex, itemsPerPage, resources = filteredResources });
}
```

---

## Issue #2: Fix PATCH Multi-Attribute Operations

### Error Details
- **Test ID**: 61, 82
- **Endpoint**: PATCH /scim/Users/{id}
- **Problem**: PATCH operations with complex filter expressions like `[primary eq true]` are not applied correctly

### Files to Modify

#### 1. **EzSCIM/Controllers/UsersController.cs**
Review and enhance the PATCH endpoint

```csharp
[HttpPatch("{id}")]
public async Task<IActionResult> PatchUser(string id, [FromBody] PatchRequest patchRequest)
{
    // Get the user
    var user = await _userRepository.GetByIdAsync(id);
    if (user == null)
        return NotFound(new ErrorResponse { status = 404, detail = "User not found" });
    
    // Apply each operation
    foreach (var operation in patchRequest.Operations ?? new List<PatchOperation>())
    {
        try
        {
            user = ApplyPatchOperation(user, operation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying PATCH operation: {Operation}", operation.op);
            return BadRequest(new ErrorResponse { status = 400, detail = $"Invalid PATCH operation: {ex.Message}" });
        }
    }
    
    // Save the user
    await _userRepository.UpdateAsync(user);
    
    // Reload to verify changes were persisted
    var updatedUser = await _userRepository.GetByIdAsync(id);
    return Ok(updatedUser);
}

private ScimUser ApplyPatchOperation(ScimUser user, PatchOperation operation)
{
    switch (operation.op?.ToLowerInvariant())
    {
        case "replace":
            return ApplyReplaceOperation(user, operation);
        case "add":
            return ApplyAddOperation(user, operation);
        case "remove":
            return ApplyRemoveOperation(user, operation);
        default:
            throw new ArgumentException($"Unsupported operation: {operation.op}");
    }
}

private ScimUser ApplyReplaceOperation(ScimUser user, PatchOperation operation)
{
    if (string.IsNullOrEmpty(operation.path))
    {
        // Replace entire resource (or specified value attributes)
        if (operation.value is JObject valueObj)
        {
            foreach (var property in valueObj.Properties())
            {
                SetPropertyValue(user, property.Name, property.Value);
            }
        }
    }
    else if (operation.path.Contains("[") && operation.path.Contains("]"))
    {
        // Handle complex filter expressions like "emails[primary eq true].value"
        ApplyFilteredReplaceOperation(user, operation.path, operation.value);
    }
    else
    {
        // Handle simple path like "displayName"
        SetPropertyValue(user, operation.path, operation.value);
    }
    
    return user;
}

private void ApplyFilteredReplaceOperation(ScimUser user, string path, object? value)
{
    // Parse path like "emails[primary eq true].value"
    var match = Regex.Match(path, @"^(\w+)\[([^\]]+)\]\.(.+)$");
    if (!match.Success)
        throw new ArgumentException($"Invalid filter path: {path}");
    
    var arrayProperty = match.Groups[1].Value;    // "emails"
    var filterExpression = match.Groups[2].Value; // "primary eq true"
    var targetProperty = match.Groups[3].Value;   // "value"
    
    // Get the array
    var arrayProp = user.GetType().GetProperty(arrayProperty, 
        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);
    
    if (arrayProp?.GetValue(user) is IList array)
    {
        // Find matching items based on filter expression
        foreach (var item in array)
        {
            if (EvaluateFilter(item, filterExpression))
            {
                // Set the target property
                var itemProp = item.GetType().GetProperty(targetProperty,
                    System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);
                if (itemProp != null)
                {
                    itemProp.SetValue(item, value);
                }
            }
        }
    }
}

private bool EvaluateFilter(object item, string filterExpression)
{
    // Parse "primary eq true" or similar simple filters
    var parts = filterExpression.Split(new[] { " eq " }, StringSplitOptions.None);
    if (parts.Length != 2)
        throw new ArgumentException($"Invalid filter expression: {filterExpression}");
    
    var propertyName = parts[0].Trim();
    var expectedValue = parts[1].Trim().ToLowerInvariant();
    
    var prop = item.GetType().GetProperty(propertyName,
        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);
    
    if (prop == null)
        return false;
    
    var actualValue = prop.GetValue(item)?.ToString()?.ToLowerInvariant() ?? "";
    return actualValue == expectedValue;
}

private void SetPropertyValue(object obj, string propertyName, object? value)
{
    var prop = obj.GetType().GetProperty(propertyName,
        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);
    
    if (prop != null && prop.CanWrite)
    {
        if (value is JObject jObj)
        {
            var jsonValue = jObj.ToObject(prop.PropertyType);
            prop.SetValue(obj, jsonValue);
        }
        else
        {
            prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
        }
    }
}
```

#### 2. **EzSCIM/Models/PatchOperation.cs** (Verify/Update)
Ensure the PatchOperation model is correctly defined

```csharp
public class PatchOperation
{
    [JsonProperty("op")]
    public string? op { get; set; }
    
    [JsonProperty("path")]
    public string? path { get; set; }
    
    [JsonProperty("value")]
    public object? value { get; set; }
}

public class PatchRequest
{
    [JsonProperty("schemas")]
    public List<string>? schemas { get; set; }
    
    [JsonProperty("Operations")]
    public List<PatchOperation>? Operations { get; set; }
}
```

---

## Issue #3: Standardize Error Messages to English

### Error Details
- **Problem**: Error messages are returned in French
- **Example**: `"detail": "Utilisateur existe déjà"` should be `"User already exists"`

### Files to Modify

#### 1. **EzSCIM/Services/ErrorMessageService.cs** (NEW FILE)
Create centralized error message management

```csharp
public static class ErrorMessages
{
    public const string UserAlreadyExists = "User already exists";
    public const string UserNotFound = "User {0} not found";
    public const string GroupAlreadyExists = "Group already exists";
    public const string GroupNotFound = "Group {0} not found";
    public const string InvalidPatchOperation = "Invalid PATCH operation";
    public const string InvalidFilter = "Invalid filter expression";
}
```

#### 2. **EzSCIM/Repositories/UserRepository.cs**
Update error messages to English

```csharp
public async Task<ScimUser> AddAsync(ScimUser user)
{
    var existing = await _context.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);
    if (existing != null)
        throw new InvalidOperationException(ErrorMessages.UserAlreadyExists);
    
    // ...rest of implementation...
}

public async Task<ScimUser?> GetByIdAsync(string id)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null)
        throw new KeyNotFoundException(string.Format(ErrorMessages.UserNotFound, id));
    
    return user;
}
```

#### 3. **EzSCIM/Controllers/ErrorHandlingMiddleware.cs** (NEW FILE)
Add centralized error handling

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/scim+json";
        var response = new { schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:Error" } };
        
        switch (exception)
        {
            case KeyNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response = new { schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:Error" }, detail = exception.Message, status = 404 };
                break;
            case InvalidOperationException:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response = new { schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:Error" }, detail = exception.Message, status = 409 };
                break;
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = new { schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:Error" }, detail = "An internal error occurred", status = 500 };
                break;
        }
        
        return context.Response.WriteAsJsonAsync(response);
    }
}
```

---

## Testing Strategy

### Unit Tests to Add

#### 1. **Test excludedAttributes Functionality**
```csharp
[Fact]
public async Task GetGroup_WithExcludedAttributes_ShouldNotIncludeMembersAttribute()
{
    // Arrange
    var groupId = "test-group-id";
    var group = new ScimGroup { Id = groupId, DisplayName = "Test", Members = new List<GroupMember>() };
    
    // Act
    var response = await _client.GetAsync($"/scim/Groups/{groupId}?excludedAttributes=members");
    var content = await response.Content.ReadAsStringAsync();
    var json = JObject.Parse(content);
    
    // Assert
    Assert.DoesNotContain("members", json.Properties().Select(p => p.Name));
}

[Fact]
public async Task GetGroups_WithExcludedAttributes_ShouldNotIncludeMembersAttribute()
{
    // Arrange & Act
    var response = await _client.GetAsync($"/scim/Groups?excludedAttributes=members");
    var content = await response.Content.ReadAsStringAsync();
    var json = JObject.Parse(content);
    
    // Assert
    var resources = (JArray)json["resources"];
    foreach (var resource in resources)
    {
        Assert.DoesNotContain("members", resource.Children().OfType<JProperty>().Select(p => p.Name));
    }
}
```

#### 2. **Test PATCH Multi-Attribute Operations**
```csharp
[Fact]
public async Task PatchUser_WithFilteredReplaceOperations_ShouldPersistAllChanges()
{
    // Arrange
    var userId = "test-user-id";
    var user = new ScimUser 
    { 
        Id = userId,
        Emails = new List<Email> 
        { 
            new Email { Value = "old@example.com", Primary = true }
        }
    };
    
    var patchRequest = new PatchRequest
    {
        Operations = new List<PatchOperation>
        {
            new PatchOperation 
            { 
                op = "replace",
                path = "emails[primary eq true].value",
                value = "new@example.com"
            }
        }
    };
    
    // Act
    var response = await _client.PatchAsync($"/scim/Users/{userId}", patchRequest);
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonConvert.DeserializeObject<ScimUser>(content);
    
    // Assert
    Assert.Equal("new@example.com", result.Emails.First(e => e.Primary).Value);
}
```

#### 3. **Test Error Messages**
```csharp
[Fact]
public async Task CreateDuplicateUser_ShouldReturnEnglishErrorMessage()
{
    // Arrange
    var user1 = new { userName = "test@example.com", name = new { familyName = "Test" } };
    var user2 = new { userName = "test@example.com", name = new { familyName = "Test" } };
    
    // Act
    await _client.PostAsync("/scim/Users", user1);
    var response = await _client.PostAsync("/scim/Users", user2);
    var content = await response.Content.ReadAsStringAsync();
    var error = JObject.Parse(content);
    
    // Assert
    Assert.Equal("User already exists", error["detail"]?.ToString());
}
```

---

## Implementation Priority

### Phase 1 (Critical - Do First)
1. Implement `excludedAttributes` support in Controllers
2. Create `AttributeFilterHelper` utility class
3. Apply to both Users and Groups endpoints
4. Run tests to validate

### Phase 2 (High Priority)
1. Fix PATCH operations for complex filter expressions
2. Enhance `ApplyPatchOperation` logic
3. Add comprehensive PATCH tests
4. Validate against SCIM validator

### Phase 3 (Important)
1. Standardize all error messages to English
2. Create `ErrorMessageService`
3. Add centralized error handling middleware
4. Update all error messages throughout codebase

### Phase 4 (Quality)
1. Add integration tests
2. Re-run Microsoft SCIM Validator
3. Document changes in CHANGELOG.md
4. Create PR for review

---

## Estimated Effort

- **Phase 1**: 2-3 hours (excludedAttributes)
- **Phase 2**: 3-4 hours (PATCH operations)
- **Phase 3**: 2 hours (Error messages)
- **Phase 4**: 2 hours (Testing & documentation)

**Total**: 9-11 hours

---

## Success Criteria

✅ All 3 failed tests pass  
✅ excludedAttributes parameter works correctly  
✅ PATCH operations with filters persist correctly  
✅ All error messages are in English  
✅ No regression in other tests  
✅ SFComplianceFailed flag is set to false  


