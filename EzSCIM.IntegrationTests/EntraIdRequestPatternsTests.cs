using System.Net;
using System.Net.Http.Json;
using EzSCIM.IntegrationTests.Data;
using EzSCIM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace EzSCIM.IntegrationTests;

/// <summary>
/// Integration tests that simulate exact request patterns from Microsoft Entra ID (Azure AD).
/// Uses PostgreSQL database via Testcontainers for realistic testing.
/// These tests verify that the SCIM API correctly handles all operations that Entra ID performs.
/// </summary>
[Collection("EntraIdIntegration")]
public class EntraIdRequestPatternsTests : IAsyncLifetime
{
    private readonly ScimWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly PostgreSqlScimDbContext _context;
    private readonly ITestOutputHelper _output;
    private IDbContextTransaction _transaction = null!;

    public EntraIdRequestPatternsTests(
        ScimWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
        
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<PostgreSqlScimDbContext>();
    }

    public async Task InitializeAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        _output.WriteLine($"[{DateTime.Now:HH:mm:ss}] Entra ID test started - PostgreSQL");
    }

    public async Task DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }
        _scope?.Dispose();
    }

    #region Phase 1: Connection Test - Entra ID calls these when testing connection

    [Fact]
    public async Task EntraId_TestConnection_ServiceProviderConfig_ShouldReturn200()
    {
        // Entra ID first calls ServiceProviderConfig to verify SCIM compliance
        _output.WriteLine("[ENTRA-REQUEST] GET /scim/ServiceProviderConfig");

        var response = await _client.GetAsync("/scim/ServiceProviderConfig");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        responseBody.ShouldContain("urn:ietf:params:scim:schemas:core:2.0:ServiceProviderConfig");
    }

    [Fact]
    public async Task EntraId_TestConnection_Schemas_ShouldReturn200WithUserAndGroupSchemas()
    {
        // Entra ID fetches schemas to understand attribute mappings
        _output.WriteLine("[ENTRA-REQUEST] GET /scim/Schemas");

        var response = await _client.GetAsync("/scim/Schemas");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        responseBody.ShouldContain("urn:ietf:params:scim:schemas:core:2.0:User");
        responseBody.ShouldContain("urn:ietf:params:scim:schemas:core:2.0:Group");
    }

    [Fact]
    public async Task EntraId_TestConnection_UserLookup_NonExistent_ShouldReturnEmptyList()
    {
        // Entra ID tests user lookup with a non-existent user - expects empty list, not 404
        var filter = "userName eq \"testconnection@nonexistent.onmicrosoft.com\"";
        _output.WriteLine($"[ENTRA-REQUEST] GET /scim/Users?filter={filter}");

        var response = await _client.GetAsync($"/scim/Users?filter={Uri.EscapeDataString(filter)}");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode} - totalResults should be 0");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimUser>>();
        result.ShouldNotBeNull();
        result.TotalResults.ShouldBe(0);
        result.Resources.ShouldBeEmpty();
    }

    #endregion

    #region Phase 2: Initial Sync - Entra ID reads all users/groups with pagination

    [Fact]
    public async Task EntraId_InitialSync_GetAllUsers_WithPagination_ShouldReturnListResponse()
    {
        // Entra ID fetches users with pagination during initial sync
        _output.WriteLine("[ENTRA-REQUEST] GET /scim/Users?startIndex=1&count=100");

        var response = await _client.GetAsync("/scim/Users?startIndex=1&count=100");
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimUser>>();
        result.ShouldNotBeNull();
        result.Schemas.ShouldContain("urn:ietf:params:scim:api:messages:2.0:ListResponse");
        result.StartIndex.ShouldBe(1);
        result.TotalResults.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task EntraId_InitialSync_GetAllGroups_WithPagination_ShouldReturnListResponse()
    {
        // Entra ID fetches groups with pagination during initial sync
        _output.WriteLine("[ENTRA-REQUEST] GET /scim/Groups?startIndex=1&count=100");

        var response = await _client.GetAsync("/scim/Groups?startIndex=1&count=100");
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimGroup>>();
        result.ShouldNotBeNull();
        result.Schemas.ShouldContain("urn:ietf:params:scim:api:messages:2.0:ListResponse");
    }

    #endregion

    #region Phase 3: User Provisioning - Entra ID creates/updates users

    [Fact]
    public async Task EntraId_UserProvisioning_CheckByUserName_ThenCreate_FullFlow()
    {
        // Step 1: Entra ID checks if user exists by userName
        var userName = $"newuser.entraid.{Guid.NewGuid():N}@company.onmicrosoft.com";
        var filter = $"userName eq \"{userName}\"";
        
        _output.WriteLine($"[ENTRA-STEP1] Check if user exists: {userName}");
        var checkResponse = await _client.GetAsync($"/scim/Users?filter={Uri.EscapeDataString(filter)}");
        checkResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var checkResult = await checkResponse.Content.ReadFromJsonAsync<ScimListResponse<ScimUser>>();
        checkResult?.TotalResults.ShouldBe(0);
        _output.WriteLine($"[ENTRA-STEP1] User does not exist (totalResults=0)");

        // Step 2: User doesn't exist, so Entra ID creates it with full SCIM payload
        var newUser = new
        {
            schemas = new[] 
            { 
                "urn:ietf:params:scim:schemas:core:2.0:User",
                "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User"
            },
            userName = userName,
            externalId = "azure-object-id-" + Guid.NewGuid().ToString("N")[..12],
            name = new
            {
                givenName = "Entra",
                familyName = "TestUser"
            },
            displayName = "Entra TestUser",
            active = true,
            emails = new[]
            {
                new { value = userName, type = "work", primary = true }
            }
        };

        _output.WriteLine("[ENTRA-STEP2] POST /scim/Users - Create user");
        var createResponse = await _client.PostAsJsonAsync("/scim/Users", newUser);
        _output.WriteLine($"[ENTRA-STEP2] Response: {createResponse.StatusCode}");

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        var createdUser = await createResponse.Content.ReadFromJsonAsync<ScimUser>();
        createdUser.ShouldNotBeNull();
        createdUser.Id.ShouldNotBeNullOrEmpty();
        createdUser.UserName.ShouldBe(userName);
        
        _output.WriteLine($"[ENTRA-STEP2] ? User created with ID: {createdUser.Id}");
    }

    [Fact]
    public async Task EntraId_UserProvisioning_CheckByExternalId_ShouldReturnEmptyList()
    {
        // Entra ID sometimes checks by externalId (Azure Object ID) before creating
        var externalId = "nonexistent-azure-object-id-" + Guid.NewGuid().ToString("N")[..12];
        var filter = $"externalId eq \"{externalId}\"";
        
        _output.WriteLine($"[ENTRA-REQUEST] GET /scim/Users?filter=externalId eq \"{externalId}\"");
        var response = await _client.GetAsync($"/scim/Users?filter={Uri.EscapeDataString(filter)}");
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimUser>>();
        result.ShouldNotBeNull();
        // CRITICAL: Entra ID expects empty list (totalResults=0), NOT 404
        result.TotalResults.ShouldBe(0);
    }

    [Fact]
    public async Task EntraId_UserProvisioning_PatchDisable_WhenUserUnassigned()
    {
        // Entra ID disables users via PATCH when they're unassigned from the app
        var userId = "user-001"; // Existing seed user
        
        var patchRequest = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new[]
            {
                new { op = "Replace", path = "active", value = false }
            }
        };

        _output.WriteLine($"[ENTRA-REQUEST] PATCH /scim/Users/{userId} - Disable user (unassigned from app)");
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var patchedUser = await response.Content.ReadFromJsonAsync<ScimUser>();
        patchedUser.ShouldNotBeNull();
        patchedUser.Active.ShouldBeFalse();
        _output.WriteLine($"[ENTRA-RESULT] ? User disabled successfully");
    }

    [Fact]
    public async Task EntraId_UserProvisioning_PatchUpdateMultipleAttributes_ProfileChange()
    {
        // Entra ID updates multiple attributes when user profile changes in Azure AD
        var userId = "user-002";
        
        var patchRequest = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "Replace", path = "displayName", value = "Jane Smith - Updated by Entra" },
                new { op = "Replace", path = "name.givenName", value = "Jane" },
                new { op = "Replace", path = "name.familyName", value = "Smith-Updated" },
                new { op = "Replace", path = "title", value = "Senior System Administrator" }
            }
        };

        _output.WriteLine($"[ENTRA-REQUEST] PATCH /scim/Users/{userId} - Update multiple attributes");
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _output.WriteLine($"[ENTRA-RESULT] ? Multiple attributes updated");
    }

    [Fact]
    public async Task EntraId_UserProvisioning_PatchEnterpriseExtension_DepartmentSync()
    {
        // Entra ID syncs enterprise extension attributes (department, manager, employeeNumber)
        var userId = "user-001";
        
        var patchRequest = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "Replace", path = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:department", value = "Engineering" },
                new { op = "Replace", path = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:employeeNumber", value = "EMP-ENTRA-12345" }
            }
        };

        _output.WriteLine($"[ENTRA-REQUEST] PATCH /scim/Users/{userId} - Enterprise extension (department, employeeNumber)");
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _output.WriteLine($"[ENTRA-RESULT] ? Enterprise extension attributes updated");
    }

    #endregion

    #region Phase 4: Group Provisioning - Entra ID creates/updates groups

    [Fact]
    public async Task EntraId_GroupProvisioning_CheckByDisplayName_ThenCreate_FullFlow()
    {
        // Step 1: Entra ID checks if group exists by displayName
        var displayName = $"Azure AD Security Group {Guid.NewGuid():N}";
        var filter = $"displayName eq \"{displayName}\"";
        
        _output.WriteLine($"[ENTRA-STEP1] Check if group exists: {displayName}");
        var checkResponse = await _client.GetAsync($"/scim/Groups?filter={Uri.EscapeDataString(filter)}");
        checkResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var checkResult = await checkResponse.Content.ReadFromJsonAsync<ScimListResponse<ScimGroup>>();
        checkResult?.TotalResults.ShouldBe(0);
        _output.WriteLine($"[ENTRA-STEP1] Group does not exist (totalResults=0)");

        // Step 2: Group doesn't exist, so create it
        var newGroup = new
        {
            schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:Group" },
            displayName = displayName,
            externalId = "azure-group-" + Guid.NewGuid().ToString("N")[..12],
            members = Array.Empty<object>()
        };

        _output.WriteLine("[ENTRA-STEP2] POST /scim/Groups - Create group");
        var createResponse = await _client.PostAsJsonAsync("/scim/Groups", newGroup);
        _output.WriteLine($"[ENTRA-STEP2] Response: {createResponse.StatusCode}");

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<ScimGroup>();
        createdGroup.ShouldNotBeNull();
        createdGroup.DisplayName.ShouldBe(displayName);
        _output.WriteLine($"[ENTRA-STEP2] ? Group created with ID: {createdGroup.Id}");
    }

    [Fact]
    public async Task EntraId_GroupProvisioning_AddMember_UserAssignedToGroup()
    {
        // Entra ID adds members to groups via PATCH when user is assigned to group
        var groupId = "group-001";
        var userId = "user-001";
        
        var patchRequest = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new[]
            {
                new 
                { 
                    op = "Add", 
                    path = "members",
                    value = new[]
                    {
                        new { value = userId, display = "John Doe" }
                    }
                }
            }
        };

        _output.WriteLine($"[ENTRA-REQUEST] PATCH /scim/Groups/{groupId} - Add member {userId}");
        var response = await _client.PatchAsJsonAsync($"/scim/Groups/{groupId}", patchRequest);
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _output.WriteLine($"[ENTRA-RESULT] ? Member added to group");
    }

    [Fact]
    public async Task EntraId_GroupProvisioning_RemoveMember_UserRemovedFromGroup()
    {
        // First add a member, then remove it (simulating full Entra ID behavior)
        var groupId = "group-002";
        var userId = "user-003";
        
        // Add member first
        var addPatch = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new[]
            {
                new { op = "Add", path = "members", value = new[] { new { value = userId } } }
            }
        };
        await _client.PatchAsJsonAsync($"/scim/Groups/{groupId}", addPatch);
        _output.WriteLine($"[ENTRA-SETUP] Added member {userId} to group {groupId}");

        // Now remove (this is what Entra ID does when user is removed from group)
        var removePatch = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new[]
            {
                new { op = "Remove", path = "members", value = new[] { new { value = userId } } }
            }
        };

        _output.WriteLine($"[ENTRA-REQUEST] PATCH /scim/Groups/{groupId} - Remove member {userId}");
        var response = await _client.PatchAsJsonAsync($"/scim/Groups/{groupId}", removePatch);
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _output.WriteLine($"[ENTRA-RESULT] ? Member removed from group");
    }

    #endregion

    #region Phase 5: Error Handling - Entra ID expects proper SCIM error responses

    [Fact]
    public async Task EntraId_ErrorHandling_GetNonExistentUser_ShouldReturn404()
    {
        var userId = "nonexistent-user-" + Guid.NewGuid().ToString("N")[..8];
        
        _output.WriteLine($"[ENTRA-REQUEST] GET /scim/Users/{userId}");
        var response = await _client.GetAsync($"/scim/Users/{userId}");
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EntraId_ErrorHandling_CreateDuplicateUser_ShouldReturn409Conflict()
    {
        // Entra ID expects 409 Conflict when trying to create a duplicate user
        var existingUser = new
        {
            schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" },
            userName = "john.doe@example.com", // Already exists in seed data
            displayName = "Duplicate User",
            active = true
        };

        _output.WriteLine("[ENTRA-REQUEST] POST /scim/Users (duplicate userName)");
        var response = await _client.PostAsJsonAsync("/scim/Users", existingUser);
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task EntraId_ErrorHandling_PatchNonExistentUser_ShouldReturn404()
    {
        var patchRequest = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new[] { new { op = "Replace", path = "active", value = false } }
        };

        var userId = "nonexistent-" + Guid.NewGuid().ToString("N")[..8];
        _output.WriteLine($"[ENTRA-REQUEST] PATCH /scim/Users/{userId}");
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        _output.WriteLine($"[ENTRA-RESPONSE] {response.StatusCode}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion

    #region Phase 6: Full Provisioning Lifecycle - Complete Entra ID cycle

    [Fact]
    public async Task EntraId_FullProvisioningCycle_CreateUpdateDisableDeleteUser()
    {
        // This test simulates the complete lifecycle of a user in Entra ID provisioning
        _output.WriteLine("=== ENTRA ID FULL PROVISIONING CYCLE ===");

        // 1. Check if user exists (should not)
        var userName = $"lifecycle.user.{Guid.NewGuid():N}@company.onmicrosoft.com";
        var filter = $"userName eq \"{userName}\"";
        
        _output.WriteLine($"[STEP 1/6] Check user existence");
        var checkResponse = await _client.GetAsync($"/scim/Users?filter={Uri.EscapeDataString(filter)}");
        checkResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var checkResult = await checkResponse.Content.ReadFromJsonAsync<ScimListResponse<ScimUser>>();
        checkResult?.TotalResults.ShouldBe(0);
        _output.WriteLine($"[STEP 1/6] ? User does not exist");

        // 2. Create user (user assigned to app in Azure AD)
        _output.WriteLine($"[STEP 2/6] Create user");
        var createUser = new
        {
            schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" },
            userName = userName,
            externalId = $"azure-{Guid.NewGuid():N}",
            displayName = "Lifecycle Test User",
            name = new { givenName = "Lifecycle", familyName = "Test" },
            active = true
        };
        var createResponse = await _client.PostAsJsonAsync("/scim/Users", createUser);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        var createdUser = await createResponse.Content.ReadFromJsonAsync<ScimUser>();
        var userId = createdUser!.Id;
        _output.WriteLine($"[STEP 2/6] ? User created: {userId}");

        // 3. Update user (profile changed in Azure AD)
        _output.WriteLine($"[STEP 3/6] Update user attributes");
        var updatePatch = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "Replace", path = "displayName", value = "Lifecycle Test User - Updated" },
                new { op = "Replace", path = "title", value = "Test Engineer" }
            }
        };
        var updateResponse = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", updatePatch);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        _output.WriteLine($"[STEP 3/6] ? User updated");

        // 4. Disable user (user unassigned from app in Azure AD)
        _output.WriteLine($"[STEP 4/6] Disable user (soft delete)");
        var disablePatch = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new[] { new { op = "Replace", path = "active", value = false } }
        };
        var disableResponse = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", disablePatch);
        disableResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        _output.WriteLine($"[STEP 4/6] ? User disabled");

        // 5. Delete user (hard delete if configured in Entra ID)
        _output.WriteLine($"[STEP 5/6] Delete user (hard delete)");
        var deleteResponse = await _client.DeleteAsync($"/scim/Users/{userId}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        _output.WriteLine($"[STEP 5/6] ? User deleted");

        // 6. Verify user no longer exists
        _output.WriteLine($"[STEP 6/6] Verify deletion");
        var verifyResponse = await _client.GetAsync($"/scim/Users/{userId}");
        verifyResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        _output.WriteLine($"[STEP 6/6] ? User confirmed deleted");

        _output.WriteLine("=== ? FULL PROVISIONING CYCLE COMPLETED ===");
    }

    #endregion
}

/// <summary>
/// Collection definition for Entra ID integration tests.
/// Uses separate PostgreSQL container for test isolation.
/// </summary>
[CollectionDefinition("EntraIdIntegration")]
public class EntraIdIntegrationCollection : ICollectionFixture<ScimWebApplicationFactory>
{
}

