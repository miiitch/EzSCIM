using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ScimAPI.Controllers;
using ScimAPI.Models;
using ScimAPI.Repositories;
using Shouldly;

namespace ScimAPI.Tests;

public class UsersControllerTests
{
    private readonly Mock<IScimRepository> _mockRepository;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockRepository = new Mock<IScimRepository>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockRepository.Object, _mockLogger.Object);
    }

    #region GetUsers Tests

    [Fact]
    public async Task GetUsers_WithNoFilter_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new ScimListResponse<ScimUser>
        {
            TotalResults = 2,
            StartIndex = 1,
            ItemsPerPage = 2,
            Resources = new List<ScimUser>
            {
                new ScimUser { Id = "1", UserName = "user1@example.com" },
                new ScimUser { Id = "2", UserName = "user2@example.com" }
            }
        };

        _mockRepository
            .Setup(r => r.GetUsersAsync(null, 1, 100))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetUsers(null);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimListResponse<ScimUser>>();
        var returnedUsers = (ScimListResponse<ScimUser>)okResult.Value!;
        returnedUsers.TotalResults.ShouldBe(2);
        returnedUsers.Resources.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetUsers_WithFilter_ShouldReturnFilteredUsers()
    {
        // Arrange
        var filter = "userName eq \"test@example.com\"";
        var users = new ScimListResponse<ScimUser>
        {
            TotalResults = 1,
            Resources = new List<ScimUser>
            {
                new ScimUser { Id = "1", UserName = "test@example.com" }
            }
        };

        _mockRepository
            .Setup(r => r.GetUsersAsync(filter, 1, 100))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetUsers(filter);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimListResponse<ScimUser>>();
        var returnedUsers = (ScimListResponse<ScimUser>)okResult.Value!;
        returnedUsers.TotalResults.ShouldBe(1);
    }

    [Fact]
    public async Task GetUsers_WithPagination_ShouldReturnPaginatedUsers()
    {
        // Arrange
        var users = new ScimListResponse<ScimUser>
        {
            TotalResults = 100,
            StartIndex = 11,
            ItemsPerPage = 10,
            Resources = new List<ScimUser>()
        };

        _mockRepository
            .Setup(r => r.GetUsersAsync(null, 11, 10))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetUsers(null, 11, 10);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimListResponse<ScimUser>>();
        var returnedUsers = (ScimListResponse<ScimUser>)okResult.Value!;
        returnedUsers.StartIndex.ShouldBe(11);
        returnedUsers.ItemsPerPage.ShouldBe(10);
    }

    [Fact]
    public async Task GetUsers_WhenException_ShouldReturn500()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetUsersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUsers(null);

        // Assert
        result.ShouldBeOfType<ObjectResult>();
        var statusCodeResult = (ObjectResult)result;
        statusCodeResult.StatusCode.ShouldBe(500);
        statusCodeResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)statusCodeResult.Value!;
        error.Status.ShouldBe(500);
    }

    #endregion

    #region GetUser Tests

    [Fact]
    public async Task GetUser_WhenExists_ShouldReturnUser()
    {
        // Arrange
        var userId = "123";
        var user = new ScimUser { Id = userId, UserName = "test@example.com" };

        _mockRepository
            .Setup(r => r.GetUserAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimUser>();
        var returnedUser = (ScimUser)okResult.Value!;
        returnedUser.Id.ShouldBe(userId);
    }

    [Fact]
    public async Task GetUser_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var userId = "non-existent";

        _mockRepository
            .Setup(r => r.GetUserAsync(userId))
            .ReturnsAsync((ScimUser?)null);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)notFoundResult.Value!;
        error.Status.ShouldBe(404);
    }

    #endregion

    #region CreateUser Tests

    [Fact]
    public async Task CreateUser_WhenValid_ShouldReturnCreated()
    {
        // Arrange
        var newUser = new ScimUser { UserName = "newuser@example.com" };
        var createdUser = new ScimUser
        {
            Id = "123",
            UserName = "newuser@example.com",
            Meta = new ScimMeta { ResourceType = "User" }
        };

        _mockRepository
            .Setup(r => r.GetUserByUserNameAsync(newUser.UserName))
            .ReturnsAsync((ScimUser?)null);

        _mockRepository
            .Setup(r => r.CreateUserAsync(It.IsAny<ScimUser>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(newUser);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.Value.ShouldBeOfType<ScimUser>();
        var returnedUser = (ScimUser)createdResult.Value!;
        returnedUser.UserName.ShouldBe("newuser@example.com");
    }

    [Fact]
    public async Task CreateUser_WhenAlreadyExists_ShouldReturn409()
    {
        // Arrange
        var newUser = new ScimUser { UserName = "existing@example.com" };
        var existingUser = new ScimUser { Id = "123", UserName = "existing@example.com" };

        _mockRepository
            .Setup(r => r.GetUserByUserNameAsync(newUser.UserName))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.CreateUser(newUser);

        // Assert
        result.ShouldBeOfType<ConflictObjectResult>();
        var conflictResult = (ConflictObjectResult)result;
        conflictResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)conflictResult.Value!;
        error.Status.ShouldBe(409);
    }

    #endregion

    #region UpdateUser Tests

    [Fact]
    public async Task UpdateUser_WhenExists_ShouldReturnUpdatedUser()
    {
        // Arrange
        var userId = "123";
        var updatedUser = new ScimUser { UserName = "updated@example.com" };
        var resultUser = new ScimUser
        {
            Id = userId,
            UserName = "updated@example.com",
            Meta = new ScimMeta { LastModified = DateTime.UtcNow }
        };

        _mockRepository
            .Setup(r => r.UpdateUserAsync(userId, updatedUser))
            .ReturnsAsync(resultUser);

        // Act
        var result = await _controller.UpdateUser(userId, updatedUser);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimUser>();
        var returnedUser = (ScimUser)okResult.Value!;
        returnedUser.UserName.ShouldBe("updated@example.com");
    }

    [Fact]
    public async Task UpdateUser_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var userId = "non-existent";
        var updatedUser = new ScimUser { UserName = "test@example.com" };

        _mockRepository
            .Setup(r => r.UpdateUserAsync(userId, updatedUser))
            .ReturnsAsync((ScimUser?)null);

        // Act
        var result = await _controller.UpdateUser(userId, updatedUser);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)notFoundResult.Value!;
        error.Status.ShouldBe(404);
    }

    #endregion

    #region PatchUser Tests

    [Fact]
    public async Task PatchUser_WhenValid_ShouldReturnUpdatedUser()
    {
        // Arrange
        var userId = "123";
        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation { Op = "replace", Path = "active", Value = false }
            }
        };

        var updatedUser = new ScimUser
        {
            Id = userId,
            UserName = "test@example.com",
            Active = false
        };

        _mockRepository
            .Setup(r => r.PatchUserAsync(userId, patchRequest))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.PatchUser(userId, patchRequest);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.ShouldBeOfType<ScimUser>();
        var returnedUser = (ScimUser)okResult.Value!;
        returnedUser.Active.ShouldBeFalse();
    }

    [Fact]
    public async Task PatchUser_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var userId = "non-existent";
        var patchRequest = new ScimPatchRequest { Operations = new List<ScimPatchOperation>() };

        _mockRepository
            .Setup(r => r.PatchUserAsync(userId, patchRequest))
            .ReturnsAsync((ScimUser?)null);

        // Act
        var result = await _controller.PatchUser(userId, patchRequest);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)notFoundResult.Value!;
        error.Status.ShouldBe(404);
    }

    #endregion

    #region DeleteUser Tests

    [Fact]
    public async Task DeleteUser_WhenExists_ShouldReturn204()
    {
        // Arrange
        var userId = "123";

        _mockRepository
            .Setup(r => r.DeleteUserAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteUser_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var userId = "non-existent";

        _mockRepository
            .Setup(r => r.DeleteUserAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.ShouldBeOfType<ScimError>();
        var error = (ScimError)notFoundResult.Value!;
        error.Status.ShouldBe(404);
    }

    #endregion
}

