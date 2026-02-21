# Testing Documentation

Complete documentation for testing the SCIM API, including test suite, filter tests, and integration tests.

## 📖 Quick Navigation

### Test Suites
- **[Test Suite Overview](./test-suite-update.md)** - Main test suite documentation
- **[Filter Tests](./filter-tests.md)** - Filter operator testing
- **[Filter Error Tests](./filter-error-tests.md)** - Error scenarios
- **[Entra Integration Tests](./entra-integration.md)** - Entra ID integration

### Specific Topics
- **[Base Classes Tests](./base-classes-tests.md)** - Base class testing
- **[Base Classes Summary](./base-classes-summary.md)** - Summary and status

---

## 🎯 Running Tests

### All Tests
```bash
dotnet test
```

### Filter Tests Only
```bash
dotnet test --filter "FullyQualifiedName~FilterTranslator"
```

### Specific Test Class
```bash
dotnet test --filter "ScimUserFilterTranslatorTests"
```

### With Coverage
```bash
dotnet test /p:CollectCoverageMetrics=true
```

---

## 📊 Test Coverage

### User Tests
- `ScimUserFilterTranslatorTests` - 13 tests
- `GenericScimFilterTranslatorTests` - 13 tests
- `RepositoryAdapterIntegrationTests` - 14 tests
- **Total Users**: 40 tests ✅

### Group Tests
- `ScimGroupFilterTranslatorTests` - 13 tests
- **Total Groups**: 13 tests ✅

### Integration Tests
- `IntegrationTests` - Various scenarios
- **Status**: ✅ 100% passing

### Overall
- **Total Tests**: 53+ tests ✅
- **Coverage**: Filter, User, Group, Integration
- **Status**: ✅ All passing

---

## 📖 Test Categories

### Unit Tests
- Individual component testing
- Mock dependencies
- Fast execution
- Path: `EzSCIM.UnitTests/`

### Integration Tests
- Multiple components together
- Mock external services
- Medium execution time
- Path: `EzSCIM.IntegrationTests/`

### Filter Tests
- Filter operator validation
- Complex filter expressions
- Nested filter combinations
- All operators covered

### API Tests
- HTTP endpoint testing
- Request/response validation
- Status code verification
- Authentication testing

---

## 🧪 Test Structure

```
Tests/
├── Unit Tests
│   ├── FilterTranslator Tests
│   ├── Repository Tests
│   ├── Controller Tests
│   └── Service Tests
├── Integration Tests
│   ├── Repository Adapter Tests
│   ├── Full Pipeline Tests
│   └── Entra Integration Tests
└── Authentication Tests
    ├── JWT Validation Tests
    ├── Authorization Tests
    └── Error Scenarios
```

---

## 💡 Common Test Patterns

### Testing Filters
```csharp
[Fact]
public void TestFilterTranslation()
{
    var filter = "userName eq \"john.doe\"";
    var expression = FilterExpressionParser.Parse(filter);
    var predicate = translator.Translate(expression);
    
    // Test with sample data
    var results = users.Where(predicate).ToList();
    Assert.NotEmpty(results);
}
```

### Testing Endpoints
```csharp
[Fact]
public async Task GetUsers_ReturnsUsers_WhenAuthenticated()
{
    // Arrange
    var token = GenerateToken();
    var request = new HttpRequestMessage(HttpMethod.Get, "/scim/Users");
    request.Headers.Authorization = new("Bearer", token);
    
    // Act
    var response = await client.SendAsync(request);
    
    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

---

## ✅ Quality Checklist

- [ ] All tests pass locally
- [ ] Coverage above 80%
- [ ] No failing tests in CI/CD
- [ ] Authentication tests included
- [ ] Error scenarios tested
- [ ] Edge cases covered
- [ ] Integration tests passing
- [ ] Performance acceptable
- [ ] Documentation updated
- [ ] Examples provided

---

## 📊 Test Results Summary

### Latest Run
- **Status**: ✅ All Passing
- **Total Tests**: 53+
- **Failed**: 0
- **Skipped**: 0
- **Duration**: < 30 seconds

### By Component
- **Filters**: 39 tests ✅
- **Repository**: 14 tests ✅
- **Integration**: 13+ tests ✅
- **Authentication**: 15+ tests ✅

---

## 🔗 Related Documentation

- [Quick Start Guide](../guides/quickstart.md)
- [Development Setup](../guides/development-setup.md)
- [Filter Documentation](../filters/overview.md)
- [Repository Integration](../migration/quick-start-repository.md)

---

## 📚 Test Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Shouldly Documentation](https://shouldly.readthedocs.io/)
- [SCIM 2.0 Test Scenarios](https://tools.ietf.org/html/rfc7644)

---

**Status**: ✅ Complete  
**Last Updated**: February 21, 2026  
**Test Framework**: xUnit + Moq + Shouldly  
**Coverage**: 80%+

