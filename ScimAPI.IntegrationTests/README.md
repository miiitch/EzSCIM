# SCIM API - Integration Tests

## Overview

This project contains integration tests for the SCIM API using **Testcontainers** and **PostgreSQL**. These tests verify end-to-end functionality by making real HTTP calls to the API and persisting data in a PostgreSQL database running in a Docker container.

## Requirements

- ✅ **Docker Desktop** installed and running
- ✅ .NET 10.0 SDK
- ✅ Internet connection (for Docker image download on first run)

## Architecture

### Independent Test Collections

The integration tests are organized into **2 independent xUnit collections**:

- **`UsersIntegration`** - 17 tests for UsersController
- **`GroupsIntegration`** - 18 tests for GroupsController

Each collection uses its own PostgreSQL container instance, enabling **parallel execution** without interference.

### Transaction Isolation

Each individual test is isolated using **database transactions**:

1. `InitializeAsync()` - Starts a transaction before each test
2. Test execution - All database changes occur within the transaction
3. `DisposeAsync()` - Rolls back the transaction after the test

This ensures **complete isolation** between tests without recreating the database schema.

### Automatic Port Assignment

Testcontainers automatically assigns random ports to PostgreSQL containers, preventing port conflicts during parallel execution.

## Seed Data

The database is pre-populated with test data on startup:

### Users (5 total)

| UserName                    | DisplayName    | Active | Title                  | ID       |
|-----------------------------|----------------|--------|------------------------|----------|
| john.doe@example.com        | John Doe       | ✅     | Software Developer     | user-001 |
| jane.smith@example.com      | Jane Smith     | ✅     | System Administrator   | user-002 |
| bob.wilson@example.com      | Bob Wilson     | ❌     | Former Employee        | user-003 |
| alice.johnson@example.com   | Alice Johnson  | ✅     | Project Manager        | user-004 |
| charlie.brown@example.com   | Charlie Brown  | ✅     | Senior Developer       | user-005 |

### Groups (3 total)

| DisplayName     | Members Count | Members                          | ID        |
|-----------------|---------------|----------------------------------|-----------|
| Administrators  | 2             | Jane Smith, Alice Johnson        | group-001 |
| Developers      | 2             | John Doe, Charlie Brown          | group-002 |
| Users           | 4             | John, Jane, Alice, Charlie       | group-003 |

## Running Tests

### Run All Integration Tests

```powershell
dotnet test ScimAPI.IntegrationTests
```

### Run Specific Collection

```powershell
# Users tests only
dotnet test --filter "FullyQualifiedName~UsersControllerIntegrationTests"

# Groups tests only
dotnet test --filter "FullyQualifiedName~GroupsControllerIntegrationTests"
```

### Run via PowerShell Script

```powershell
.\Run-AllTests.ps1
```

This script runs unit tests first, then integration tests.

### Verbose Output

```powershell
dotnet test --verbosity detailed
```

## Execution Time

- **Container Startup**: 5-10 seconds per collection (first run may take longer to download PostgreSQL image)
- **Test Execution**: ~2-5 seconds per collection
- **Total**: ~15-20 seconds for all integration tests

## Test Examples

### Filter Tests

The integration tests verify SCIM filter functionality against a real database:

```http
# Get active users only
GET /scim/Users?filter=active eq true
# Returns: 4 users (john, jane, alice, charlie)

# Get users by name
GET /scim/Users?filter=userName eq "john.doe@example.com"
# Returns: 1 user (john)

# Get groups by display name
GET /scim/Groups?filter=displayName eq "Developers"
# Returns: 1 group (Developers)

# Get groups containing text
GET /scim/Groups?filter=displayName co "Admin"
# Returns: 1 group (Administrators)
```

### CRUD Operations

```csharp
// Create user
POST /scim/Users
Body: { "userName": "newuser@example.com", "active": true }
Response: 201 Created

// Update user
PUT /scim/Users/user-001
Body: { "userName": "updated@example.com", "active": true }
Response: 200 OK

// Patch user
PATCH /scim/Users/user-001
Body: { "Operations": [{ "op": "replace", "path": "active", "value": false }] }
Response: 200 OK

// Delete user
DELETE /scim/Users/user-001
Response: 204 No Content
```

## Test Coverage

### UsersController (17 tests)

- ✅ GET /scim/Users (no filter, with filter, pagination, active filter)
- ✅ GET /scim/Users/{id} (exists, not exists)
- ✅ POST /scim/Users (valid, duplicate)
- ✅ PUT /scim/Users/{id} (exists, not exists)
- ✅ PATCH /scim/Users/{id} (valid, not exists)
- ✅ DELETE /scim/Users/{id} (exists, not exists)

### GroupsController (18 tests)

- ✅ GET /scim/Groups (no filter, with filter, contains filter)
- ✅ GET /scim/Groups/{id} (exists, not exists)
- ✅ POST /scim/Groups (valid, duplicate)
- ✅ PUT /scim/Groups/{id} (exists, not exists)
- ✅ PATCH /scim/Groups/{id} (add member, not exists)
- ✅ DELETE /scim/Groups/{id} (exists, not exists)

## Troubleshooting

### Docker Not Running

```
Error: Docker daemon is not running
```

**Solution**: Start Docker Desktop and ensure it's running.

### Port Already in Use

This should not happen due to automatic port assignment, but if it does:

```powershell
# Stop all containers
docker stop $(docker ps -aq)
```

### PostgreSQL Image Download

First run downloads the `postgres:16` image (~100MB):

```
Pulling image: postgres:16
```

**Solution**: Wait for download to complete. Subsequent runs will use the cached image.

### Test Failures

If tests fail:

1. Check Docker is running
2. Check logs in test output (use `--verbosity detailed`)
3. Verify seed data in `SeedData.cs`
4. Check for SQL errors in output

## Debugging

### View Test Logs

Tests output detailed logs via `ITestOutputHelper`:

```
[10:30:45] PostgreSQL container started
[10:30:45] Connection string: Host=localhost;Port=55432;Database=scimdb;...
[10:30:46] Database schema created
[10:30:46] Seeded 5 users
[10:30:46] Seeded 3 groups
[10:30:46] Transaction started for test isolation
[REQUEST] GET /scim/Users
[RESPONSE] 200 - {"totalResults":5,"startIndex":1,...}
[10:30:47] Transaction rolled back
```

### Connect to PostgreSQL

While tests are running, you can connect to the container (port will be random):

```powershell
# Get container port
docker ps

# Connect with psql
docker exec -it <container-id> psql -U scimuser -d scimdb
```

## Implementation Details

- **Framework**: xUnit with `IAsyncLifetime` for setup/teardown
- **HTTP Client**: `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory`
- **Database**: PostgreSQL 16 in Testcontainers
- **ORM**: Entity Framework Core 10.0 with Npgsql
- **Assertions**: Shouldly for fluent assertions
- **Authentication**: Disabled for tests (authorization policy removed)

## Related Documentation

- [Run-AllTests.ps1](../Run-AllTests.ps1) - PowerShell script to run all tests
- [INTEGRATION-TESTS-COMPLETE.md](../INTEGRATION-TESTS-COMPLETE.md) - Complete implementation documentation
- [ScimAPI](../ScimAPI/) - Main API project
- [ScimAPI.UnitTests](../ScimAPI.UnitTests/) - Unit tests with mocks

