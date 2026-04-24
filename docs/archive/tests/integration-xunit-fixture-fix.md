# Integration Test Fix: Duplicate xUnit Fixture (April 2026)

## Summary

44 of 83 integration tests were systematically failing with `Connection refused` when
connecting to the PostgreSQL Testcontainer. The root cause was a **duplicate xUnit
fixture declaration** in two test classes that caused two PostgreSQL containers to be
started simultaneously — the second one always failed.

After the fix: **83/83 integration tests pass**.

---

## Affected Tests

| Collection | Tests failing | Error |
|---|---|---|
| `ScimValidatorComplianceTests` | 27 / 27 | `Npgsql.NpgsqlException: Failed to connect to 127.0.0.1:XXXXX` |
| `EntraIdRequestPatternsTests` | 17 / 17 | `Npgsql.NpgsqlException: Failed to connect to 127.0.0.1:XXXXX` |

`GroupsControllerIntegrationTests` and `UsersControllerIntegrationTests` were **not
affected** — they only used `IClassFixture<T>` with no collection definition.

---

## Root Cause

Both failing classes declared `ScimWebApplicationFactory` as a fixture **twice**:

```csharp
// ❌ Before — duplicate fixture declaration
[Collection("ScimValidatorCompliance")]
public class ScimValidatorComplianceTests : IClassFixture<ScimWebApplicationFactory>, IAsyncLifetime
// ...

// At the bottom of the same file:
[CollectionDefinition("ScimValidatorCompliance")]
public class ScimValidatorComplianceCollection : ICollectionFixture<ScimWebApplicationFactory> { }
```

### xUnit Fixture Resolution

In xUnit 2.x, when a test class belongs to a collection **and** that collection has an
`ICollectionFixture<T>` in its `[CollectionDefinition]`, xUnit creates a **shared
collection-level factory**.

If the class *also* declares `IClassFixture<T>` for the same `T`, xUnit creates a
**second, per-class factory** on top of the shared one. Both call `InitializeAsync()`,
which means **two PostgreSQL Testcontainer instances start simultaneously**.

The second container consistently failed to become ready, yielding `Connection refused`
for every test in the class.

### Proof via Timing

| Collection | Run duration | Result |
|---|---|---|
| `GroupsControllerIntegrationTests` (class fixture only) | ~797 ms | ✅ Pass |
| `ScimValidatorComplianceTests` (duplicate fixture) | ~56 ms | ❌ Fail |

56 ms is far too short for a PostgreSQL container to start — it confirms an immediate
factory initialization failure rather than a container readiness timeout.

---

## Fix Applied

Remove `IClassFixture<ScimWebApplicationFactory>` from any class whose collection
already provides `ICollectionFixture<ScimWebApplicationFactory>`. The collection fixture
is automatically injected into the constructor — the `IClassFixture` declaration is
both redundant and harmful.

### `ScimValidatorComplianceTests.cs`

```csharp
// ✅ After
[Collection("ScimValidatorCompliance")]
public class ScimValidatorComplianceTests : IAsyncLifetime
```

### `EntraIdRequestPatternsTests.cs`

```csharp
// ✅ After
[Collection("EntraIdIntegration")]
public class EntraIdRequestPatternsTests : IAsyncLifetime
```

The `ScimWebApplicationFactory factory` constructor parameter is still injected
automatically from the collection fixture — no other code change was needed.

---

## Result

| Metric | Before | After |
|---|---|---|
| Failing integration tests | 44 | **0** |
| Passing integration tests | 39 | **83** |
| Total integration tests | 83 | 83 |
| PostgreSQL containers per collection | 2 (one failing) | **1** |

---

## xUnit Fixture Rules (Reference)

| Scenario | Instances | When to use |
|---|---|---|
| `IClassFixture<T>` only | 1 per test class | Each class needs its own isolated instance |
| `ICollectionFixture<T>` via `[CollectionDefinition]` | 1 shared across the collection | Multiple classes share one expensive resource (e.g., a single DB container) |
| **Both for the same `T`** ⚠️ | **2 instances** | **Avoid — almost always a bug** |

### Correct pattern for a shared container

```csharp
// 1. Define the collection fixture ONCE
[CollectionDefinition("MyCollection")]
public class MyCollectionFixture : ICollectionFixture<ScimWebApplicationFactory> { }

// 2. In the test class — do NOT also add IClassFixture<ScimWebApplicationFactory>
[Collection("MyCollection")]
public class MyTests : IAsyncLifetime
{
    public MyTests(ScimWebApplicationFactory factory) { /* collection fixture injected */ }
}
```

### Correct pattern for a per-class container

```csharp
// No [CollectionDefinition] needed
[Collection("IsolatedCollection")]
public class MyTests : IClassFixture<ScimWebApplicationFactory>, IAsyncLifetime
{
    public MyTests(ScimWebApplicationFactory factory) { /* class fixture injected */ }
}
```

---

## Files Changed

| File | Change |
|---|---|
| `EzSCIM.IntegrationTests/ScimValidatorComplianceTests.cs` | Removed `: IClassFixture<ScimWebApplicationFactory>` |
| `EzSCIM.IntegrationTests/EntraIdRequestPatternsTests.cs` | Removed `: IClassFixture<ScimWebApplicationFactory>` |

---

**Date**: April 15, 2026  
**Impact**: 44 integration tests fixed (53 % of the suite)  
**Test framework**: xUnit 2.9.2 + Testcontainers.PostgreSql 4.1.0

