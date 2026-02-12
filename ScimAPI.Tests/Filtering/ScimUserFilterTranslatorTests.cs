using ScimAPI.Filtering;
using ScimAPI.Filtering.AST;
using ScimAPI.Models;
using Shouldly;
using Xunit;

namespace ScimAPI.Tests.Filtering
{
    /// <summary>
    /// Tests for ScimUserFilterTranslator - AST to LINQ Expression conversion.
    /// </summary>
    public class ScimUserFilterTranslatorTests
    {
        private readonly ScimUserFilterTranslator _translator;
        private readonly List<ScimUser> _users;

        public ScimUserFilterTranslatorTests()
        {
            _translator = new ScimUserFilterTranslator();
            _users = new List<ScimUser>
            {
                new ScimUser
                {
                    Id = "1",
                    UserName = "john.doe@example.com",
                    DisplayName = "John Doe",
                    Active = true,
                    Name = new ScimName { GivenName = "John", FamilyName = "Doe" }
                },
                new ScimUser
                {
                    Id = "2",
                    UserName = "jane.smith@example.com",
                    DisplayName = "Jane Smith",
                    Active = true,
                    Name = new ScimName { GivenName = "Jane", FamilyName = "Smith" }
                },
                new ScimUser
                {
                    Id = "3",
                    UserName = "bob.inactive@example.com",
                    DisplayName = "Bob Inactive",
                    Active = false,
                    Name = new ScimName { GivenName = "Bob", FamilyName = "Inactive" }
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
        public void Apply_NullFilter_ReturnsOriginalQueryable()
        {
            // Arrange
            var query = _users.AsQueryable();

            // Act
            var result = _translator.Apply(query, null);

            // Assert
            result.Count().ShouldBe(3);
        }

        [Fact]
        public void BuildPredicate_EqualsFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter("userName", FilterOperator.Equals, new StringValue("john.doe@example.com"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].UserName.ShouldBe("john.doe@example.com");
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
            result[0].UserName.ShouldBe("john.doe@example.com");
        }

        [Fact]
        public void BuildPredicate_BooleanEquals_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter("active", FilterOperator.Equals, new BooleanValue(true));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(2);
            result.ShouldAllBe(u => u.Active);
        }

        [Fact]
        public void BuildPredicate_ContainsFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter("userName", FilterOperator.Contains, new StringValue("jane"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].UserName.ShouldContain("jane");
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
            result[0].UserName.ShouldStartWith("john");
        }

        [Fact]
        public void BuildPredicate_NestedProperty_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter("name.givenName", FilterOperator.Equals, new StringValue("John"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].Name.GivenName.ShouldBe("John");
        }

        [Fact]
        public void BuildPredicate_PresenceFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new PresenceFilter("displayName");

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(3); // All users have displayName
        }

        [Fact]
        public void BuildPredicate_AndFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new AndFilter(
                new ComparisonFilter("userName", FilterOperator.Contains, new StringValue("example.com")),
                new ComparisonFilter("active", FilterOperator.Equals, new BooleanValue(true))
            );

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _users.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(2);
            result.ShouldAllBe(u => u.Active && u.UserName.Contains("example.com"));
        }

        [Fact]
        public void BuildPredicate_OrFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new OrFilter(
                new ComparisonFilter("userName", FilterOperator.StartsWith, new StringValue("john")),
                new ComparisonFilter("userName", FilterOperator.StartsWith, new StringValue("jane"))
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
            result[0].Active.ShouldBeFalse();
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
            result.ShouldAllBe(u => u.Active);
        }
    }
}

