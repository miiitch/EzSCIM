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

### Bug Fixes & Regression Reports
- **[Integration xUnit Fixture Fix](./integration-xunit-fixture-fix.md)** - Fix for 44 failing tests (duplicate IClassFixture / ICollectionFixture, April 2026)

---

## 🎯 Running Tests

### All Tests
```bash
dotnet test
```

### Unit Tests Only
```bash
dotnet test EzSCIM.UnitTests/EzSCIM.UnitTests.csproj
```

### Integration Tests Only
```bash
dotnet test EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj
```

### Specific Collection
```bash
dotnet test EzSCIM.IntegrationTests/EzSCIM.IntegrationTests.csproj \
  --filter "FullyQualifiedName~ScimValidatorComplianceTests"
```

### Filter Tests Only
```bash
dotnet test --filter "FullyQualifiedName~FilterTranslator"
```

### With Coverage
```bash
dotnet test /p:CollectCoverageMetrics=true
```

---

## 📊 Test Coverage

### Unit Tests (`EzSCIM.UnitTests`)
- `ScimUserFilterTranslatorTests` — 13 tests
- `GenericScimFilterTranslatorTests` — 13 tests
- `RepositoryAdapterIntegrationTests` — 14 tests
- `ScimGroupFilterTranslatorTests` — 13 tests
- Other unit tests — 217 tests
- **Total Unit Tests**: 270 ✅

### Integration Tests (`EzSCIM.IntegrationTests`)

| Collection | Tests | PostgreSQL fixture | Status |
|---|---|---|---|
| `GroupsControllerIntegrationTests` | 17 | `IClassFixture` | ✅ |
| `UsersControllerIntegrationTests` | 19 | `IClassFixture` | ✅ |
| `ScimValidatorComplianceTests` | 27 | `ICollectionFixture` | ✅ |
| `EntraIdRequestPatternsTests` | 17 | `ICollectionFixture` | ✅ |
| `ScimPatchApplierUnitTests` | 3 | none (in-memory) | ✅ |
| **Total Integration Tests** | **83** | | **✅** |

### Overall
- **Total Tests**: 353 ✅
- **Failing**: 0
- **Status**: ✅ All passing

---

## 📖 Test Categories

### Unit Tests
- Individual component testing
- Mock dependencies
- Fast execution (< 100 ms total)
- Path: `EzSCIM.UnitTests/`

### Integration Tests
- Full HTTP stack with PostgreSQL via Testcontainers
- Real database (Docker container per test collection)
- Transaction rollback for test isolation
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
EzSCIM.UnitTests/                         # 270 unit tests
├── UsersControllerTests.cs
├── GroupsControllerTests.cs
├── InMemoryScimRepositoryTests.cs
├── SchemaJsonSerializationTests.cs
├── ScimSchemaGeneratorTests.cs
├── Filtering/                            # Filter translator tests
└── Integration/                          # Repository adapter tests

EzSCIM.IntegrationTests/                  # 83 integration tests
├── ScimValidatorComplianceTests.cs       # 27 tests (ICollectionFixture)
├── EntraIdRequestPatternsTests.cs        # 17 tests (ICollectionFixture)
├── GroupsControllerIntegrationTests.cs   # 17 tests (IClassFixture)
├── UsersControllerIntegrationTests.cs    # 19 tests (IClassFixture)
├── ScimPatchApplierUnitTests.cs          # 3 tests (no DB)
└── ScimWebApplicationFactory.cs         # Shared factory (Testcontainers PostgreSQL)
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

    var results = users.Where(predicate).ToList();
    Assert.NotEmpty(results);
}
```

### Testing Endpoints (Integration)
```csharp
[Fact]
public async Task GetUsers_ReturnsUsers_WhenAuthenticated()
{
    var response = await _client.GetAsync("/scim/Users");
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
}
```

### xUnit Fixture Pattern (correct)
```csharp
// One shared PostgreSQL container for the whole collection
[CollectionDefinition("MyCollection")]
public class MyCollectionFixture : ICollectionFixture<ScimWebApplicationFactory> { }

// Test class — do NOT also add IClassFixture for the same type
[Collection("MyCollection")]
public class MyTests : IAsyncLifetime
{
    public MyTests(ScimWebApplicationFactory factory) { /* injected from collection */ }
}
```

---

## ✅ Quality Checklist

- [x] All tests pass locally
- [x] Coverage above 80%
- [x] No failing tests
- [x] Authentication tests included
- [x] Error scenarios tested
- [x] Edge cases covered
- [x] Integration tests passing
- [x] Performance acceptable
- [x] Documentation updated
- [x] Examples provided

---

## 📊 Test Results Summary

### Latest Run (April 15, 2026)
- **Status**: ✅ All Passing
- **Unit tests**: 270 / 270
- **Integration tests**: 83 / 83
- **Total Tests**: 353
- **Failed**: 0
- **Skipped**: 0
- **Duration**: < 5 s (unit) + ~2 s (integration)

### By Component
- **Unit — Filters**: 39 tests ✅
- **Unit — Repository**: 14 tests ✅
- **Unit — Controllers**: 43 tests ✅
- **Unit — Schema**: 174 tests ✅
- **Integration — SCIM Validator**: 27 tests ✅
- **Integration — Entra ID**: 17 tests ✅
- **Integration — Users API**: 19 tests ✅
- **Integration — Groups API**: 17 tests ✅
- **Integration — Patch Applier**: 3 tests ✅

---

## 🔗 Related Documentation

- [Quick Start Guide](../guides/quickstart.md)
- [Development Setup](../guides/development-setup.md)
- [Filter Documentation](../filters/overview.md)
- [Repository Integration](../migration/quick-start-repository.md)
- [Integration xUnit Fixture Fix](./integration-xunit-fixture-fix.md)

---

## 📚 Test Resources

- [xUnit Documentation](https://xunit.net/)
- [Shouldly Documentation](https://shouldly.readthedocs.io/)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [SCIM 2.0 Test Scenarios](https://tools.ietf.org/html/rfc7644)

---

**Status**: ✅ Complete  
**Last Updated**: April 15, 2026  
**Test Framework**: xUnit 2.9.2 + Testcontainers.PostgreSql 4.1.0 + Shouldly  
**Coverage**: 80%+
