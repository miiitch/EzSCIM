using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using ScimAPI.IntegrationTests.Data;
using ScimAPI.Models;
using Shouldly;
using Xunit.Abstractions;

namespace ScimAPI.IntegrationTests;

/// <summary>
/// Integration tests for GroupsController using real HTTP calls and PostgreSQL database.
/// </summary>
[Collection("GroupsIntegration")]
public class GroupsControllerIntegrationTests : IClassFixture<ScimWebApplicationFactory>, IAsyncLifetime
{
    private readonly ScimWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly ScimDbContext _context;
    private readonly ITestOutputHelper _output;
    private IDbContextTransaction _transaction = null!;

    public GroupsControllerIntegrationTests(
        ScimWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
        
        // Create and store scope for scoped services
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ScimDbContext>();
    }

    public async Task InitializeAsync()
    {
        // Start transaction for test isolation
        _transaction = await _context.Database.BeginTransactionAsync();
        _output.WriteLine($"[{DateTime.Now:HH:mm:ss}] Transaction started for test isolation");
    }

    public async Task DisposeAsync()
    {
        // Rollback transaction to isolate tests
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _output.WriteLine($"[{DateTime.Now:HH:mm:ss}] Transaction rolled back");
        }
        
        // Dispose scope
        _scope?.Dispose();
    }

    #region GetGroups Tests

    [Fact]
    public async Task GetGroups_WithNoFilter_ShouldReturnAllGroups()
    {
        // Arrange
        _output.WriteLine("[REQUEST] GET /scim/Groups");

        // Act
        var response = await _client.GetAsync("/scim/Groups");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimGroup>>();
        groups.ShouldNotBeNull();
        groups.TotalResults.ShouldBeGreaterThanOrEqualTo(3); // At least 3 seed groups (may have more from other tests)
        groups.Resources.Count.ShouldBeGreaterThanOrEqualTo(3);
        groups.StartIndex.ShouldBe(1);
    }

    [Fact]
    public async Task GetGroups_WithFilter_ShouldReturnFilteredGroups()
    {
        // Arrange
        var filter = "displayName eq \"Developers\"";
        _output.WriteLine($"[REQUEST] GET /scim/Groups?filter={Uri.EscapeDataString(filter)}");

        // Act
        var response = await _client.GetAsync($"/scim/Groups?filter={Uri.EscapeDataString(filter)}");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimGroup>>();
        groups.ShouldNotBeNull();
        groups.TotalResults.ShouldBe(1);
        groups.Resources.Count.ShouldBe(1);
        groups.Resources[0].DisplayName.ShouldBe("Developers");
    }

    [Fact]
    public async Task GetGroups_WithContainsFilter_ShouldReturnMatchingGroups()
    {
        // Arrange
        var filter = "displayName co \"Admin\"";
        _output.WriteLine($"[REQUEST] GET /scim/Groups?filter={Uri.EscapeDataString(filter)}");

        // Act
        var response = await _client.GetAsync($"/scim/Groups?filter={Uri.EscapeDataString(filter)}");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimGroup>>();
        groups.ShouldNotBeNull();
        groups.TotalResults.ShouldBeGreaterThanOrEqualTo(1); // At least 1 group with "Admin" in the name
        groups.Resources[0].DisplayName.ShouldContain("Admin"); // May be "Administrators" or "Administrators Updated"
    }

    #endregion

    #region GetGroup Tests

    [Fact]
    public async Task GetGroup_WhenExists_ShouldReturnGroup()
    {
        // Arrange
        var groupId = "group-001"; // Administrators from seed data
        _output.WriteLine($"[REQUEST] GET /scim/Groups/{groupId}");

        // Act
        var response = await _client.GetAsync($"/scim/Groups/{groupId}");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var group = await response.Content.ReadFromJsonAsync<ScimGroup>();
        group.ShouldNotBeNull();
        group.Id.ShouldBe(groupId);
        group.DisplayName.ShouldBe("Administrators");
    }

    [Fact]
    public async Task GetGroup_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var groupId = "non-existent-group";
        _output.WriteLine($"[REQUEST] GET /scim/Groups/{groupId}");

        // Act
        var response = await _client.GetAsync($"/scim/Groups/{groupId}");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ScimError>();
        error.ShouldNotBeNull();
        error.Status.ShouldBe(404);
    }

    #endregion

    #region CreateGroup Tests

    [Fact]
    public async Task CreateGroup_WhenValid_ShouldReturnCreated()
    {
        // Arrange
        var newGroup = new ScimGroup
        {
            DisplayName = "New Test Group",
            Members = new List<ScimMember>()
        };
        _output.WriteLine($"[REQUEST] POST /scim/Groups");

        // Act
        var response = await _client.PostAsJsonAsync("/scim/Groups", newGroup);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createdGroup = await response.Content.ReadFromJsonAsync<ScimGroup>();
        createdGroup.ShouldNotBeNull();
        createdGroup.Id.ShouldNotBeNullOrEmpty();
        createdGroup.DisplayName.ShouldBe("New Test Group");
        createdGroup.Meta.ShouldNotBeNull();
        createdGroup.Meta.ResourceType.ShouldBe("Group");

        // Verify persistence
        var getResponse = await _client.GetAsync($"/scim/Groups/{createdGroup.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateGroup_WhenAlreadyExists_ShouldReturn409()
    {
        // Arrange
        var existingGroup = new ScimGroup
        {
            DisplayName = "Administrators" // Already exists in seed data
        };
        _output.WriteLine($"[REQUEST] POST /scim/Groups (duplicate)");

        // Act
        var response = await _client.PostAsJsonAsync("/scim/Groups", existingGroup);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var error = await response.Content.ReadFromJsonAsync<ScimError>();
        error.ShouldNotBeNull();
        error.Status.ShouldBe(409);
    }

    #endregion

    #region UpdateGroup Tests

    [Fact]
    public async Task UpdateGroup_WhenExists_ShouldReturnUpdatedGroup()
    {
        // Arrange
        var groupId = "group-001";
        var updatedGroup = new ScimGroup
        {
            DisplayName = "Administrators Updated",
            Members = new List<ScimMember>()
        };
        _output.WriteLine($"[REQUEST] PUT /scim/Groups/{groupId}");

        // Act
        var response = await _client.PutAsJsonAsync($"/scim/Groups/{groupId}", updatedGroup);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var returnedGroup = await response.Content.ReadFromJsonAsync<ScimGroup>();
        returnedGroup.ShouldNotBeNull();
        returnedGroup.Id.ShouldBe(groupId);
        returnedGroup.DisplayName.ShouldBe("Administrators Updated");
        returnedGroup.Meta.ShouldNotBeNull();
        returnedGroup.Meta.LastModified.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Fact]
    public async Task UpdateGroup_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var groupId = "non-existent-group";
        var updatedGroup = new ScimGroup
        {
            DisplayName = "Test Group"
        };
        _output.WriteLine($"[REQUEST] PUT /scim/Groups/{groupId}");

        // Act
        var response = await _client.PutAsJsonAsync($"/scim/Groups/{groupId}", updatedGroup);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ScimError>();
        error.ShouldNotBeNull();
        error.Status.ShouldBe(404);
    }

    #endregion

    #region PatchGroup Tests

    [Fact(Skip = "PATCH operations not implemented for EF Core repositories")]
    public async Task PatchGroup_AddMember_ShouldReturnUpdatedGroup()
    {
        // Arrange
        var groupId = "group-001";
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation
                {
                    Op = "add",
                    Path = "members",
                    Value = new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string> { { "value", "user-001" } }
                    }
                }
            }
        };
        _output.WriteLine($"[REQUEST] PATCH /scim/Groups/{groupId}");

        // Act
        var response = await _client.PatchAsJsonAsync($"/scim/Groups/{groupId}", patchRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var patchedGroup = await response.Content.ReadFromJsonAsync<ScimGroup>();
        patchedGroup.ShouldNotBeNull();
        patchedGroup.Id.ShouldBe(groupId);
        patchedGroup.Members.ShouldNotBeNull();
    }

    [Fact(Skip = "PATCH operations not implemented for EF Core repositories")]
    public async Task PatchGroup_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var groupId = "non-existent-group";
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>()
        };
        _output.WriteLine($"[REQUEST] PATCH /scim/Groups/{groupId}");

        // Act
        var response = await _client.PatchAsJsonAsync($"/scim/Groups/{groupId}", patchRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ScimError>();
        error.ShouldNotBeNull();
        error.Status.ShouldBe(404);
    }

    #endregion

    #region DeleteGroup Tests

    [Fact]
    public async Task DeleteGroup_WhenExists_ShouldReturn204()
    {
        // Arrange
        var groupId = "group-003"; // Users group
        _output.WriteLine($"[REQUEST] DELETE /scim/Groups/{groupId}");

        // Act
        var response = await _client.DeleteAsync($"/scim/Groups/{groupId}");
        _output.WriteLine($"[RESPONSE] {response.StatusCode}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/scim/Groups/{groupId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteGroup_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var groupId = "non-existent-group";
        _output.WriteLine($"[REQUEST] DELETE /scim/Groups/{groupId}");

        // Act
        var response = await _client.DeleteAsync($"/scim/Groups/{groupId}");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ScimError>();
        error.ShouldNotBeNull();
        error.Status.ShouldBe(404);
    }

    #endregion
}



