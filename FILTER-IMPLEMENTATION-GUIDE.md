## 🔧 Filter Implementation Guide

This guide shows how to implement SCIM filter parsing and evaluation in your repository.

---

## Current Implementation Status

### GetUsersAsync Method Signature
```csharp
Task<ScimListResponse<ScimUser>> GetUsersAsync(
    string? filter = null, 
    int startIndex = 1, 
    int count = 100
);
```

### Current Implementation (InMemoryScimRepository)

The current implementation uses a **simple string-based filter approach**:

```csharp
public async Task<ScimListResponse<ScimUser>> GetUsersAsync(string? filter = null, int startIndex = 1, int count = 100)
{
    var allUsers = _users.Values.ToList();

    // Simple filtering example
    if (!string.IsNullOrEmpty(filter))
    {
        // Extract value from filter like: userName eq "value"
        if (filter.Contains("userName") && filter.Contains("eq"))
        {
            var value = filter.Split("\"")[1];
            allUsers = allUsers.Where(u => u.UserName == value).ToList();
        }
    }

    var users = allUsers
        .Skip(startIndex - 1)
        .Take(count)
        .ToList();

    return new ScimListResponse<ScimUser>
    {
        TotalResults = allUsers.Count,
        ItemsPerPage = count,
        StartIndex = startIndex,
        Resources = users
    };
}
```

---

## Implementation Options

### Option 1: Simple String Matching (Current)

**Pros:**
- Quick to implement
- Works for basic filters
- No external dependencies

**Cons:**
- Limited to simple filters
- Hard to maintain
- Doesn't support logical operators

**Example:**
```csharp
private List<ScimUser> ApplySimpleFilter(List<ScimUser> users, string filter)
{
    if (filter.Contains("active eq true"))
        return users.Where(u => u.Active).ToList();
    
    if (filter.Contains("active eq false"))
        return users.Where(u => !u.Active).ToList();
    
    if (filter.Contains("userName eq"))
    {
        var value = ExtractQuotedValue(filter);
        return users.Where(u => u.UserName == value).ToList();
    }
    
    return users;
}
```

---

### Option 2: LINQ Dynamic Filtering

**Pros:**
- Supports complex filters
- Type-safe at runtime
- Good performance

**Cons:**
- More complex to implement
- Requires building LINQ expressions

**Example:**
```csharp
public List<ScimUser> ApplyLinqFilter(List<ScimUser> users, string filter)
{
    if (string.IsNullOrEmpty(filter))
        return users;

    // Build LINQ expression from filter
    var expression = BuildExpression(filter);
    return users.AsQueryable().Where(expression).ToList();
}

private Expression<Func<ScimUser, bool>> BuildExpression(string filter)
{
    // Parse filter and build expression tree
    // Example: "active eq true" -> user => user.Active == true
    var parameter = Expression.Parameter(typeof(ScimUser), "user");
    
    // Extract attribute name, operator, value
    var parts = ParseFilter(filter);
    
    // Build comparison expression
    var property = Expression.Property(parameter, parts.Attribute);
    var constant = Expression.Constant(parts.Value);
    var comparison = Expression.Equal(property, constant);
    
    return Expression.Lambda<Func<ScimUser, bool>>(comparison, parameter);
}
```

---

### Option 3: Open Source Library (ScimCore or Similar)

**Pros:**
- Full SCIM spec compliance
- Maintained and tested
- Handles all edge cases

**Cons:**
- External dependency
- More overhead

**Example:**
```csharp
// Using a hypothetical ScimFilter library
public List<ScimUser> ApplyFilterWithLibrary(List<ScimUser> users, string filter)
{
    var filterEvaluator = new ScimFilterEvaluator();
    return users.Where(u => filterEvaluator.Evaluate(filter, u)).ToList();
}
```

---

## Recommended: Enhanced Simple Filtering

Below is a recommended **middle-ground approach** that handles common filters without external dependencies:

```csharp
/// <summary>
/// Applies SCIM filter to users collection
/// Supports: eq, ne, co, sw, ew, pr, gt, lt, and logical operators (and, or, not)
/// </summary>
public List<ScimUser> ApplyUserFilter(List<ScimUser> users, string filter)
{
    if (string.IsNullOrEmpty(filter))
        return users;

    // Simple filter parsing and application
    var filtered = users.AsEnumerable();

    // Handle "active eq true/false"
    if (filter.Contains("active eq true"))
        filtered = filtered.Where(u => u.Active == true);
    else if (filter.Contains("active eq false"))
        filtered = filtered.Where(u => u.Active == false);

    // Handle "userName eq"
    if (filter.Contains("userName eq"))
    {
        var value = ExtractQuotedValue(filter, "userName");
        filtered = filtered.Where(u => u.UserName == value);
    }

    // Handle "userName sw" (starts with)
    if (filter.Contains("userName sw"))
    {
        var value = ExtractQuotedValue(filter, "userName");
        filtered = filtered.Where(u => u.UserName.StartsWith(value));
    }

    // Handle "displayName co" (contains)
    if (filter.Contains("displayName co"))
    {
        var value = ExtractQuotedValue(filter, "displayName");
        filtered = filtered.Where(u => u.DisplayName.Contains(value));
    }

    // Handle "emails.value ew" (ends with)
    if (filter.Contains("emails.value ew"))
    {
        var value = ExtractQuotedValue(filter, "emails.value");
        filtered = filtered.Where(u => 
            u.Emails?.Any(e => e.Value?.EndsWith(value) ?? false) ?? false
        );
    }

    // Handle "email pr" (present)
    if (filter.Contains("email pr"))
        filtered = filtered.Where(u => !string.IsNullOrEmpty(u.Primary?.Value));

    // Handle compound filters with "and"
    if (filter.Contains(" and "))
        filtered = ApplyAndFilter(filtered, filter);

    return filtered.ToList();
}

private List<ScimUser> ApplyAndFilter(IEnumerable<ScimUser> users, string filter)
{
    var conditions = filter.Split(" and ");
    var result = users;

    foreach (var condition in conditions)
    {
        // Recursively apply each condition
        result = ApplyUserFilter(result.ToList(), condition.Trim());
    }

    return result.ToList();
}

private string ExtractQuotedValue(string filter, string attribute)
{
    // Extract value from pattern: attribute op "value"
    var start = filter.IndexOf("\"") + 1;
    var end = filter.LastIndexOf("\"");
    return end > start ? filter.Substring(start, end - start) : string.Empty;
}
```

---

## For Groups - Similar Implementation

```csharp
public List<ScimGroup> ApplyGroupFilter(List<ScimGroup> groups, string filter)
{
    if (string.IsNullOrEmpty(filter))
        return groups;

    var filtered = groups.AsEnumerable();

    // Handle "displayName eq"
    if (filter.Contains("displayName eq"))
    {
        var value = ExtractQuotedValue(filter, "displayName");
        filtered = filtered.Where(g => g.DisplayName == value);
    }

    // Handle "displayName co" (contains)
    if (filter.Contains("displayName co"))
    {
        var value = ExtractQuotedValue(filter, "displayName");
        filtered = filtered.Where(g => g.DisplayName.Contains(value));
    }

    // Handle "displayName sw" (starts with)
    if (filter.Contains("displayName sw"))
    {
        var value = ExtractQuotedValue(filter, "displayName");
        filtered = filtered.Where(g => g.DisplayName.StartsWith(value));
    }

    // Handle "id pr" (present)
    if (filter.Contains("id pr"))
        filtered = filtered.Where(g => !string.IsNullOrEmpty(g.Id));

    return filtered.ToList();
}
```

---

## Testing Filter Implementation

```csharp
[TestClass]
public class FilterTests
{
    private UsersOnlyRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        _repository = new UsersOnlyRepository(new MockLogger());
    }

    [TestMethod]
    public async Task GetUsersAsync_WithActiveFilter_ReturnsActiveUsers()
    {
        // Arrange
        var user1 = new ScimUser { Id = "1", UserName = "john", Active = true };
        var user2 = new ScimUser { Id = "2", UserName = "jane", Active = false };
        await _repository.CreateUserAsync(user1);
        await _repository.CreateUserAsync(user2);

        // Act
        var result = await _repository.GetUsersAsync("active eq true");

        // Assert
        Assert.AreEqual(1, result.TotalResults);
        Assert.AreEqual("john", result.Resources[0].UserName);
    }

    [TestMethod]
    public async Task GetUsersAsync_WithUserNameFilter_ReturnsMatchingUser()
    {
        // Arrange
        var user = new ScimUser { Id = "1", UserName = "john.doe" };
        await _repository.CreateUserAsync(user);

        // Act
        var result = await _repository.GetUsersAsync("userName eq \"john.doe\"");

        // Assert
        Assert.AreEqual(1, result.TotalResults);
        Assert.AreEqual("john.doe", result.Resources[0].UserName);
    }

    [TestMethod]
    public async Task GetUsersAsync_WithContainsFilter_ReturnsMatchingUsers()
    {
        // Arrange
        var user1 = new ScimUser { Id = "1", DisplayName = "John Smith" };
        var user2 = new ScimUser { Id = "2", DisplayName = "Jane Doe" };
        await _repository.CreateUserAsync(user1);
        await _repository.CreateUserAsync(user2);

        // Act
        var result = await _repository.GetUsersAsync("displayName co \"John\"");

        // Assert
        Assert.AreEqual(1, result.TotalResults);
        Assert.AreEqual("John Smith", result.Resources[0].DisplayName);
    }
}
```

---

## Performance Considerations

### For Large Datasets

```csharp
// ✅ GOOD - Filters in memory, handles pagination
public async Task<ScimListResponse<ScimUser>> GetUsersAsync(string filter, int startIndex, int count)
{
    var allUsers = await GetAllUsersFromDatabaseAsync(); // Database query
    var filtered = ApplyFilter(allUsers, filter);         // In-memory filter
    var paginated = filtered
        .Skip(startIndex - 1)
        .Take(count)
        .ToList();

    return new ScimListResponse<ScimUser>
    {
        TotalResults = filtered.Count,      // Total after filter
        ItemsPerPage = count,
        StartIndex = startIndex,
        Resources = paginated
    };
}

// ❌ AVOID - Filtering happens in database, bypasses pagination
public async Task<ScimListResponse<ScimUser>> GetUsersAsync_Bad(string filter, int startIndex, int count)
{
    // If you filter at DB level, you can't get accurate TotalResults
    // This violates SCIM spec expectations
}
```

---

## Error Handling

```csharp
public async Task<ScimListResponse<ScimUser>> GetUsersAsync(string filter, int startIndex, int count)
{
    try
    {
        // Validate startIndex
        if (startIndex < 1)
            throw new ArgumentException("startIndex must be >= 1");

        // Validate count
        if (count < 1 || count > 1000)
            throw new ArgumentException("count must be 1-1000");

        // Validate filter syntax (basic check)
        if (!string.IsNullOrEmpty(filter) && !IsValidFilter(filter))
            throw new ArgumentException("Invalid filter syntax");

        // Apply filter and return
        var allUsers = _users.Values.ToList();
        var filtered = ApplyFilter(allUsers, filter);
        var paginated = filtered.Skip(startIndex - 1).Take(count).ToList();

        return new ScimListResponse<ScimUser>
        {
            TotalResults = filtered.Count,
            ItemsPerPage = count,
            StartIndex = startIndex,
            Resources = paginated
        };
    }
    catch (ArgumentException ex)
    {
        _logger.LogWarning(ex, "Invalid filter or pagination parameters");
        throw new ScimException("Invalid request", "400", ex.Message);
    }
}

private bool IsValidFilter(string filter)
{
    // Basic syntax validation
    var validOperators = new[] { "eq", "ne", "co", "sw", "ew", "pr", "and", "or", "not" };
    return validOperators.Any(op => filter.Contains($" {op} ") || filter.Contains($" {op}"));
}
```

---

## Summary Table

| Approach | Complexity | Performance | Compliance | Recommendation |
|----------|-----------|-------------|-----------|-----------------|
| **Simple String Matching** | Low | Fast | Partial | For MVP/POC |
| **Enhanced Filtering** | Medium | Good | Good | Recommended |
| **LINQ Expressions** | High | Excellent | Very Good | For complex scenarios |
| **External Library** | Medium | Excellent | Complete | For full SCIM support |

**Recommendation:** Start with **Enhanced Filtering** (Option shown above), which balances simplicity with functionality.

