# Unit Tests - ScimAPI

This project contains unit tests for the SCIM API implementation.

## Test Structure

### InMemoryScimRepositoryTests.cs
Tests for the in-memory SCIM repository implementation with **60+ tests** covering:

#### User CRUD (7 tests)
- Creation with automatic ID generation and metadata
- Retrieval by ID
- Full update
- Deletion

#### User Filters (14 tests)
- Basic operators: `eq`, `sw`, `co`, `pr`
- Logical operators: `and`, `or`, `not`
- Complex expressions with parentheses
- Filters on all fields (userName, active, displayName, name.givenName, name.familyName, emails)

#### Pagination (2 tests)
- First page pagination
- Subsequent page pagination

#### User PATCH (4 tests)
- Replacement of simple fields (active, displayName)
- Replacement of nested fields (name.givenName)
- Multiple operations

#### Group CRUD (4 tests)
- Create, retrieve, update, delete

#### Group Filters (3 tests)
- Filters by displayName: `eq`, `co`, `sw`

#### Group PATCH (2 tests)
- Add members
- Remove members

#### Schemas (2 tests)
- Add custom schemas
- Retrieve schemas

#### Edge Cases (7 tests)
- Null value handling
- Case-insensitive search
- Empty results

### UsersControllerTests.cs
Tests for the users controller with **25 tests** covering:
- GET /scim/Users (with/without filters, pagination, errors)
- GET /scim/Users/{id}
- POST /scim/Users (creation, conflicts)
- PUT /scim/Users/{id}
- PATCH /scim/Users/{id}
- DELETE /scim/Users/{id}

### GroupsControllerTests.cs
Tests for the groups controller with **18 tests** covering:
- GET /scim/Groups (with/without filters, errors)
- GET /scim/Groups/{id}
- POST /scim/Groups (creation, conflicts)
- PUT /scim/Groups/{id}
- PATCH /scim/Groups/{id} (add/remove members)
- DELETE /scim/Groups/{id}

## Running Tests

### All tests
```bash
dotnet test
```

### Tests from a specific file
```bash
dotnet test --filter "FullyQualifiedName~InMemoryScimRepositoryTests"
dotnet test --filter "FullyQualifiedName~UsersControllerTests"
dotnet test --filter "FullyQualifiedName~GroupsControllerTests"
```

### A specific test
```bash
dotnet test --filter "FullyQualifiedName~GetUsers_FilterWithComplexExpression"
```

### With detailed output
```bash
dotnet test --verbosity detailed
```

### With code coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Tools Used

- **xUnit** - Test framework
- **Shouldly** - Readable and expressive assertions (MIT License - free)
- **Moq** - Mocking dependencies for controller tests

## Test Examples

### Simple filter test
```csharp
[Fact]
public async Task GetUsers_FilterByUserName_Eq_ShouldReturnMatchingUser()
{
    // Arrange
    await _repository.CreateUserAsync(new ScimUser { UserName = "john.doe@example.com" });
    await _repository.CreateUserAsync(new ScimUser { UserName = "jane.smith@example.com" });

    // Act
    var result = await _repository.GetUsersAsync(filter: "userName eq \"john.doe@example.com\"");

    // Assert
    result.TotalResults.ShouldBe(1);
    result.Resources.First().UserName.ShouldBe("john.doe@example.com");
}
```

### Complex filter test
```csharp
[Fact]
public async Task GetUsers_FilterWithComplexExpression_ShouldReturnMatchingUsers()
{
    // Arrange
    await _repository.CreateUserAsync(new ScimUser { UserName = "john@example.com", Active = true });
    await _repository.CreateUserAsync(new ScimUser { UserName = "jane@example.com", Active = true });
    await _repository.CreateUserAsync(new ScimUser { UserName = "bob@example.com", Active = false });

    // Act
    var result = await _repository.GetUsersAsync(
        filter: "(userName sw \"john\" or userName sw \"jane\") and active eq true"
    );

    // Assert
    result.TotalResults.ShouldBe(2);
}
```

### Controller test
```csharp
[Fact]
public async Task CreateUser_WhenValid_ShouldReturnCreated()
{
    // Arrange
    var newUser = new ScimUser { UserName = "newuser@example.com" };
    _mockRepository.Setup(r => r.GetUserByUserNameAsync(newUser.UserName))
        .ReturnsAsync((ScimUser?)null);
    _mockRepository.Setup(r => r.CreateUserAsync(It.IsAny<ScimUser>()))
        .ReturnsAsync(new ScimUser { Id = "123", UserName = "newuser@example.com" });

    // Act
    var result = await _controller.CreateUser(newUser);

    // Assert
    var createdResult = (CreatedAtActionResult)result;
    var returnedUser = (ScimUser)createdResult.Value!;
    returnedUser.UserName.ShouldBe("newuser@example.com");
}
```

## Code Coverage

Tests cover:
- ✅ 100% of public repository methods
- ✅ 100% of controller endpoints
- ✅ All SCIM filter operators
- ✅ All error cases (404, 409, 500)
- ✅ Complete PATCH operations
- ✅ Edge cases

## Microsoft Entra Scenarios Tested

Tests validate all Microsoft Entra usage scenarios:
1. Existence check by userName
2. Search by externalId (Azure Object ID)
3. Active user filtering
4. Create/update with conflict handling
5. User deactivation (PATCH active = false)
6. Group and member management

## Continuous Integration

Tests are designed to run in a CI/CD pipeline:
- Fast (< 5 seconds for all tests)
- Isolated (each test is independent)
- Deterministic (no external dependencies)
- Reproducible (same results on every run)

## Adding New Tests

To add a new test:

1. Choose the appropriate file (Repository or Controller)
2. Use the **Arrange-Act-Assert** pattern
3. Descriptive name: `MethodName_Scenario_ExpectedResult`
4. Use Shouldly for assertions (simple and readable syntax)
5. Mock dependencies if necessary

Example:
```csharp
[Fact]
public async Task MyNewTest_WhenCondition_ShouldReturnResult()
{
    // Arrange
    // Prepare test data
    
    // Act
    // Execute the method under test
    
    // Assert
    // Verify the result with Shouldly
    result.ShouldBe(expected);
    result.ShouldNotBeNull();
    result.Count.ShouldBe(5);
}
```

### Common Shouldly Syntax

```csharp
// Equality
value.ShouldBe(expected);
value.ShouldNotBe(unexpected);

// Null
value.ShouldBeNull();
value.ShouldNotBeNull();

// Booleans
condition.ShouldBeTrue();
condition.ShouldBeFalse();

// Collections
collection.Count.ShouldBe(5);
collection.ShouldBeEmpty();
collection.ShouldContain(item);

// Comparisons
number.ShouldBeGreaterThan(10);
number.ShouldBeLessThan(100);
date.ShouldBeInRange(start, end);

// Types
object.ShouldBeOfType<MyType>();
```

## Troubleshooting

### Randomly failing tests
- Check test isolation
- Ensure there is no shared state

### StackOverflowException
- Complex filters with unbalanced parentheses can cause infinite recursion
- Implemented solution: removal of outer parentheses before processing

### Slow tests
- In-memory tests should be fast
- If slow: check for multiple enumerations or expensive operations
