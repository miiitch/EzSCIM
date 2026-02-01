using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ScimAPI.Controllers;
using ScimAPI.Filtering.AST;
using ScimAPI.Models;
using ScimAPI.Repositories;
using Shouldly;

namespace ScimAPI.Tests;

public class GroupsControllerTests
{
    private readonly Mock<IScimRepository> _mockRepository;
    private readonly Mock<ILogger<GroupsController>> _mockLogger;
    private readonly GroupsController _controller;

    public GroupsControllerTests()
    {
        _mockRepository = new Mock<IScimRepository>();
        _mockLogger = new Mock<ILogger<GroupsController>>();
        _controller = new GroupsController(_mockRepository.Object, _mockLogger.Object);
        
        // Configurer l'authentification pour les tests
        AuthenticationTestHelper.SetupAuthenticatedContext(_controller);
    }

    #region GetGroups Tests

    [Fact]
    public async Task GetGroups_WithNoFilter_ShouldReturnAllGroups()
    {
        // Arrange
        var groups = new ScimListResponse<ScimGroup>
        {
            TotalResults = 2,
            StartIndex = 1,
            ItemsPerPage = 2,
            Resources = new List<ScimGroup>
            {
                new ScimGroup { Id = "1", DisplayName = "Administrators" },
                new ScimGroup { Id = "2", DisplayName = "Developers" }
            }
        };

        _mockRepository
            .Setup(r => r.GetGroupsAsync(null, 1, 100))
            .ReturnsAsync(groups);

        // Act
        var result = await _controller.GetGroups(null);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimListResponse<ScimGroup>>();
        var returnedGroups = (ScimListResponse<ScimGroup>)okResult.Value!;
        returnedGroups.TotalResults.ShouldBe(2);
        returnedGroups.Resources.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetGroups_WithFilter_ShouldReturnFilteredGroups()
    {
        // Arrange
        var filter = "displayName eq \"Administrators\"";
        var groups = new ScimListResponse<ScimGroup>
        {
            TotalResults = 1,
            Resources = new List<ScimGroup>
            {
                new ScimGroup { Id = "1", DisplayName = "Administrators" }
            }
        };

        _mockRepository
            .Setup(r => r.GetGroupsAsync(It.IsAny<FilterExpression>(), 1, 100))
            .ReturnsAsync(groups);

        // Act
        var result = await _controller.GetGroups(filter);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimListResponse<ScimGroup>>();
        var returnedGroups = (ScimListResponse<ScimGroup>)okResult.Value!;
        returnedGroups.TotalResults.ShouldBe(1);
    }

    [Fact]
    public async Task GetGroups_WhenException_ShouldReturn500()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetGroupsAsync(It.IsAny<FilterExpression>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetGroups(null);

        // Assert
        result.ShouldBeOfType<ObjectResult>();
        var statusCodeResult = (ObjectResult)result;
        statusCodeResult.StatusCode.ShouldBe(500);
        statusCodeResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)statusCodeResult.Value!;
        error.Status.ShouldBe(500);
    }

    #endregion

    #region GetGroup Tests

    [Fact]
    public async Task GetGroup_WhenExists_ShouldReturnGroup()
    {
        // Arrange
        var groupId = "123";
        var group = new ScimGroup { Id = groupId, DisplayName = "Administrators" };

        _mockRepository
            .Setup(r => r.GetGroupAsync(groupId))
            .ReturnsAsync(group);

        // Act
        var result = await _controller.GetGroup(groupId);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimGroup>();
        var returnedGroup = (ScimGroup)okResult.Value!;
        returnedGroup.Id.ShouldBe(groupId);
    }

    [Fact]
    public async Task GetGroup_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var groupId = "non-existent";

        _mockRepository
            .Setup(r => r.GetGroupAsync(groupId))
            .ReturnsAsync((ScimGroup?)null);

        // Act
        var result = await _controller.GetGroup(groupId);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)notFoundResult.Value!;
        error.Status.ShouldBe(404);
    }

    #endregion

    #region CreateGroup Tests

    [Fact]
    public async Task CreateGroup_WhenValid_ShouldReturnCreated()
    {
        // Arrange
        var newGroup = new ScimGroup { DisplayName = "New Group" };
        var createdGroup = new ScimGroup
        {
            Id = "123",
            DisplayName = "New Group",
            Meta = new ScimMeta { ResourceType = "Group" }
        };

        _mockRepository
            .Setup(r => r.GetGroupByDisplayNameAsync(newGroup.DisplayName))
            .ReturnsAsync((ScimGroup?)null);

        _mockRepository
            .Setup(r => r.CreateGroupAsync(It.IsAny<ScimGroup>()))
            .ReturnsAsync(createdGroup);

        // Act
        var result = await _controller.CreateGroup(newGroup);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.Value.ShouldBeOfType<ScimGroup>();
        var returnedGroup = (ScimGroup)createdResult.Value!;
        returnedGroup.DisplayName.ShouldBe("New Group");
    }

    [Fact]
    public async Task CreateGroup_WhenAlreadyExists_ShouldReturn409()
    {
        // Arrange
        var newGroup = new ScimGroup { DisplayName = "Existing Group" };
        var existingGroup = new ScimGroup { Id = "123", DisplayName = "Existing Group" };

        _mockRepository
            .Setup(r => r.GetGroupByDisplayNameAsync(newGroup.DisplayName))
            .ReturnsAsync(existingGroup);

        // Act
        var result = await _controller.CreateGroup(newGroup);

        // Assert
        result.ShouldBeOfType<ConflictObjectResult>();
        var conflictResult = (ConflictObjectResult)result;
        conflictResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)conflictResult.Value!;
        error.Status.ShouldBe(409);
    }

    #endregion

    #region UpdateGroup Tests

    [Fact]
    public async Task UpdateGroup_WhenExists_ShouldReturnUpdatedGroup()
    {
        // Arrange
        var groupId = "123";
        var updatedGroup = new ScimGroup { DisplayName = "Updated Group" };
        var resultGroup = new ScimGroup
        {
            Id = groupId,
            DisplayName = "Updated Group",
            Meta = new ScimMeta { LastModified = DateTime.UtcNow }
        };

        _mockRepository
            .Setup(r => r.UpdateGroupAsync(groupId, updatedGroup))
            .ReturnsAsync(resultGroup);

        // Act
        var result = await _controller.UpdateGroup(groupId, updatedGroup);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimGroup>();
        var returnedGroup = (ScimGroup)okResult.Value!;
        returnedGroup.DisplayName.ShouldBe("Updated Group");
    }

    [Fact]
    public async Task UpdateGroup_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var groupId = "non-existent";
        var updatedGroup = new ScimGroup { DisplayName = "Test Group" };

        _mockRepository
            .Setup(r => r.UpdateGroupAsync(groupId, updatedGroup))
            .ReturnsAsync((ScimGroup?)null);

        // Act
        var result = await _controller.UpdateGroup(groupId, updatedGroup);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)notFoundResult.Value!;
        error.Status.ShouldBe(404);
    }

    #endregion

    #region PatchGroup Tests

    [Fact]
    public async Task PatchGroup_AddMember_ShouldReturnUpdatedGroup()
    {
        // Arrange
        var groupId = "123";
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
                        new Dictionary<string, string> { { "value", "user-id-123" } }
                    }
                }
            }
        };

        var updatedGroup = new ScimGroup
        {
            Id = groupId,
            DisplayName = "Test Group",
            Members = new List<ScimMember>
            {
                new ScimMember { Value = "user-id-123" }
            }
        };

        _mockRepository
            .Setup(r => r.PatchGroupAsync(groupId, patchRequest))
            .ReturnsAsync(updatedGroup);

        // Act
        var result = await _controller.PatchGroup(groupId, patchRequest);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimGroup>();
        var returnedGroup = (ScimGroup)okResult.Value!;
        returnedGroup.Members.Count.ShouldBe(1);
    }

    [Fact]
    public async Task PatchGroup_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var groupId = "non-existent";
        var patchRequest = new ScimPatchRequest { Operations = new List<ScimPatchOperation>() };

        _mockRepository
            .Setup(r => r.PatchGroupAsync(groupId, patchRequest))
            .ReturnsAsync((ScimGroup?)null);

        // Act
        var result = await _controller.PatchGroup(groupId, patchRequest);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)notFoundResult.Value!;
        error.Status.ShouldBe(404);
    }

    #endregion

    #region DeleteGroup Tests

    [Fact]
    public async Task DeleteGroup_WhenExists_ShouldReturn204()
    {
        // Arrange
        var groupId = "123";

        _mockRepository
            .Setup(r => r.DeleteGroupAsync(groupId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteGroup(groupId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteGroup_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var groupId = "non-existent";

        _mockRepository
            .Setup(r => r.DeleteGroupAsync(groupId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteGroup(groupId);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)notFoundResult.Value!;
        error.Status.ShouldBe(404);
    }

    #endregion
}

