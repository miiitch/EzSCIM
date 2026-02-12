using ScimAPI.Constants;
using ScimAPI.Filtering;
using ScimAPI.Filtering.AST;
using ScimAPI.Models;
using Shouldly;
using Xunit;

namespace ScimAPI.Tests.Filtering
{
    /// <summary>
    /// Tests for ScimGroupFilterTranslator - AST to LINQ Expression conversion.
    /// </summary>
    public class ScimGroupFilterTranslatorTests
    {
        private readonly ScimGroupFilterTranslator _translator;
        private readonly List<ScimGroup> _groups;

        public ScimGroupFilterTranslatorTests()
        {
            _translator = new ScimGroupFilterTranslator();
            _groups = new List<ScimGroup>
            {
                new ScimGroup
                {
                    Id = "1",
                    DisplayName = "Developers",
                    ExternalId = "dev-group-001"
                },
                new ScimGroup
                {
                    Id = "2",
                    DisplayName = "Managers",
                    ExternalId = "mgr-group-001"
                },
                new ScimGroup
                {
                    Id = "3",
                    DisplayName = "Administrators",
                    ExternalId = "admin-group-001"
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
            var query = _groups.AsQueryable();

            // Act
            var result = _translator.Apply(query, null);

            // Assert
            result.Count().ShouldBe(3);
        }

        [Fact]
        public void BuildPredicate_EqualsFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter(
                ScimAttributeNames.Group.DisplayName, 
                FilterOperator.Equals, 
                new StringValue("Developers"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].DisplayName.ShouldBe("Developers");
        }

        [Fact]
        public void BuildPredicate_EqualsFilter_IsCaseInsensitive()
        {
            // Arrange
            var filter = new ComparisonFilter(
                ScimAttributeNames.Group.DisplayName, 
                FilterOperator.Equals, 
                new StringValue("DEVELOPERS"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].DisplayName.ShouldBe("Developers");
        }

        [Fact]
        public void BuildPredicate_ContainsFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter(
                ScimAttributeNames.Group.DisplayName, 
                FilterOperator.Contains, 
                new StringValue("admin"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].DisplayName.ShouldContain("Admin");
        }

        [Fact]
        public void BuildPredicate_StartsWithFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter(
                ScimAttributeNames.Group.DisplayName, 
                FilterOperator.StartsWith, 
                new StringValue("Man"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].DisplayName.ShouldStartWith("Man");
        }

        [Fact]
        public void BuildPredicate_EndsWithFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter(
                ScimAttributeNames.Group.DisplayName, 
                FilterOperator.EndsWith, 
                new StringValue("pers"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].DisplayName.ShouldEndWith("pers");
        }

        [Fact]
        public void BuildPredicate_PresenceFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new PresenceFilter(ScimAttributeNames.Common.ExternalId);

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(3); // All groups have externalId
        }

        [Fact]
        public void BuildPredicate_AndFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new AndFilter(
                new ComparisonFilter(ScimAttributeNames.Group.DisplayName, FilterOperator.Contains, new StringValue("er")),
                new ComparisonFilter(ScimAttributeNames.Common.ExternalId, FilterOperator.StartsWith, new StringValue("dev"))
            );

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(1);
            result[0].DisplayName.ShouldBe("Developers");
        }

        [Fact]
        public void BuildPredicate_OrFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new OrFilter(
                new ComparisonFilter(ScimAttributeNames.Group.DisplayName, FilterOperator.Equals, new StringValue("Developers")),
                new ComparisonFilter(ScimAttributeNames.Group.DisplayName, FilterOperator.Equals, new StringValue("Managers"))
            );

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(2);
        }

        [Fact]
        public void BuildPredicate_NotFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new NotFilter(
                new ComparisonFilter(ScimAttributeNames.Group.DisplayName, FilterOperator.Equals, new StringValue("Developers"))
            );

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(2);
            result.ShouldAllBe(g => g.DisplayName != "Developers");
        }

        [Fact]
        public void BuildPredicate_ComplexFilter_WorksCorrectly()
        {
            // Arrange: (displayName sw "Dev" or displayName sw "Man") and externalId pr
            var filter = new AndFilter(
                new OrFilter(
                    new ComparisonFilter(ScimAttributeNames.Group.DisplayName, FilterOperator.StartsWith, new StringValue("Dev")),
                    new ComparisonFilter(ScimAttributeNames.Group.DisplayName, FilterOperator.StartsWith, new StringValue("Man"))
                ),
                new PresenceFilter(ScimAttributeNames.Common.ExternalId)
            );

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(2);
        }

        [Fact]
        public void BuildPredicate_NotEqualsFilter_WorksCorrectly()
        {
            // Arrange
            var filter = new ComparisonFilter(
                ScimAttributeNames.Group.DisplayName, 
                FilterOperator.NotEquals, 
                new StringValue("Developers"));

            // Act
            var predicate = _translator.BuildPredicate(filter);
            var result = _groups.AsQueryable().Where(predicate!).ToList();

            // Assert
            result.Count.ShouldBe(2);
            result.ShouldAllBe(g => g.DisplayName != "Developers");
        }
    }
}

