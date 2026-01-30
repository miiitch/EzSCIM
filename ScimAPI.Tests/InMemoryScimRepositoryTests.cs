using Shouldly;
using ScimAPI.Models;
using ScimAPI.Repositories;
using Xunit;

namespace ScimAPI.Tests;

public class InMemoryScimRepositoryTests
{
    private readonly InMemoryScimRepository _repository = new();

    #region User CRUD Tests

    [Fact]
    public async Task CreateUser_ShouldGenerateIdAndMeta()
    {
        // Arrange
        var user = new ScimUser
        {
            UserName = "john.doe@example.com",
            Name = new ScimName { GivenName = "John", FamilyName = "Doe" },
            Active = true
        };

        // Act
        var result = await _repository.CreateUserAsync(user);

        // Assert
        result.Id.ShouldNotBeNullOrEmpty();
        result.Meta.ShouldNotBeNull();
        result.Meta.ResourceType.ShouldBe("User");
        result.Meta.Created.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task GetUser_WhenExists_ShouldReturnUser()
    {
        // Arrange
        var user = await _repository.CreateUserAsync(new ScimUser
        {
            UserName = "test@example.com",
            Active = true
        });

        // Act
        var result = await _repository.GetUserAsync(user.Id);

        // Assert
        result.ShouldNotBeNull();
        result.UserName.ShouldBe("test@example.com");
    }

    [Fact]
    public async Task GetUser_WhenNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetUserAsync("non-existent-id");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateUser_ShouldUpdateAllFields()
    {
        // Arrange
        var user = await _repository.CreateUserAsync(new ScimUser
        {
            UserName = "original@example.com",
            Active = true
        });

        var updatedUser = new ScimUser
        {
            UserName = "updated@example.com",
            DisplayName = "Updated Name",
            Active = false
        };

        // Act
        var result = await _repository.UpdateUserAsync(user.Id, updatedUser);

        // Assert
        result.ShouldNotBeNull();
        result.UserName.ShouldBe("updated@example.com");
        result.DisplayName.ShouldBe("Updated Name");
        result.Active.ShouldBeFalse();
        result.Meta.LastModified.ShouldBeGreaterThan(result.Meta.Created);
    }

    [Fact]
    public async Task DeleteUser_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        var user = await _repository.CreateUserAsync(new ScimUser { UserName = "delete@example.com" });

        // Act
        var result = await _repository.DeleteUserAsync(user.Id);

        // Assert
        result.ShouldBeTrue();
        var deletedUser = await _repository.GetUserAsync(user.Id);
        deletedUser.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteUser_WhenNotExists_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteUserAsync("non-existent-id");

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region User Filter Tests

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

    [Fact]
    public async Task GetUsers_FilterByUserName_Sw_ShouldReturnMatchingUsers()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser { UserName = "john.doe@example.com" });
        await _repository.CreateUserAsync(new ScimUser { UserName = "john.smith@example.com" });
        await _repository.CreateUserAsync(new ScimUser { UserName = "jane.doe@example.com" });

        // Act
        var result = await _repository.GetUsersAsync(filter: "userName sw \"john\"");

        // Assert
        result.TotalResults.ShouldBe(2);
        result.Resources.All(u => u.UserName.StartsWith("john")).ShouldBeTrue();
    }

    [Fact]
    public async Task GetUsers_FilterByUserName_Co_ShouldReturnMatchingUsers()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser { UserName = "john.doe@example.com" });
        await _repository.CreateUserAsync(new ScimUser { UserName = "jane.doe@example.com" });
        await _repository.CreateUserAsync(new ScimUser { UserName = "bob.smith@example.com" });

        // Act
        var result = await _repository.GetUsersAsync(filter: "userName co \"doe\"");

        // Assert
        result.TotalResults.ShouldBe(2);
        result.Resources.All(u => u.UserName.Contains("doe")).ShouldBeTrue();
    }

    [Fact]
    public async Task GetUsers_FilterByActive_ShouldReturnMatchingUsers()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser { UserName = "active@example.com", Active = true });
        await _repository.CreateUserAsync(new ScimUser { UserName = "inactive@example.com", Active = false });

        // Act
        var result = await _repository.GetUsersAsync(filter: "active eq true");

        // Assert
        result.TotalResults.ShouldBe(1);
        result.Resources.First().Active.ShouldBeTrue();
    }

    [Fact]
    public async Task GetUsers_FilterByDisplayName_Co_ShouldReturnMatchingUsers()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser { UserName = "user1", DisplayName = "Admin User" });
        await _repository.CreateUserAsync(new ScimUser { UserName = "user2", DisplayName = "Regular User" });
        await _repository.CreateUserAsync(new ScimUser { UserName = "user3", DisplayName = "Guest" });

        // Act
        var result = await _repository.GetUsersAsync(filter: "displayName co \"User\"");

        // Assert
        result.TotalResults.ShouldBe(2);
    }

    [Fact]
    public async Task GetUsers_FilterByGivenName_ShouldReturnMatchingUsers()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser
        {
            UserName = "john@example.com",
            Name = new ScimName { GivenName = "John", FamilyName = "Doe" }
        });
        await _repository.CreateUserAsync(new ScimUser
        {
            UserName = "jane@example.com",
            Name = new ScimName { GivenName = "Jane", FamilyName = "Smith" }
        });

        // Act
        var result = await _repository.GetUsersAsync(filter: "name.givenName eq \"John\"");

        // Assert
        result.TotalResults.ShouldBe(1);
        result.Resources.First().Name.GivenName.ShouldBe("John");
    }

    [Fact]
    public async Task GetUsers_FilterByFamilyName_ShouldReturnMatchingUsers()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser
        {
            UserName = "john@example.com",
            Name = new ScimName { GivenName = "John", FamilyName = "Doe" }
        });
        await _repository.CreateUserAsync(new ScimUser
        {
            UserName = "jane@example.com",
            Name = new ScimName { GivenName = "Jane", FamilyName = "Doe" }
        });

        // Act
        var result = await _repository.GetUsersAsync(filter: "name.familyName eq \"Doe\"");

        // Assert
        result.TotalResults.ShouldBe(2);
        result.Resources.All(u => u.Name.FamilyName == "Doe").ShouldBeTrue();
    }

    [Fact]
    public async Task GetUsers_FilterWithAnd_ShouldReturnMatchingUsers()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser { UserName = "john@example.com", Active = true });
        await _repository.CreateUserAsync(new ScimUser { UserName = "john.smith@example.com", Active = false });
        await _repository.CreateUserAsync(new ScimUser { UserName = "jane@example.com", Active = true });

        // Act
        var result = await _repository.GetUsersAsync(filter: "userName sw \"john\" and active eq true");

        // Assert
        result.TotalResults.ShouldBe(1);
        result.Resources.First().UserName.ShouldBe("john@example.com");
    }

    [Fact]
    public async Task GetUsers_FilterWithOr_ShouldReturnMatchingUsers()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser { UserName = "john@example.com" });
        await _repository.CreateUserAsync(new ScimUser { UserName = "jane@example.com" });
        await _repository.CreateUserAsync(new ScimUser { UserName = "bob@example.com" });

        // Act
        var result = await _repository.GetUsersAsync(filter: "userName eq \"john@example.com\" or userName eq \"jane@example.com\"");

        // Assert
        result.TotalResults.ShouldBe(2);
    }

    [Fact]
    public async Task GetUsers_FilterWithNot_ShouldReturnMatchingUsers()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser { UserName = "active@example.com", Active = true });
        await _repository.CreateUserAsync(new ScimUser { UserName = "inactive@example.com", Active = false });

        // Act
        var result = await _repository.GetUsersAsync(filter: "not (active eq false)");

        // Assert
        result.TotalResults.ShouldBe(1);
        result.Resources.First().Active.ShouldBeTrue();
    }

    [Fact]
    public async Task GetUsers_FilterWithComplexExpression_ShouldReturnMatchingUsers()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser { UserName = "john@example.com", Active = true });
        await _repository.CreateUserAsync(new ScimUser { UserName = "jane@example.com", Active = true });
        await _repository.CreateUserAsync(new ScimUser { UserName = "bob@example.com", Active = false });

        // Act
        var result = await _repository.GetUsersAsync(filter: "(userName sw \"john\" or userName sw \"jane\") and active eq true");

        // Assert
        result.TotalResults.ShouldBe(2);
    }

    [Fact]
    public async Task GetUsers_FilterByPresent_ShouldReturnUsersWithAttribute()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser { UserName = "user1@example.com", DisplayName = "User One" });
        await _repository.CreateUserAsync(new ScimUser { UserName = "user2@example.com", DisplayName = null });

        // Act
        var result = await _repository.GetUsersAsync(filter: "displayName pr");

        // Assert
        result.TotalResults.ShouldBe(1);
        result.Resources.First().DisplayName.ShouldBe("User One");
    }

    #endregion

    #region User Pagination Tests

    [Fact]
    public async Task GetUsers_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            await _repository.CreateUserAsync(new ScimUser { UserName = $"user{i}@example.com" });
        }

        // Act
        var result = await _repository.GetUsersAsync(startIndex: 1, count: 5);

        // Assert
        result.TotalResults.ShouldBe(10);
        result.ItemsPerPage.ShouldBe(5);
        result.StartIndex.ShouldBe(1);
        result.Resources.Count.ShouldBe(5);
    }

    [Fact]
    public async Task GetUsers_WithPaginationSecondPage_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            await _repository.CreateUserAsync(new ScimUser { UserName = $"user{i}@example.com" });
        }

        // Act
        var result = await _repository.GetUsersAsync(startIndex: 6, count: 5);

        // Assert
        result.TotalResults.ShouldBe(10);
        result.ItemsPerPage.ShouldBe(5);
        result.StartIndex.ShouldBe(6);
        result.Resources.Count.ShouldBe(5);
    }

    #endregion

    #region User Patch Tests

    [Fact]
    public async Task PatchUser_ReplaceActive_ShouldUpdateActive()
    {
        // Arrange
        var user = await _repository.CreateUserAsync(new ScimUser
        {
            UserName = "test@example.com",
            Active = true
        });

        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation { Op = "replace", Path = "active", Value = false }
            }
        };

        // Act
        var result = await _repository.PatchUserAsync(user.Id, patchRequest);

        // Assert
        result.ShouldNotBeNull();
        result!.Active.ShouldBeFalse();
    }

    [Fact]
    public async Task PatchUser_ReplaceDisplayName_ShouldUpdateDisplayName()
    {
        // Arrange
        var user = await _repository.CreateUserAsync(new ScimUser
        {
            UserName = "test@example.com",
            DisplayName = "Original Name"
        });

        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation { Op = "replace", Path = "displayName", Value = "New Name" }
            }
        };

        // Act
        var result = await _repository.PatchUserAsync(user.Id, patchRequest);

        // Assert
        result.ShouldNotBeNull();
        result!.DisplayName.ShouldBe("New Name");
    }

    [Fact]
    public async Task PatchUser_ReplaceGivenName_ShouldUpdateGivenName()
    {
        // Arrange
        var user = await _repository.CreateUserAsync(new ScimUser
        {
            UserName = "test@example.com",
            Name = new ScimName { GivenName = "John", FamilyName = "Doe" }
        });

        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation { Op = "replace", Path = "name.givenName", Value = "Johnny" }
            }
        };

        // Act
        var result = await _repository.PatchUserAsync(user.Id, patchRequest);

        // Assert
        result.ShouldNotBeNull();
        result!.Name.GivenName.ShouldBe("Johnny");
        result.Name.FamilyName.ShouldBe("Doe"); // Should remain unchanged
    }

    [Fact]
    public async Task PatchUser_MultipleOperations_ShouldApplyAll()
    {
        // Arrange
        var user = await _repository.CreateUserAsync(new ScimUser
        {
            UserName = "test@example.com",
            Active = true,
            DisplayName = "Original"
        });

        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation { Op = "replace", Path = "active", Value = false },
                new ScimPatchOperation { Op = "replace", Path = "displayName", Value = "Updated" }
            }
        };

        // Act
        var result = await _repository.PatchUserAsync(user.Id, patchRequest);

        // Assert
        result.ShouldNotBeNull();
        result!.Active.ShouldBeFalse();
        result.DisplayName.ShouldBe("Updated");
    }

    #endregion

    #region Group CRUD Tests

    [Fact]
    public async Task CreateGroup_ShouldGenerateIdAndMeta()
    {
        // Arrange
        var group = new ScimGroup
        {
            DisplayName = "Administrators"
        };

        // Act
        var result = await _repository.CreateGroupAsync(group);

        // Assert
        result.Id.ShouldNotBeNullOrEmpty();
        result.Meta.ShouldNotBeNull();
        result.Meta.ResourceType.ShouldBe("Group");
    }

    [Fact]
    public async Task GetGroup_WhenExists_ShouldReturnGroup()
    {
        // Arrange
        var group = await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Test Group" });

        // Act
        var result = await _repository.GetGroupAsync(group.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.DisplayName.ShouldBe("Test Group");
    }

    [Fact]
    public async Task UpdateGroup_ShouldUpdateDisplayName()
    {
        // Arrange
        var group = await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Original Name" });
        var updatedGroup = new ScimGroup { DisplayName = "Updated Name" };

        // Act
        var result = await _repository.UpdateGroupAsync(group.Id, updatedGroup);

        // Assert
        result.ShouldNotBeNull();
        result!.DisplayName.ShouldBe("Updated Name");
    }

    [Fact]
    public async Task DeleteGroup_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        var group = await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Delete Me" });

        // Act
        var result = await _repository.DeleteGroupAsync(group.Id);

        // Assert
        result.ShouldBeTrue();
        var deletedGroup = await _repository.GetGroupAsync(group.Id);
        deletedGroup.ShouldBeNull();
    }

    #endregion

    #region Group Filter Tests

    [Fact]
    public async Task GetGroups_FilterByDisplayName_Eq_ShouldReturnMatchingGroup()
    {
        // Arrange
        await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Administrators" });
        await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Developers" });

        // Act
        var result = await _repository.GetGroupsAsync(filter: "displayName eq \"Administrators\"");

        // Assert
        result.TotalResults.ShouldBe(1);
        result.Resources.First().DisplayName.ShouldBe("Administrators");
    }

    [Fact]
    public async Task GetGroups_FilterByDisplayName_Co_ShouldReturnMatchingGroups()
    {
        // Arrange
        await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Dev Team" });
        await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Dev Managers" });
        await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "QA Team" });

        // Act
        var result = await _repository.GetGroupsAsync(filter: "displayName co \"Dev\"");

        // Assert
        result.TotalResults.ShouldBe(2);
    }

    [Fact]
    public async Task GetGroups_FilterByDisplayName_Sw_ShouldReturnMatchingGroups()
    {
        // Arrange
        await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Test Group 1" });
        await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Test Group 2" });
        await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Production Group" });

        // Act
        var result = await _repository.GetGroupsAsync(filter: "displayName sw \"Test\"");

        // Assert
        result.TotalResults.ShouldBe(2);
    }

    #endregion

    #region Group Patch Tests

    [Fact]
    public async Task PatchGroup_AddMember_ShouldAddMemberToGroup()
    {
        // Arrange
        var user = await _repository.CreateUserAsync(new ScimUser { UserName = "user@example.com" });
        var group = await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Test Group" });

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
                        new Dictionary<string, string> { { "value", user.Id }, { "display", user.UserName } }
                    }
                }
            }
        };

        // Act
        var result = await _repository.PatchGroupAsync(group.Id, patchRequest);

        // Assert
        result.ShouldNotBeNull();
        result!.Members.Count.ShouldBe(1);
        result.Members.First().Value.ShouldBe(user.Id);
    }

    [Fact]
    public async Task PatchGroup_RemoveMember_ShouldRemoveMemberFromGroup()
    {
        // Arrange
        var user = await _repository.CreateUserAsync(new ScimUser { UserName = "user@example.com" });
        var group = await _repository.CreateGroupAsync(new ScimGroup
        {
            DisplayName = "Test Group",
            Members = new List<ScimMember>
            {
                new ScimMember { Value = user.Id, Display = user.UserName }
            }
        });

        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation
                {
                    Op = "remove",
                    Path = $"members[value eq \"{user.Id}\"]"
                }
            }
        };

        // Act
        var result = await _repository.PatchGroupAsync(group.Id, patchRequest);

        // Assert
        result.ShouldNotBeNull();
        result!.Members.ShouldBeEmpty();
    }

    #endregion

    #region Schema Tests

    [Fact]
    public async Task AddCustomSchema_ShouldStoreSchema()
    {
        // Arrange
        var schema = new ScimSchema
        {
            Id = "urn:ietf:params:scim:schemas:extension:custom:2.0:User",
            Name = "CustomUser",
            Description = "Custom User Extension",
            Attributes = new List<ScimSchemaAttribute>
            {
                new ScimSchemaAttribute { Name = "customField", Type = "string" }
            }
        };

        // Act
        await _repository.AddCustomSchemaAsync(schema);
        var schemas = await _repository.GetCustomSchemasAsync();

        // Assert
        schemas.Count.ShouldBe(1);
        schemas.First().Id.ShouldBe(schema.Id);
    }

    [Fact]
    public async Task GetCustomSchemas_WhenEmpty_ShouldReturnEmptyList()
    {
        // Act
        var schemas = await _repository.GetCustomSchemasAsync();

        // Assert
        schemas.ShouldBeEmpty();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task GetUserByUserName_WhenNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetUserByUserNameAsync("nonexistent@example.com");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetUserByUserName_CaseInsensitive_ShouldReturnUser()
    {
        // Arrange
        await _repository.CreateUserAsync(new ScimUser { UserName = "Test@Example.com" });

        // Act
        var result = await _repository.GetUserByUserNameAsync("test@example.com");

        // Assert
        result.ShouldNotBeNull();
        result!.UserName.ShouldBe("Test@Example.com");
    }

    [Fact]
    public async Task GetGroupByDisplayName_WhenNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetGroupByDisplayNameAsync("Non Existent Group");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetGroupByDisplayName_CaseInsensitive_ShouldReturnGroup()
    {
        // Arrange
        await _repository.CreateGroupAsync(new ScimGroup { DisplayName = "Test Group" });

        // Act
        var result = await _repository.GetGroupByDisplayNameAsync("test group");

        // Assert
        result.ShouldNotBeNull();
        result!.DisplayName.ShouldBe("Test Group");
    }

    [Fact]
    public async Task GetUsers_WithNoResults_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetUsersAsync(filter: "userName eq \"nonexistent@example.com\"");

        // Assert
        result.TotalResults.ShouldBe(0);
        result.Resources.ShouldBeEmpty();
    }

    [Fact]
    public async Task PatchUser_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation { Op = "replace", Path = "active", Value = false }
            }
        };

        // Act
        var result = await _repository.PatchUserAsync("non-existent-id", patchRequest);

        // Assert
        result.ShouldBeNull();
    }

    #endregion
}


