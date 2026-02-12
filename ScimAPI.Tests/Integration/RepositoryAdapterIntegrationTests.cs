using ScimAPI.Examples;
using ScimAPI.Filtering;
using ScimAPI.Filtering.AST;
using ScimAPI.Models;
using ScimAPI.Repositories;
using Shouldly;
using Xunit;

namespace ScimAPI.Tests.Integration
{
    /// <summary>
    /// End-to-end integration test: CustomUser repository → SCIM adapter → filtering.
    /// Tests the complete workflow from custom repository to SCIM API.
    /// </summary>
    public class RepositoryAdapterIntegrationTests
    {
        private readonly CustomUserRepository _dataRepository;
        private readonly GenericScimFilterTranslator<CustomUser> _translator;
        private readonly ScimUserRepositoryAdapter<CustomUser> _adapter;

        public RepositoryAdapterIntegrationTests()
        {
            // Setup the complete integration chain
            _dataRepository = new CustomUserRepository();
            _translator = new GenericScimFilterTranslator<CustomUser>();
            _adapter = new ScimUserRepositoryAdapter<CustomUser>(_dataRepository, _translator);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var users = new[]
            {
                new CustomUser
                {
                    Username = "john.doe@example.com",
                    Email = "john.doe@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    DisplayName = "John Doe",
                    IsActive = true,
                    JobTitle = "Software Engineer"
                },
                new CustomUser
                {
                    Username = "jane.smith@example.com",
                    Email = "jane.smith@example.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    DisplayName = "Jane Smith",
                    IsActive = true,
                    JobTitle = "Senior Developer"
                },
                new CustomUser
                {
                    Username = "bob.inactive@example.com",
                    Email = "bob.inactive@example.com",
                    FirstName = "Bob",
                    LastName = "Inactive",
                    DisplayName = "Bob Inactive",
                    IsActive = false,
                    JobTitle = "Consultant"
                },
                new CustomUser
                {
                    Username = "alice.manager@example.com",
                    Email = "alice.manager@example.com",
                    FirstName = "Alice",
                    LastName = "Manager",
                    DisplayName = "Alice Manager",
                    IsActive = true,
                    JobTitle = "Engineering Manager"
                }
            };

            foreach (var user in users)
            {
                _dataRepository.CreateAsync(user).Wait();
            }
        }

        [Fact]
        public async Task GetUsersAsync_NoFilter_ReturnsAllUsers()
        {
            // Act
            var result = await _adapter.GetUsersAsync();

            // Assert
            result.ShouldNotBeNull();
            result.TotalResults.ShouldBe(4);
            result.Resources.Count.ShouldBe(4);
        }

        [Fact]
        public async Task GetUsersAsync_FilterByUserName_ReturnsMatchingUser()
        {
            // Arrange
            var filter = new ComparisonFilter("userName", FilterOperator.Equals, new StringValue("john.doe@example.com"));

            // Act
            var result = await _adapter.GetUsersAsync(filter);

            // Assert
            result.TotalResults.ShouldBe(1);
            result.Resources[0].UserName.ShouldBe("john.doe@example.com");
        }

        [Fact]
        public async Task GetUsersAsync_FilterByActive_ReturnsActiveUsers()
        {
            // Arrange
            var filter = new ComparisonFilter("active", FilterOperator.Equals, new BooleanValue(true));

            // Act
            var result = await _adapter.GetUsersAsync(filter);

            // Assert
            result.TotalResults.ShouldBe(3);
            result.Resources.ShouldAllBe(u => u.Active);
        }

        [Fact]
        public async Task GetUsersAsync_FilterByGivenName_ReturnsMatchingUsers()
        {
            // Arrange
            var filter = new ComparisonFilter("givenName", FilterOperator.StartsWith, new StringValue("J"));

            // Act
            var result = await _adapter.GetUsersAsync(filter);

            // Assert
            result.TotalResults.ShouldBe(2); // John and Jane
            result.Resources.Count.ShouldBe(2);
            foreach (var user in result.Resources)
            {
                user.Name.GivenName.ShouldNotBeNull();
                user.Name.GivenName.ShouldStartWith("J");
            }
        }

        [Fact]
        public async Task GetUsersAsync_ComplexFilter_WorksCorrectly()
        {
            // Arrange: (givenName sw "J") and (active eq true)
            var filter = new AndFilter(
                new ComparisonFilter("givenName", FilterOperator.StartsWith, new StringValue("J")),
                new ComparisonFilter("active", FilterOperator.Equals, new BooleanValue(true))
            );

            // Act
            var result = await _adapter.GetUsersAsync(filter);

            // Assert
            result.TotalResults.ShouldBe(2); // John and Jane (both active)
            result.Resources.ShouldAllBe(u => u.Active);
        }

        [Fact]
        public async Task GetUsersAsync_OrFilter_WorksCorrectly()
        {
            // Arrange: (title co "Engineer") or (title co "Manager")
            var filter = new OrFilter(
                new ComparisonFilter("title", FilterOperator.Contains, new StringValue("Engineer")),
                new ComparisonFilter("title", FilterOperator.Contains, new StringValue("Manager"))
            );

            // Act
            var result = await _adapter.GetUsersAsync(filter);

            // Assert
            result.TotalResults.ShouldBe(2); // Software Engineer, Engineering Manager
        }

        [Fact]
        public async Task GetUsersAsync_WithPagination_ReturnsCorrectPage()
        {
            // Act
            var result = await _adapter.GetUsersAsync(null, startIndex: 2, count: 2);

            // Assert
            result.TotalResults.ShouldBe(4);
            result.ItemsPerPage.ShouldBe(2);
            result.StartIndex.ShouldBe(2);
            result.Resources.Count.ShouldBe(2);
        }

        [Fact]
        public async Task GetUserAsync_ValidId_ReturnsUser()
        {
            // Arrange
            var created = await _dataRepository.CreateAsync(new CustomUser
            {
                Username = "test@example.com",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IsActive = true
            });

            // Act
            var result = await _adapter.GetUserAsync(created.Id);

            // Assert
            result.ShouldNotBeNull();
            result.UserName.ShouldBe("test@example.com");
        }

        [Fact]
        public async Task GetUserByUserNameAsync_ValidUserName_ReturnsUser()
        {
            // Act
            var result = await _adapter.GetUserByUserNameAsync("john.doe@example.com");

            // Assert
            result.ShouldNotBeNull();
            result.UserName.ShouldBe("john.doe@example.com");
            result.Name.GivenName.ShouldBe("John");
        }

        [Fact]
        public async Task CreateUserAsync_ValidUser_CreatesSuccessfully()
        {
            // Arrange
            var newUser = new ScimUser
            {
                UserName = "new.user@example.com",
                Name = new ScimName { GivenName = "New", FamilyName = "User" },
                DisplayName = "New User",
                Active = true,
                Title = "Developer"
            };

            // Act
            var result = await _adapter.CreateUserAsync(newUser);

            // Assert
            result.ShouldNotBeNull();
            result.UserName.ShouldBe("new.user@example.com");
            result.Id.ShouldNotBeNullOrEmpty();

            // Verify it was actually created
            var retrieved = await _adapter.GetUserAsync(result.Id);
            retrieved.ShouldNotBeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_ValidUser_UpdatesSuccessfully()
        {
            // Arrange
            var created = await _dataRepository.CreateAsync(new CustomUser
            {
                Username = "update@example.com",
                Email = "update@example.com",
                FirstName = "Original",
                IsActive = true
            });

            var updatedUser = new ScimUser
            {
                UserName = "update@example.com",
                Name = new ScimName { GivenName = "Updated", FamilyName = "User" },
                Active = true
            };

            // Act
            var result = await _adapter.UpdateUserAsync(created.Id, updatedUser);

            // Assert
            result.ShouldNotBeNull();
            result.Name.GivenName.ShouldBe("Updated");
        }

        [Fact]
        public async Task DeleteUserAsync_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var created = await _dataRepository.CreateAsync(new CustomUser
            {
                Username = "delete@example.com",
                Email = "delete@example.com",
                IsActive = true
            });

            // Act
            var result = await _adapter.DeleteUserAsync(created.Id);

            // Assert
            result.ShouldBeTrue();

            // Verify it was actually deleted
            var retrieved = await _adapter.GetUserAsync(created.Id);
            retrieved.ShouldBeNull();
        }

        [Fact]
        public async Task EndToEnd_FilterCreateUpdateDelete_WorksCorrectly()
        {
            // 1. Create a user
            var newUser = new ScimUser
            {
                UserName = "e2e@example.com",
                Name = new ScimName { GivenName = "E2E", FamilyName = "Test" },
                Active = true
            };

            var created = await _adapter.CreateUserAsync(newUser);
            created.Id.ShouldNotBeNullOrEmpty();

            // 2. Filter to find it
            var filter = new ComparisonFilter("userName", FilterOperator.Equals, new StringValue("e2e@example.com"));
            var filterResult = await _adapter.GetUsersAsync(filter);
            filterResult.TotalResults.ShouldBe(1);

            // 3. Update it
            created.Name.GivenName = "Updated";
            var updated = await _adapter.UpdateUserAsync(created.Id, created);
            updated.ShouldNotBeNull();
            updated.Name.GivenName.ShouldBe("Updated");

            // 4. Verify update via filter
            var verifyFilter = new ComparisonFilter("givenName", FilterOperator.Equals, new StringValue("Updated"));
            var verifyResult = await _adapter.GetUsersAsync(verifyFilter);
            verifyResult.TotalResults.ShouldBeGreaterThan(0);

            // 5. Delete it
            var deleted = await _adapter.DeleteUserAsync(created.Id);
            deleted.ShouldBeTrue();

            // 6. Verify deletion via filter
            var deletedFilter = new ComparisonFilter("userName", FilterOperator.Equals, new StringValue("e2e@example.com"));
            var deletedResult = await _adapter.GetUsersAsync(deletedFilter);
            deletedResult.TotalResults.ShouldBe(0);
        }
    }
}




