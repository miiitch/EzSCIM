# Testing

## Test projects

| Project | Type | DB | Description |
|---|---|---|---|
| `EzSCIM.UnitTests` | Unit | In-memory | Controllers, filter translator, patch applier — no DB |
| `EzSCIM.IntegrationTests` | Integration | PostgreSQL (Testcontainers) | Full HTTP stack via WebApplicationFactory |

---

## Integration test architecture

### Database

Integration tests use a real **PostgreSQL** database via [Testcontainers](https://dotnet.testcontainers.org/).
The container is started once per test collection and shared across all tests in that collection.

### `ScimWebApplicationFactory`

Bootstraps the full `EzSCIM.EntraID.Demo` application with DB overrides:

1. Removes SQL Server registration from Demo's `Program.cs`
2. Registers `PostgreSqlScimDbContext` with Testcontainers connection string
3. Forwards `ScimDbContextBase` → `PostgreSqlScimDbContext` (for `DemoUserGroupRepository`)
4. Registers `DemoUserGroupRepository`, `GenericScimFilterTranslator<T>`, `DemoScimRepository`
5. Disables JWT authentication (always returns authenticated)
6. Calls `EnsureCreatedAsync()` + `SeedData.SeedAsync()` before tests

### `PostgreSqlScimDbContext`

Located in `EzSCIM.IntegrationTests/Data/PostgreSqlScimDbContext.cs`.

Inherits `ScimDbContextBase` from `EzSCIM.Demo.Data`, overrides `OnModelCreating`
to add `HasColumnType("jsonb")` for all JSON columns.

```csharp
public class PostgreSqlScimDbContext : ScimDbContextBase
{
    public PostgreSqlScimDbContext(DbContextOptions<PostgreSqlScimDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DemoUserEntity>(e =>
        {
            e.Property(u => u.EmailsJson).HasColumnType("jsonb");
            e.Property(u => u.PhoneNumbersJson).HasColumnType("jsonb");
            e.Property(u => u.AddressesJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<DemoGroupEntity>(e =>
        {
            e.Property(g => g.MembersJson).HasColumnType("jsonb");
        });
    }
}
```

---

## Test collections

Each collection shares one `ScimWebApplicationFactory` instance (one PostgreSQL container).
Collections run in parallel.

| Collection | File | Shares factory? |
|---|---|---|
| `UsersIntegration` | `UsersControllerIntegrationTests.cs` | Yes (fixture) |
| `GroupsIntegration` | `GroupsControllerIntegrationTests.cs` | Yes (fixture) |
| `EntraIdIntegration` | `EntraIdRequestPatternsTests.cs` | Yes (fixture) |
| `ScimValidatorCompliance` | `ScimValidatorComplianceTests.cs` | Yes (fixture) |

---

## Test isolation strategy

Tests use database transactions for isolation. Each test:
1. Opens a transaction in `InitializeAsync`
2. Runs the test (HTTP calls go through the app which uses the same transaction scope)
3. Rolls back the transaction in `DisposeAsync`

> **Important**: HTTP calls via `HttpClient` run in the server's own DI scope, not the
> test's scope. Rollback only isolates changes made through the test's direct DbContext
> reference — not changes made by the server through HTTP. Use unique data per test
> (GUIDs in names) to avoid cross-test conflicts.

---

## Seed data

Located in `EzSCIM.IntegrationTests/Data/SeedData.cs`.

Seeded once before the first test in each collection:
- 3 users: `user-001` (john.doe@example.com), `user-002` (jane.smith@example.com), `user-003`
- 3 groups: `group-001` (Administrators), `group-002` (Developers), `group-003` (Users)

Tests that target specific IDs (`user-001`, `group-001`) rely on seed data.
Tests that create new resources use `Guid.NewGuid()` in names to avoid conflicts.

---

## Bug-first testing methodology

When a bug is reported:

1. **Write a failing test first** in `ScimValidatorComplianceTests.cs`
2. Document in the test:
   - Validator test name
   - Which validator runs were affected
   - Exact error message from validator
   - Root cause explanation
3. Verify the test **fails** (red)
4. Implement the fix
5. Verify the test **passes** (green)

Test name pattern:
```
<Endpoint>_<Operation>_Should<ExpectedBehavior>
```

Example:
```csharp
[Fact]
public async Task PatchUser_ReplaceAttributes_ShouldPersistAllValues()
```

---

## Running with output

```bash
# Show test output (useful for debugging integration tests)
dotnet test EzSCIM.IntegrationTests \
  --logger "console;verbosity=detailed" \
  -- xunit.diagnosticMessages=true
```

---

## Adding a new integration test

1. Choose the appropriate collection file
2. Inject `PostgreSqlScimDbContext` for transaction isolation
3. Use `_client.GetAsync/PostAsJsonAsync/PatchAsJsonAsync/DeleteAsync` for HTTP calls
4. Assert `response.StatusCode`, then deserialize and assert the body
5. For persistence tests: make an HTTP call, then `GET` the resource and verify

```csharp
[Fact]
public async Task CreateUser_WhenValid_ShouldPersistAndReturnCreated()
{
    // Arrange
    var newUser = new { schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" },
                        userName = $"test.{Guid.NewGuid():N}@acme.com", active = true };

    // Act
    var response = await _client.PostAsJsonAsync("/scim/Users", newUser);

    // Assert status
    response.StatusCode.ShouldBe(HttpStatusCode.Created);

    // Assert body
    var created = await response.Content.ReadFromJsonAsync<ScimUser>();
    created.ShouldNotBeNull();
    created.Id.ShouldNotBeNullOrEmpty();

    // Assert persistence
    var get = await _client.GetAsync($"/scim/Users/{created.Id}");
    get.StatusCode.ShouldBe(HttpStatusCode.OK);
}
```

---

**See also**: [architecture.md](./architecture.md) | [scim-validator.md](./scim-validator.md)

