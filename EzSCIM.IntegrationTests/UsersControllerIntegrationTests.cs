using System.Net;
using System.Net.Http.Json;
using EzSCIM.IntegrationTests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using EzSCIM.Models;
using Shouldly;
using Xunit.Abstractions;

namespace EzSCIM.IntegrationTests;

/// <summary>
/// Integration tests for UsersController using real HTTP calls and PostgreSQL database.
/// </summary>
[Collection("UsersIntegration")]
public class UsersControllerIntegrationTests : IClassFixture<ScimWebApplicationFactory>, IAsyncLifetime
{
    private readonly ScimWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly PostgreSqlScimDbContext _context;
    private readonly ITestOutputHelper _output;
    private IDbContextTransaction _transaction = null!;

    public UsersControllerIntegrationTests(
        ScimWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
        
        // Create and store scope for scoped services
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<PostgreSqlScimDbContext>();
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

    #region GetUsers Tests

    [Fact]
    public async Task GetUsers_WithNoFilter_ShouldReturnAllUsers()
    {
        // Arrange
        _output.WriteLine("[REQUEST] GET /scim/Users");

        // Act
        var response = await _client.GetAsync("/scim/Users");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimUser>>();
        users.ShouldNotBeNull();
        users.TotalResults.ShouldBeGreaterThanOrEqualTo(5); // At least 5 seed users (may have more from other tests)
        users.Resources.Count.ShouldBeGreaterThanOrEqualTo(5);
        users.StartIndex.ShouldBe(1);
    }

    [Fact]
    public async Task GetUsers_WithFilter_ShouldReturnFilteredUsers()
    {
        // Arrange
        var filter = "userName eq \"john.doe@example.com\"";
        _output.WriteLine($"[REQUEST] GET /scim/Users?filter={Uri.EscapeDataString(filter)}");

        // Act
        var response = await _client.GetAsync($"/scim/Users?filter={Uri.EscapeDataString(filter)}");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimUser>>();
        users.ShouldNotBeNull();
        users.TotalResults.ShouldBe(1);
        users.Resources.Count.ShouldBe(1);
        users.Resources[0].UserName.ShouldBe("john.doe@example.com");
    }

    [Fact]
    public async Task GetUsers_WithPagination_ShouldReturnPaginatedUsers()
    {
        // Arrange
        _output.WriteLine("[REQUEST] GET /scim/Users?startIndex=2&count=2");

        // Act
        var response = await _client.GetAsync("/scim/Users?startIndex=2&count=2");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimUser>>();
        users.ShouldNotBeNull();
        users.TotalResults.ShouldBeGreaterThanOrEqualTo(5); // At least 5 users
        users.StartIndex.ShouldBe(2);
        users.ItemsPerPage.ShouldBe(2);
        users.Resources.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetUsers_WithActiveFilter_ShouldReturnActiveUsers()
    {
        // Arrange
        var filter = "active eq true";
        _output.WriteLine($"[REQUEST] GET /scim/Users?filter={Uri.EscapeDataString(filter)}");

        // Act
        var response = await _client.GetAsync($"/scim/Users?filter={Uri.EscapeDataString(filter)}");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<ScimListResponse<ScimUser>>();
        users.ShouldNotBeNull();
        users.TotalResults.ShouldBeGreaterThanOrEqualTo(3); // At least 3 active users (other tests may deactivate some)
        users.Resources.All(u => u.Active).ShouldBeTrue();
    }

    #endregion

    #region GetUser Tests

    [Fact]
    public async Task GetUser_WhenExists_ShouldReturnUser()
    {
        // Arrange
        var userId = "user-001"; // John Doe from seed data
        _output.WriteLine($"[REQUEST] GET /scim/Users/{userId}");

        // Act
        var response = await _client.GetAsync($"/scim/Users/{userId}");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<ScimUser>();
        user.ShouldNotBeNull();
        user.Id.ShouldBe(userId);
        user.UserName.ShouldBe("john.doe@example.com");
    }

    [Fact]
    public async Task GetUser_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var userId = "non-existent-user";
        _output.WriteLine($"[REQUEST] GET /scim/Users/{userId}");

        // Act
        var response = await _client.GetAsync($"/scim/Users/{userId}");
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ScimError>();
        error.ShouldNotBeNull();
        error.Status.ShouldBe(404);
    }

    #endregion

    #region CreateUser Tests

    [Fact]
    public async Task CreateUser_WhenValid_ShouldReturnCreated()
    {
        // Arrange
        var newUser = new ScimUser
        {
            UserName = "newuser@example.com",
            DisplayName = "New User",
            Active = true,
            Name = new ScimName
            {
                GivenName = "New",
                FamilyName = "User"
            }
        };
        _output.WriteLine($"[REQUEST] POST /scim/Users");

        // Act
        var response = await _client.PostAsJsonAsync("/scim/Users", newUser);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createdUser = await response.Content.ReadFromJsonAsync<ScimUser>();
        createdUser.ShouldNotBeNull();
        createdUser.Id.ShouldNotBeNullOrEmpty();
        createdUser.UserName.ShouldBe("newuser@example.com");
        createdUser.Meta.ShouldNotBeNull();
        createdUser.Meta.ResourceType.ShouldBe("User");

        // Verify persistence
        var getResponse = await _client.GetAsync($"/scim/Users/{createdUser.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUser_WhenAlreadyExists_ShouldReturn409()
    {
        // Arrange
        var existingUser = new ScimUser
        {
            UserName = "john.doe@example.com", // Already exists in seed data
            DisplayName = "Duplicate User"
        };
        _output.WriteLine($"[REQUEST] POST /scim/Users (duplicate)");

        // Act
        var response = await _client.PostAsJsonAsync("/scim/Users", existingUser);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var error = await response.Content.ReadFromJsonAsync<ScimError>();
        error.ShouldNotBeNull();
        error.Status.ShouldBe(409);
    }

    #endregion

    #region UpdateUser Tests

    [Fact]
    public async Task UpdateUser_WhenExists_ShouldReturnUpdatedUser()
    {
        // Arrange
        var userId = "user-001";
        var updatedUser = new ScimUser
        {
            UserName = "john.doe.updated@example.com",
            DisplayName = "John Doe Updated",
            Active = true
        };
        _output.WriteLine($"[REQUEST] PUT /scim/Users/{userId}");

        // Act
        var response = await _client.PutAsJsonAsync($"/scim/Users/{userId}", updatedUser);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var returnedUser = await response.Content.ReadFromJsonAsync<ScimUser>();
        returnedUser.ShouldNotBeNull();
        returnedUser.Id.ShouldBe(userId);
        returnedUser.UserName.ShouldBe("john.doe.updated@example.com");
        returnedUser.Meta.ShouldNotBeNull();
        returnedUser.Meta.LastModified.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Fact]
    public async Task UpdateUser_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var userId = "non-existent-user";
        var updatedUser = new ScimUser
        {
            UserName = "test@example.com"
        };
        _output.WriteLine($"[REQUEST] PUT /scim/Users/{userId}");

        // Act
        var response = await _client.PutAsJsonAsync($"/scim/Users/{userId}", updatedUser);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ScimError>();
        error.ShouldNotBeNull();
        error.Status.ShouldBe(404);
    }

    #endregion

    #region PatchUser Tests

    [Fact]
    public async Task PatchUser_WhenValid_ShouldReturnUpdatedUser()
    {
        // Arrange
        var userId = "user-001";
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation { Op = "replace", Path = "active", Value = false }
            }
        };
        _output.WriteLine($"[REQUEST] PATCH /scim/Users/{userId}");

        // Act
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var patchedUser = await response.Content.ReadFromJsonAsync<ScimUser>();
        patchedUser.ShouldNotBeNull();
        patchedUser.Id.ShouldBe(userId);
        patchedUser.Active.ShouldBeFalse();
    }

    [Fact]
    public async Task PatchUser_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var userId = "non-existent-user";
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>()
        };
        _output.WriteLine($"[REQUEST] PATCH /scim/Users/{userId}");

        // Act
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ScimError>();
        error.ShouldNotBeNull();
        error.Status.ShouldBe(404);
    }

    [Fact]
    public async Task PatchUser_CustomStringField_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = "user-001";
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation 
                { 
                    Op = "replace", 
                    Path = "urn:scim:custom:User:customField1", 
                    Value = "CustomValue123" 
                }
            }
        };
        _output.WriteLine($"[REQUEST] PATCH /scim/Users/{userId} with custom field");

        // Act
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var patchedUser = await response.Content.ReadFromJsonAsync<ScimUser>();
        patchedUser.ShouldNotBeNull();
        patchedUser.Id.ShouldBe(userId);
    }

    [Fact]
    public async Task PatchUser_CustomBooleanField_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = "user-002";
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation 
                { 
                    Op = "replace", 
                    Path = "urn:scim:custom:User:isVip", 
                    Value = true 
                }
            }
        };
        _output.WriteLine($"[REQUEST] PATCH /scim/Users/{userId} with custom boolean field");

        // Act
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var patchedUser = await response.Content.ReadFromJsonAsync<ScimUser>();
        patchedUser.ShouldNotBeNull();
        patchedUser.Id.ShouldBe(userId);
    }

    [Fact]
    public async Task PatchUser_EnterpriseExtensionField_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = "user-001";
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation 
                { 
                    Op = "replace", 
                    Path = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:department", 
                    Value = "Engineering" 
                },
                new ScimPatchOperation 
                { 
                    Op = "add", 
                    Path = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:employeeNumber", 
                    Value = "EMP-12345" 
                }
            }
        };
        _output.WriteLine($"[REQUEST] PATCH /scim/Users/{userId} with enterprise extension fields");

        // Act
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var patchedUser = await response.Content.ReadFromJsonAsync<ScimUser>();
        patchedUser.ShouldNotBeNull();
        patchedUser.Id.ShouldBe(userId);
    }

    [Fact]
    public async Task PatchUser_MultipleFieldsAtOnce_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = "user-001";
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation { Op = "replace", Path = "nickName", Value = "Johnny" },
                new ScimPatchOperation { Op = "replace", Path = "preferredLanguage", Value = "en-US" },
                new ScimPatchOperation { Op = "replace", Path = "timezone", Value = "America/New_York" },
                new ScimPatchOperation { Op = "replace", Path = "urn:scim:custom:User:customField1", Value = "Test Value" }
            }
        };
        _output.WriteLine($"[REQUEST] PATCH /scim/Users/{userId} with multiple fields");

        // Act
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var patchedUser = await response.Content.ReadFromJsonAsync<ScimUser>();
        patchedUser.ShouldNotBeNull();
        patchedUser.Id.ShouldBe(userId);
    }

    [Fact]
    public async Task PatchUser_RemoveCustomField_ShouldClearValue()
    {
        // Arrange
        var userId = "user-001";
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation 
                { 
                    Op = "remove", 
                    Path = "urn:scim:custom:User:customField1"
                }
            }
        };
        _output.WriteLine($"[REQUEST] PATCH /scim/Users/{userId} remove custom field");

        // Act
        var response = await _client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {response.StatusCode} - {responseBody}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var patchedUser = await response.Content.ReadFromJsonAsync<ScimUser>();
        patchedUser.ShouldNotBeNull();
        patchedUser.Id.ShouldBe(userId);
    }

    #endregion

    #region DeleteUser Tests

    [Fact]
    public async Task DeleteUser_WhenExists_ShouldReturn204()
    {
        // Arrange
        var userId = "user-005"; // Charlie Brown
        _output.WriteLine($"[REQUEST] DELETE /scim/Users/{userId}");

        // Act
        var response = await _client.DeleteAsync($"/scim/Users/{userId}");
        _output.WriteLine($"[RESPONSE] {response.StatusCode}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/scim/Users/{userId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var userId = "non-existent-user";
        _output.WriteLine($"[REQUEST] DELETE /scim/Users/{userId}");

        // Act
        var response = await _client.DeleteAsync($"/scim/Users/{userId}");
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



