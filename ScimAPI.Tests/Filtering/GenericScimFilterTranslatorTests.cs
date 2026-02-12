using ScimAPI.Attributes;
using ScimAPI.Examples;
using ScimAPI.Filtering;
using ScimAPI.Filtering.AST;
using Shouldly;
using Xunit;

namespace ScimAPI.Tests.Filtering
{
    /// <summary>
    /// Tests for GenericScimFilterTranslator with custom user types.
    /// </summary>
    public class GenericScimFilterTranslatorTests
    {
        private readonly GenericScimFilterTranslator<CustomUser> _translator;
        private readonly List<CustomUser> _users;

        public GenericScimFilterTranslatorTests()
        {
            _translator = new GenericScimFilterTranslator<CustomUser>();
            _users = new List<CustomUser>
            {
                new CustomUser
                {
                    Id = "1",
                    Username = "john.doe@example.com",
                    Email = "john.doe@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    DisplayName = "John Doe",
                    IsActive = true,
                    JobTitle = "Developer"
                },
                new CustomUser
                {
                    Id = "2",
                    Username = "jane.smith@example.com",
                    Email = "jane.smith@example.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    DisplayName = "Jane Smith",
                    IsActive = true,
                    JobTitle = "Manager"
                },
                new CustomUser
                {
                    Id = "3",
                    Username = "bob.inactive@example.com",
                    Email = "bob.inactive@example.com",
                    FirstName = "Bob",
                    LastName = "Inactive",
                    DisplayName = "Bob Inactive",
                    IsActive = false,
                    JobTitle = "Consultant"
                }
            };
        }

        [Fact]
        public void BuildPredicate_NullFilter_ReturnsNull()
        {
            // Act
            var predicate = _translator.BuildPredicate(null);

            // Assert
            predicate.ShouldBeNull();
        }

        [Fact]
        public void BuildPredicate_EqualsFilter_WorksWithScimPropertyAttribute()
        {
            // Arrange - userName maps to Username property via [ScimProperty]
            var filter = new ComparisonFilter("userName", FilterOperator.Equals, new StringValue("john.doe@example.com"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].Username.ShouldBe("john.doe@example.com");
        }

        [Fact]
        public void BuildPredicate_EqualsFilter_IsCaseInsensitive()
        {
            // Arrange
            var filter = new ComparisonFilter("userName", FilterOperator.Equals, new StringValue("JOHN.DOE@EXAMPLE.COM"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].Username.ShouldBe("john.doe@example.com");
        }

        [Fact]
        public void BuildPredicate_BooleanEquals_WorksCorrectly()
        {
            // Arrange - active maps to IsActive property
            var filter = new ComparisonFilter("active", FilterOperator.Equals, new BooleanValue(true));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(2);
            result.ShouldAllBe(u => u.IsActive);
        }

        [Fact]
        public void BuildPredicate_ContainsFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter("displayName", FilterOperator.Contains, new StringValue("Jane"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].DisplayName.ShouldContain("Jane");
        }

        [Fact]
        public void BuildPredicate_StartsWithFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter("userName", FilterOperator.StartsWith, new StringValue("john"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].Username.ShouldStartWith("john");
        }

        [Fact]
        public void BuildPredicate_EndsWithFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter("userName", FilterOperator.EndsWith, new StringValue("example.com"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(3);
        }

        [Fact]
        public void BuildPredicate_PresenceFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new PresenceFilter("email");

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(3); // All users have email
        }

        [Fact]
        public void BuildPredicate_AndFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new AndFilter(
                new ComparisonFilter("title", FilterOperator.Equals, new StringValue("Developer")),
                new ComparisonFilter("active", FilterOperator.Equals, new BooleanValue(true))
            );

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].JobTitle.ShouldBe("Developer");
            result[0].IsActive.ShouldBeTrue();
        }

        [Fact]
        public void BuildPredicate_OrFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new OrFilter(
                new ComparisonFilter("givenName", FilterOperator.Equals, new StringValue("John")),
                new ComparisonFilter("givenName", FilterOperator.Equals, new StringValue("Jane"))
            );

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(2);
        }

        [Fact]
        public void BuildPredicate_NotFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new NotFilter(
                new ComparisonFilter("active", FilterOperator.Equals, new BooleanValue(true))
            );

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].IsActive.ShouldBeFalse();
        }

        [Fact]
        public void BuildPredicate_ComplexFilter_WorksCorrectly()
        {
            // Arrange: (userName sw "john" or userName sw "jane") and active eq true
            var filter = new AndFilter(
                new OrFilter(
                    new ComparisonFilter("userName", FilterOperator.StartsWith, new StringValue("john")),
                    new ComparisonFilter("userName", FilterOperator.StartsWith, new StringValue("jane"))
                ),
                new ComparisonFilter("active", FilterOperator.Equals, new BooleanValue(true))
            );

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(2);
            result.ShouldAllBe(u => u.IsActive);
        }

        [Fact]
        public void BuildPredicate_UnknownProperty_ReturnsFalse()
        {
            // Arrange
            var filter = new ComparisonFilter("unknownProperty", FilterOperator.Equals, new StringValue("test"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(0); // No matches for unknown property
        }
    }
}

